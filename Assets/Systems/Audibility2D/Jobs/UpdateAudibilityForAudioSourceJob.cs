using Systems.Audibility2D.Data;
using Systems.Audibility2D.Utility;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;

namespace Systems.Audibility2D.Jobs
{
    [BurstCompile]
    public struct UpdateAudibilityForAudioSourceJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<AudioSource2DComputeData> audioSourcesData;
        [NativeDisableParallelForRestriction] public NativeArray<AudioTile2DComputeData> audioTilesData;
        
        [BurstCompile]
        public void Execute(int nAudioSource)
        {
            NativeList<int> tilesToUpdateNeighbours = new(64, Allocator.Temp);
            AudioSource2DComputeData audioSourceData = audioSourcesData[nAudioSource];

            // Skip if tile is outside of map
            if (Hint.Unlikely(audioSourceData.tileIndex >= audioTilesData.Length || audioSourceData.tileIndex < 0)) return;
            
            // Get start tile and initialize with audio value
            AudioTile2DComputeData startTile = audioTilesData[audioSourceData.tileIndex];
            AudibilityLevel.UpdateAudioLevelForTile(ref tilesToUpdateNeighbours, ref startTile, audioSourceData,
                audioSourceData.audioLevel);
            audioTilesData[audioSourceData.tileIndex] = startTile;

            // Perform update sequence
            while (Hint.Likely(tilesToUpdateNeighbours.Length > 0))
            {
                // Perform update sequence
                int tileIndex = tilesToUpdateNeighbours[0];
                AudioTile2DComputeData tile = audioTilesData[tileIndex];
                AudibilityLevel.UpdateNeighbourAudioLevelsForTile(
                    ref tilesToUpdateNeighbours,
                    ref audioTilesData, ref tile, audioSourceData);

                // Remove tiles from update
                tilesToUpdateNeighbours.RemoveAt(0);
            }

            tilesToUpdateNeighbours.Dispose();
        }
    }
}