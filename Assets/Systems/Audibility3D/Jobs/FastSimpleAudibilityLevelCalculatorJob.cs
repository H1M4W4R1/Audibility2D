using System.Runtime.CompilerServices;
using Systems.Audibility.Common.Data;
using Systems.Audibility.Common.Utility;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility3D.Jobs
{
    /// <summary>
    ///     Job used to calculate audio level based on performed raycast data
    /// </summary>
    [BurstCompile]
    public struct FastSimpleAudibilityLevelCalculatorJob : IJobParallelFor
    {
        /// <summary>
        ///     Array of performed commands
        /// </summary>
        [ReadOnly] public NativeArray<RaycastCommand> raycastCommands;
        
        /// <summary>
        ///     Array of results (found objects)
        /// </summary>
        [ReadOnly] public NativeArray<RaycastHit> raycastResults;
        
        /// <summary>
        ///     Array of audio source max distances
        /// </summary>
        [ReadOnly] public NativeArray<float> sourceRanges;
        
        /// <summary>
        ///     Maximum hits per command
        /// </summary>
        [ReadOnly] public int raycastMaxHits;

        /// <summary>
        ///     Array of scanned levels to perform update on
        /// </summary>
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