using System.Runtime.CompilerServices;
using Systems.Audibility.Common.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Systems.Audibility3D.Jobs
{
    /// <summary>
    ///     Job to reset scanned audio levels to default value
    /// </summary>
    [BurstCompile]
    public struct ResetScannedAudioLevelsJob : IJobParallelFor
    {
        /// <summary>
        ///     Array of audio levels to reset
        /// </summary>
        [WriteOnly] public NativeArray<DecibelLevel> scannedLevels;
        
        /// <summary>
        ///     Array of source audio levels to use as base value
        /// </summary>
        [ReadOnly] public NativeArray<DecibelLevel> sourceDecibelLevels;
        
        /// <summary>
        ///     Count of sources
        /// </summary>
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