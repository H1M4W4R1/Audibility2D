using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Systems.Audibility3D.Jobs
{
    [BurstCompile]
    public struct ResetRaycastResultTableJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<RaycastHit> raycastHits;
        
        [BurstCompile]
        public void Execute(int index)
        {
            raycastHits[index] = default;
        }
    }
}