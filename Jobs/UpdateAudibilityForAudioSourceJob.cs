using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Utility;
using Systems.Utilities.Indexing.Grid;
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
        ///     Tilemap being scanned
        /// </summary>
        [ReadOnly] public GridInfo3D tilemapInfo;
        
        /// <summary>
        ///     Audio sources information for computation
        /// </summary>
        [ReadOnly] public NativeArray<AudioSourceInfo> audioSourcesData;
        
        /// <summary>
        ///     Audio tiles data, can be written (hope it won't cause race conditions)
        /// </summary>
        [NativeDisableParallelForRestriction] public NativeArray<AudioTileInfo> audioTilesData;
        
        [BurstCompile]
        public void Execute(int nAudioSource)
        {
            NativeList<int> tilesToUpdateNeighbours = new(64, Allocator.Temp);
            AudioSourceInfo audioSourceInfo = audioSourcesData[nAudioSource];

            // Skip if tile is outside of map
            if (Hint.Unlikely(audioSourceInfo.tileIndex >= audioTilesData.Length || audioSourceInfo.tileIndex < 0)) return;
            
            // Get start tile and initialize with audio value
            AudioTileInfo startTile = audioTilesData[audioSourceInfo.tileIndex];
            AudibilityTools.UpdateAudioLevelForTile(tilemapInfo, ref tilesToUpdateNeighbours, 
                startTile, ref startTile, audioSourceInfo,
                audioSourceInfo.audioLevel);
            audioTilesData[audioSourceInfo.tileIndex] = startTile;

            // Perform update sequence
            while (Hint.Likely(tilesToUpdateNeighbours.Length > 0))
            {
                // Perform update sequence
                int tileIndex = tilesToUpdateNeighbours[0];
                AudioTileInfo tile = audioTilesData[tileIndex];
                AudibilityTools.UpdateNeighbourAudioLevelsForTile(tilemapInfo,
                    ref tilesToUpdateNeighbours,
                    ref audioTilesData, ref tile, audioSourceInfo);

                // Remove tiles from update
                tilesToUpdateNeighbours.RemoveAt(0);
            }

            tilesToUpdateNeighbours.Dispose();
        }
    }
}