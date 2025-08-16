using Systems.Audibility.Common.Data;
using Systems.Audibility.Common.Utility;
using Systems.Audibility2D.Data;
using Systems.Audibility2D.Jobs;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Systems.Audibility2D.Utility
{
    /// <summary>
    ///     Utility class to calculate audibility level from provided data
    /// </summary>
    public static class AudibilityLevel2D
    {
        /// <summary>
        ///     Update audibility level
        /// </summary>
        [BurstCompile] public static void UpdateAudibilityLevel(
            in NativeArray<AudioSource2DComputeData> audioSourcesData,
            ref NativeArray<AudioTile2DComputeData> audioTilesData)
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
            ref NativeArray<AudioTile2DComputeData> audioTilesData,
            ref AudioTile2DComputeData currentTile,
            in AudioSource2DComputeData currentAudioSource)
        {
            for (int neighbourId = 0; neighbourId <= Tile2DNeighbourIndexData.MAX_INDEX; neighbourId++)
            {
                // Get tile index
                int neighbourTileIndex = currentTile.neighbourData[neighbourId];
                
                // Early return
                if (neighbourTileIndex == -1) break;
                
                // Process tile
                AudioTile2DComputeData neighbourTile = audioTilesData[neighbourTileIndex];
                UpdateAudioLevelForTile(ref tilesToUpdateNeighbours, ref neighbourTile, currentAudioSource,
                    currentTile.currentAudioLevel);
                audioTilesData[neighbourTileIndex] = neighbourTile;
            }
        }

        /// <summary>
        ///     Method used to update audio level of specific tile
        /// </summary>
        [BurstCompile] internal static void UpdateAudioLevelForTile(
            ref NativeList<int> tilesToUpdateNeighbours,
            ref AudioTile2DComputeData neighbouringTile,
            in AudioSource2DComputeData currentAudioSource,
            in DecibelLevel localTileDecibelLevel)
        {
            // Compute distance between tile and audio source (straight line)
            float distanceSq = math.distancesq(neighbouringTile.worldPosition, currentAudioSource.worldPosition);

            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
            DecibelLevel newTileLevel = localTileDecibelLevel.MuffleBy(neighbouringTile.mufflingStrength);
            newTileLevel = newTileLevel.MuffleAllFrequenciesBy((byte) math.lerp(0, Loudness.MAX,
                math.clamp(distanceSq / currentAudioSource.rangeSq, 0, 1)));
            newTileLevel = DecibelLevel.Max(newTileLevel, neighbouringTile.currentAudioLevel);

            if (!Hint.Unlikely(neighbouringTile.currentAudioLevel != newTileLevel)) return;

            // Update audio level based on maximum between current level and new one calculated by muffling values
            neighbouringTile.currentAudioLevel = newTileLevel;

            // Notify to update neighbours
            if (Hint.Likely(!tilesToUpdateNeighbours.Contains(neighbouringTile.index)))
                tilesToUpdateNeighbours.Add(neighbouringTile.index);
        }
    }
}