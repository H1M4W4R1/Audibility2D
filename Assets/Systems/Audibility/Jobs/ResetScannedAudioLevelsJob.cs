using System.Runtime.CompilerServices;
using Systems.Audibility.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Systems.Audibility.Jobs
{
    [BurstCompile]
    public struct ResetScannedAudioLevelsJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<DecibelLevel> scannedLevels;
        [ReadOnly] public NativeArray<DecibelLevel> sourceDecibelLevels;
        [ReadOnly] public int sourcesCount;
        
        [BurstCompile]
        public void Execute(int nSample)
        {
            // Cache as lambda shit
            int nSources = sourcesCount;
            
            scannedLevels[nSample] = sourceDecibelLevels[GetSourceIndex(nSample)];
            return;
            
            [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
            int GetSourceIndex(int sampleIndex) => sampleIndex % nSources;
        }
    }
}