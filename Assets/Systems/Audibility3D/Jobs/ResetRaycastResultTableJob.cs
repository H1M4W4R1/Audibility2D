using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Systems.Audibility3D.Jobs
{
    /// <summary>
    ///     Job to reset raycast hit array...
    /// </summary>
    [BurstCompile]
    public struct ResetRaycastResultTableJob : IJobParallelFor
    {
        /// <summary>
        ///     Array to reset
        /// </summary>
        [WriteOnly] public NativeArray<RaycastHit> raycastHits;
        
        [BurstCompile]
        public void Execute(int index)
        {
            raycastHits[index] = default;
        }
    }
}