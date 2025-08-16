using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Utility;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;

namespace Systems.Audibility2D.Jobs
{
    /// <summary>
    ///     Job used to efficiently update audibility level of all tiles
    ///     looping for all audio sources in parallel to make it way faster
    ///     than it should be.
    /// </summary>
    [BurstCompile]
    public struct UpdateAudibilityForAudioSourceJob : IJobParallelFor
    {
        /// <summary>
        ///     Audio sources information for computation
        /// </summary>
        [ReadOnly] public NativeArray<AudioSourceData> audioSourcesData;
        
        /// <summary>
        ///     Audio tiles data, can be written (hope it won't cause race conditions)
        /// </summary>
        [NativeDisableParallelForRestriction] public NativeArray<AudioTileData> audioTilesData;
        
        [BurstCompile]
        public void Execute(int nAudioSource)
        {
            NativeList<int> tilesToUpdateNeighbours = new(64, Allocator.Temp);
            AudioSourceData audioSourceData = audioSourcesData[nAudioSource];

            // Skip if tile is outside of map
            if (Hint.Unlikely(audioSourceData.tileIndex >= audioTilesData.Length || audioSourceData.tileIndex < 0)) return;
            
            // Get start tile and initialize with audio value
            AudioTileData startTile = audioTilesData[audioSourceData.tileIndex];
            AudibilityLevel.UpdateAudioLevelForTile(ref tilesToUpdateNeighbours, 
                startTile, ref startTile, audioSourceData,
                audioSourceData.audioLevel);
            audioTilesData[audioSourceData.tileIndex] = startTile;

            // Perform update sequence
            while (Hint.Likely(tilesToUpdateNeighbours.Length > 0))
            {
                // Perform update sequence
                int tileIndex = tilesToUpdateNeighbours[0];
                AudioTileData tile = audioTilesData[tileIndex];
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