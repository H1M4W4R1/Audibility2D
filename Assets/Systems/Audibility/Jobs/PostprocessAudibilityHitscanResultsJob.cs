using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility.Jobs
{
    [BurstCompile]
    public struct PostprocessAudibilityHitscanResultsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> audioSourcePositions;
        [ReadOnly] public NativeArray<float> audioSourceVolumes;
        [ReadOnly] public NativeArray<float> audioSourceRanges;
        [ReadOnly] public NativeArray<RaycastHit> results;
        [NativeDisableParallelForRestriction] public NativeArray<float> audioLevels;

        public NativeArray<float> rayLengths;
        public NativeArray<RaycastCommand> commands;
        public AudibilityPlane plane;
        public int nDirections;

        [BurstCompile]
        public void Execute(int commandIndex)
        {
            // Prepare data
            float3 endPosition;
            RaycastCommand command = commands[commandIndex];
            RaycastHit detectedHit = results[commandIndex];
            float rayLength = rayLengths[commandIndex];

            // Move command index into audio sample index
            int sampleIndex = commandIndex / nDirections;


            // Handle both cases for collision detection
            if (detectedHit.point == default)
            {
                endPosition = command.from + command.direction * AudioUtilities.MAX_RAY_DISTANCE;
                rayLength += AudioUtilities.MAX_RAY_DISTANCE;
            }
            else
            {
                float3 newDirection = math.reflect(command.direction, detectedHit.normal);
                endPosition = detectedHit.point;

                // Override direction vectors to prevent reflections from angular surfaces in 2D mode
                switch (plane)
                {
                    case AudibilityPlane.PlaneXY: newDirection.z = 0; break;
                    case AudibilityPlane.PlaneXZ: newDirection.y = 0; break;
                    case AudibilityPlane.Euler3D:
                    default: break;
                }

                // Update command data
                command.direction = newDirection;
                rayLength += detectedHit.distance;
            }

            // Compute and update audio level based on positioning
            float audioLevel = AudioUtilities.ComputeInverseAudioLevelNearRay(audioSourcePositions,
                audioSourceVolumes, audioSourceRanges, command.from,
                endPosition, rayLength);
            if (audioLevel < audioLevels[sampleIndex]) audioLevels[sampleIndex] = audioLevel;

            command.from = endPosition;
            commands[commandIndex] = command;
            rayLengths[commandIndex] = rayLength;
        }
    }
}