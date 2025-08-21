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
        ///     Audibility system settings
        /// </summary>
        [ReadOnly] public AudioSystemSettings audibilitySettings;
        
        /// <summary>
        ///     Tilemap being scanned
        /// </summary>
        [ReadOnly] public GridInfo2D tilemapInfo;
        
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
            AudibilityTools.UpdateAudioLevelForTile(audibilitySettings, ref tilesToUpdateNeighbours, ref startTile,
                audioSourceInfo.audioLevel, 0);
            audioTilesData[audioSourceInfo.tileIndex] = startTile;

            // Perform update sequence
            while (Hint.Likely(tilesToUpdateNeighbours.Length > 0))
            {
                // Perform update sequence
                int tileIndex = tilesToUpdateNeighbours[0];
                AudioTileInfo tile = audioTilesData[tileIndex];
                AudibilityTools.UpdateNeighbourAudioLevelsForTile(
                    audibilitySettings,
                    tilemapInfo,
                    ref tilesToUpdateNeighbours,
                    ref audioTilesData, ref tile);

                // Remove tiles from update
                tilesToUpdateNeighbours.RemoveAt(0);
            }

            tilesToUpdateNeighbours.Dispose();
        }
    }
}