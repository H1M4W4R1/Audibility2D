using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Data.Native.Wrappers;
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
    public struct GetAverageAudioLoudnessDataJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<AudioTileInfo> tileData;
        [WriteOnly] public NativeArray<AudioLoudnessLevel> averageTileLoudnessData;
        
        [BurstCompile]
        public void Execute(int index)
        {
            averageTileLoudnessData[index] = tileData[index].currentAudioLevel.GetValue();
        }
    }
}