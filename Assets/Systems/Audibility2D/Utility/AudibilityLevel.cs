using JetBrains.Annotations;
using Systems.Audibility2D.Components;
using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Data.Native.Wrappers;
using Systems.Audibility2D.Data.Tiles;
using Systems.Audibility2D.Jobs;
using Systems.Audibility2D.Utility.Internal;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Utility
{
    /// <summary>
    ///     Utility class to calculate audibility level from provided data
    /// </summary>
    [BurstCompile] public static class AudibilityLevel
    {
        public const int LOUDNESS_NONE = 0;
        public const int LOUDNESS_MAX = 160;

        /// <summary>
        ///     Simpler version to handle audibility calculations
        /// </summary>
        /// <param name="audioTilemap">
        ///     Instance of tilemap to calculate audio data from, should contain <see cref="AudioTile"/> objects
        /// </param>
        /// <param name="audioSourceComputeData">
        ///     Reference to handle for Audio Source Data array, automatically allocated as PERSISTENT
        /// </param>
        /// <param name="tileComputeData">
        ///     Reference to handle for Tile Data array, automatically allocated as PERSISTENT
        ///     Also your output array.
        /// </param>
        [BurstDiscard] public static void UpdateAudibilityLevel(
            [NotNull] Tilemap audioTilemap,
            ref NativeArray<AudioSourceInfo> audioSourceComputeData,
            ref NativeArray<AudioTileInfo> tileComputeData)
        {
            TilemapInfo tilemapInfo = new(audioTilemap);
            
            // Initialize tilemap arrays
            AudibilityTools.TilemapToArray(audioTilemap, ref tileComputeData);

            // Prepare array of audio sources
            AudibleSound[] sources =
                Object.FindObjectsByType<AudibleSound>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            QuickArray.PerformEfficientAllocation(ref audioSourceComputeData, sources.Length,
                Allocator.Persistent);

            // Get audio sources data
            AudibilityTools.AudioSourcesToArray(audioTilemap, sources, ref audioSourceComputeData);

            // Handle computation
            UpdateAudibilityLevel(tilemapInfo, audioSourceComputeData, ref tileComputeData);
        }

        /// <summary>
        ///     Update audibility level for entire map
        /// </summary>
        ///  ///
        ///  <param name="tilemapInfo">Tilemap info to update data for</param>
        ///  <param name="audioSourceComputeData">
        ///     Handle for Audio Source Data array, must be filled with proper data
        /// </param>
        /// <param name="tileComputeData">
        ///     Reference to handle for Tile Data array, must be filled with proper data
        ///     Also your output array.
        /// </param>
        [BurstCompile] public static void UpdateAudibilityLevel(
            in TilemapInfo tilemapInfo,
            in NativeArray<AudioSourceInfo> audioSourceComputeData,
            ref NativeArray<AudioTileInfo> tileComputeData)
        {
            UpdateAudibilityForAudioSourceJob updateAudibilityJob = new()
            {
                tilemapInfo = tilemapInfo,
                audioSourcesData = audioSourceComputeData,
                audioTilesData = tileComputeData
            };

            updateAudibilityJob
                .Schedule(audioSourceComputeData.Length, math.min(audioSourceComputeData.Length, 64))
                .Complete();
        }

        /// <summary>
        ///     Method that computes audio updates for all neighboring tiles of specific tile
        /// </summary>
        /// <param name="tilemapInfo">Tilemap info to compute data for</param>
        /// <param name="tilesToUpdateNeighbours">
        ///     Reference to list containing indices of tiles which should have their neighbours updated
        /// </param>
        /// <param name="audioTilesData">
        ///     Reference to handle for Tile Data array, must be filled with proper data.
        /// </param>
        /// <param name="currentTile">
        ///     Reference to tile which neighbours should be checked.
        /// </param>
        /// <param name="currentAudioSource">
        ///     Current audio source that is being analyzed
        /// </param>
        [BurstCompile] internal static void UpdateNeighbourAudioLevelsForTile(
            in TilemapInfo tilemapInfo,
            ref NativeList<int> tilesToUpdateNeighbours,
            ref NativeArray<AudioTileInfo> audioTilesData,
            ref AudioTileInfo currentTile,
            in AudioSourceInfo currentAudioSource)
        {
            for (int neighbourId = 0; neighbourId <= AudioTileNeighboursInfo.MAX_INDEX; neighbourId++)
            {
                // Get tile index
                int neighbourTileIndex = currentTile.GetNeighbourIndex(neighbourId);

                // Early return
                if (Hint.Unlikely(neighbourTileIndex == -1)) break;

                // Process tile
                AudioTileInfo neighbourTile = audioTilesData[neighbourTileIndex];
                UpdateAudioLevelForTile(
                    tilemapInfo, ref tilesToUpdateNeighbours,
                    currentTile, ref neighbourTile, currentAudioSource, currentTile.currentAudioLevel);
                audioTilesData[neighbourTileIndex] = neighbourTile;
            }
        }

        /// <summary>
        ///     Method used to update audio level of specific tile
        /// </summary>
        /// <param name="tilemapInfo">Tilemap info to compute data for</param>
        /// <param name="tilesToUpdateNeighbours">
        ///     Reference to list containing indices of tiles which should have their neighbours updated
        /// </param>
        /// <param name="originalTile">
        ///     Handle to tile update origin (aka. tile which updates current one)
        /// </param>
        /// <param name="neighbouringTile">
        ///     Reference to tile that should be updated. For source tiles should be same tile
        ///     as <see cref="originalTile"/>.
        /// </param>
        /// <param name="currentAudioSource">
        ///     Current audio source that is being analyzed
        /// </param>
        /// <param name="currentAudioLevel">
        ///     Audio level in current tile, passed separately to handle source tiles correctly
        /// </param> 
        [BurstCompile] internal static void UpdateAudioLevelForTile(
            in TilemapInfo tilemapInfo,
            ref NativeList<int> tilesToUpdateNeighbours,
            in AudioTileInfo originalTile,
            ref AudioTileInfo neighbouringTile,
            in AudioSourceInfo currentAudioSource,
            in AudioLoudnessLevel currentAudioLevel)
        {
            // Get positions
            float3 originalTilePosition = originalTile.index.GetWorldPosition(tilemapInfo);
            float3 neighbouringTilePosition = neighbouringTile.index.GetWorldPosition(tilemapInfo);
            
            // Compute distance between tiles to decrease audio level
            // Unfortunately we can't use distanceSq as it behaves poorly in division scenarios
            float distance = math.distance(originalTilePosition, neighbouringTilePosition);

            // This will always result in silence, skip this trash ;)
            if (Hint.Likely(distance > currentAudioSource.range)) return;

            // Copy current audio level and compute muffling 
            AudioLoudnessLevel newTileLevel = currentAudioLevel;
            newTileLevel = newTileLevel.MuffleBy(neighbouringTile.mufflingStrength); // Current tile muffling
            newTileLevel = newTileLevel.MuffleBy(math.lerp(0, LOUDNESS_MAX,
                math.clamp(distance / currentAudioSource.range, 0, 1)));
            newTileLevel = AudioLoudnessLevel.Max(newTileLevel, neighbouringTile.currentAudioLevel);

            // Detect audio changes to prevent infinite loop
            if (!Hint.Unlikely(neighbouringTile.currentAudioLevel != newTileLevel)) return;

            // Update audio level based on maximum between current level and new one calculated by muffling values
            neighbouringTile.currentAudioLevel = newTileLevel;

            // Notify to update neighbours
            if (Hint.Likely(!tilesToUpdateNeighbours.Contains(neighbouringTile.index.value)))
                tilesToUpdateNeighbours.Add(neighbouringTile.index.value);
        }
    }
}