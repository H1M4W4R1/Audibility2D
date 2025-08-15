using System.Runtime.CompilerServices;
using Systems.Audibility.Data;
using Systems.Audibility.Utility;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility.Jobs
{
    [BurstCompile]
    public struct FastSimpleAudibilityLevelCalculatorJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RaycastCommand> raycastCommands;
        [ReadOnly] public NativeArray<RaycastHit> raycastResults;
        [ReadOnly] public NativeArray<float> sourceRanges;
        [ReadOnly] public int raycastMaxHits;

        public NativeArray<DecibelLevel> scannedLevels;

        [BurstCompile]
        public void Execute(int nSample)
        {
            int nSources = sourceRanges.Length;
            
            // Handle distance check
            float distance = raycastCommands[nSample].distance;
            float maxDistance = sourceRanges[GetSourceIndex(nSample)];

            // Prevent distance going over max to improve performance by reduction
            // of unnecessary computation
            if (Hint.Likely(distance > maxDistance))
            {
                scannedLevels[nSample] = Loudness.SILENCE;
                return;
            }

            // Handle all results for this sample
            for (int nResult = 0; nResult < raycastMaxHits; nResult++)
            {
                // Acquire hit data
                RaycastHit hit = raycastResults[nSample * raycastMaxHits + nResult];
                if (Hint.Unlikely(hit.colliderInstanceID == 0)) continue;

                // Muffle sound if obstacle present
                scannedLevels[nSample] = scannedLevels[nSample].MuffleBy(Muffling.CONCRETE);
            }

            scannedLevels[nSample] = scannedLevels[nSample]
                .MuffleAllFrequenciesBy((byte) math.lerp(0, Loudness.MAX,
                    math.clamp(distance / maxDistance, 0, 1)));
            return;
            
            [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] int GetSourceIndex(int sampleIndex) => sampleIndex % nSources;
        }
    }
}