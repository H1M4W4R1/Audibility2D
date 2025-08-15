using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility.Jobs
{
    [BurstCompile] public struct CreateAudioSamplingRaycastsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> points;
        [ReadOnly] public NativeArray<float3> sourcePositions;
        [ReadOnly] public QueryParameters raycastParameters;
        [WriteOnly] public NativeArray<RaycastCommand> raycastCommands;

        [BurstCompile] public void Execute(int nSample)
        {
            int nSources = sourcePositions.Length;

            float3 atPosition = points[GetPointIndex(nSample)];

            float3 sourcePosition = sourcePositions[GetSourceIndex(nSample)];
            float3 direction = math.normalize(atPosition - sourcePosition);
            float distance = math.distance(atPosition, sourcePosition);

            RaycastCommand command = new()
            {
                from = sourcePosition,
                direction = direction,
                distance = distance,
                queryParameters = raycastParameters,
                physicsScene = Physics.defaultPhysicsScene
            };

            raycastCommands[nSample] = command;
            return;

            [BurstCompile]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            int GetSourceIndex(int sampleIndex) => sampleIndex % nSources;

            [BurstCompile]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            int GetPointIndex(int sampleIndex) => sampleIndex / nSources;
        }
    }
}