using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Systems.Audibility2D.Jobs
{
    /// <summary>
    ///     Job used to quickly convert tile data after computation into loudness levels
    ///     Created because Unity API sucks
    /// </summary>
    [BurstCompile]
    public struct GetDebugAudioTileDataJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<AudioTileData> tileData;
        [WriteOnly] public NativeArray<AudioTileDebugData> audioTileDebugData;
        
        [BurstCompile]
        public void Execute(int index)
        {
            AudioTileData tile = tileData[index];
            float normalizedLoudness = tile.currentAudioLevel.GetAverage() / (float) AudibilityLevel.LOUDNESS_MAX;
            audioTileDebugData[index] = new AudioTileDebugData(tile.worldPosition, normalizedLoudness);
        }
    }
}