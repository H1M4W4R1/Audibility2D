using JetBrains.Annotations;
using Systems.Audibility2D.Components;
using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Jobs;
using Systems.Audibility2D.Tiles;
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
            ref NativeArray<AudioSourceData> audioSourceComputeData,
            ref NativeArray<AudioTileData> tileComputeData)
        {
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
            UpdateAudibilityLevel(audioSourceComputeData, ref tileComputeData);
        }

        /// <summary>
        ///     Update audibility level for entire map
        /// </summary>
        ///  /// <param name="audioSourceComputeData">
        ///     Handle for Audio Source Data array, must be filled with proper data
        /// </param>
        /// <param name="tileComputeData">
        ///     Reference to handle for Tile Data array, must be filled with proper data
        ///     Also your output array.
        /// </param>
        [BurstCompile] public static void UpdateAudibilityLevel(
            in NativeArray<AudioSourceData> audioSourceComputeData,
            ref NativeArray<AudioTileData> tileComputeData)
        {
            UpdateAudibilityForAudioSourceJob updateAudibilityJob = new()
            {
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
            ref NativeList<int> tilesToUpdateNeighbours,
            ref NativeArray<AudioTileData> audioTilesData,
            ref AudioTileData currentTile,
            in AudioSourceData currentAudioSource)
        {
            for (int neighbourId = 0; neighbourId <= AudioTileNeighbourData.MAX_INDEX; neighbourId++)
            {
                // Get tile index
                int neighbourTileIndex = currentTile.GetNeighbourIndex(neighbourId);

                // Early return
                if (Hint.Unlikely(neighbourTileIndex == -1)) break;

                // Process tile
                AudioTileData neighbourTile = audioTilesData[neighbourTileIndex];
                UpdateAudioLevelForTile(ref tilesToUpdateNeighbours,
                    currentTile, ref neighbourTile, currentAudioSource, currentTile.currentAudioLevel);
                audioTilesData[neighbourTileIndex] = neighbourTile;
            }
        }

        /// <summary>
        ///     Method used to update audio level of specific tile
        /// </summary>
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
            ref NativeList<int> tilesToUpdateNeighbours,
            in AudioTileData originalTile,
            ref AudioTileData neighbouringTile,
            in AudioSourceData currentAudioSource,
            in AudioLoudnessLevel currentAudioLevel)
        {
            // Compute distance between tiles to decrease audio level
            // Unfortunately we can't use distanceSq as it behaves poorly in division scenarios
            float distance = math.distance(neighbouringTile.worldPosition, originalTile.worldPosition);

            // This will always result in silence, skip this trash ;)
            if (Hint.Likely(distance > currentAudioSource.range)) return;

            // Copy current audio level and compute muffling 
            AudioLoudnessLevel newTileLevel = currentAudioLevel;
            newTileLevel = newTileLevel.MuffleBy(neighbouringTile.mufflingStrength); // Current tile muffling
            newTileLevel = newTileLevel.MuffleAllFrequenciesBy((byte) math.lerp(0, LOUDNESS_MAX,
                math.clamp(distance / currentAudioSource.range, 0, 1)));
            newTileLevel = AudioLoudnessLevel.Max(newTileLevel, neighbouringTile.currentAudioLevel);

            // Detect audio changes to prevent infinite loop
            if (!Hint.Unlikely(neighbouringTile.currentAudioLevel != newTileLevel)) return;

            // Update audio level based on maximum between current level and new one calculated by muffling values
            neighbouringTile.currentAudioLevel = newTileLevel;

            // Notify to update neighbours
            if (Hint.Likely(!tilesToUpdateNeighbours.Contains(neighbouringTile.index)))
                tilesToUpdateNeighbours.Add(neighbouringTile.index);
        }
    }
}