using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Utility;
using Systems.Utilities.Indexing.Grid;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Systems.Audibility2D.Jobs
{
    /// <summary>
    ///     Job used to quickly convert tile data after computation into loudness levels
    ///     Created because Unity API sucks
    /// </summary>
    [BurstCompile] public struct GetDebugAudioTileDataJob : IJobParallelFor
    {
        [ReadOnly] public GridInfo3D tilemapInfo;
        [ReadOnly] public NativeArray<AudioTileInfo> tileData;
        [WriteOnly] public NativeArray<AudioTileDebugInfo> audioTileDebugData;

        [BurstCompile] public void Execute(int index)
        {
            AudioTileInfo tile = tileData[index];
            float normalizedLoudness = tile.currentAudioLevel.GetAverage() / (float) AudibilityTools.LOUDNESS_MAX;
            audioTileDebugData[index] =
                new AudioTileDebugInfo(tile.index.GetWorldPosition(tilemapInfo), normalizedLoudness);
        }
    }
}