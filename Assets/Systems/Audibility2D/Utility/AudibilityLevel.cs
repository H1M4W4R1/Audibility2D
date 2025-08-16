using JetBrains.Annotations;
using Systems.Audibility2D.Components;
using Systems.Audibility2D.Data;
using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Jobs;
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
    [BurstCompile]
    public static class AudibilityLevel
    {
        /// <summary>
        ///     Simpler version to handle audibility calculations
        /// </summary>
        [BurstDiscard]
        public static void UpdateAudibilityLevel(
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
        [BurstCompile] public static void UpdateAudibilityLevel(
            in NativeArray<AudioSourceData> audioSourcesData,
            ref NativeArray<AudioTileData> audioTilesData)
        {
            UpdateAudibilityForAudioSourceJob updateAudibilityJob = new()
            {
                audioSourcesData = audioSourcesData,
                audioTilesData = audioTilesData
            };

            updateAudibilityJob.Schedule(audioSourcesData.Length, math.min(audioSourcesData.Length, 64))
                .Complete();
        }

        /// <summary>
        ///     Method that computes audio updates for all neighboring tiles of specific tile
        /// </summary>
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
            newTileLevel = newTileLevel.MuffleAllFrequenciesBy((byte) math.lerp(0, Loudness.MAX,
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