using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Systems.Audibility
{
    [BurstCompile] public static class AudioUtilities
    {
        public const int MAX_REFLECTIONS = 5;
        public const int RAYS_PER_CIRCLE = 18;
        public const float MAX_RAY_DISTANCE = 50f;

        [BurstDiscard] [MethodImpl(MethodImplOptions.AggressiveInlining)] [NotNull]
        public static float[] GetAudibilityLevel(
            [NotNull] in float3[] worldPoint,
            in AudibilityPlane plane = AudibilityPlane.PlaneXZ)
        {
            // Get all known audio sources
            AudioSource[] audioSources =
                Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            NativeArray<float3> samplingPositions = new(worldPoint, Allocator.TempJob);
            NativeArray<float3> audioSourcePositions = new(audioSources.Length, Allocator.TempJob);
            NativeArray<float> audioSourceVolumes = new(audioSources.Length, Allocator.TempJob);
            NativeArray<float> audioSourceRanges = new(audioSources.Length, Allocator.TempJob);

            // Copy source data
            for (int n = 0; n < audioSources.Length; n++)
            {
                audioSourcePositions[n] = audioSources[n].transform.localPosition;
                audioSourceVolumes[n] = audioSources[n].volume;
                audioSourceRanges[n] = math.pow(audioSources[n].maxDistance, 2);
            }

            // Prepare output table
            NativeArray<float> outputAudioLevels = new(samplingPositions.Length, Allocator.Temp);
            for (int n = 0; n < outputAudioLevels.Length; n++) outputAudioLevels[n] = float.MaxValue;

            GetAudibility2D(samplingPositions, audioSourcePositions, audioSourceVolumes, audioSourceRanges,
                ref outputAudioLevels,
                AudibilityPlane.PlaneXZ);

            // Copy data
            float[] results = new float[outputAudioLevels.Length];
            for (int n = 0; n < outputAudioLevels.Length; n++) results[n] = outputAudioLevels[n];

            samplingPositions.Dispose();
            audioSourcePositions.Dispose();
            audioSourceVolumes.Dispose();
            outputAudioLevels.Dispose();

            return results;
        }

        [BurstDiscard] private static void GetAudibility3D(
            in NativeArray<float3> samplingPositions,
            in NativeArray<float3> audioSourcePositions,
            in NativeArray<float> audioSourceVolumes,
            in NativeArray<float> audioSourceRanges,
            ref NativeArray<float> audioLevels)
        {
            NativeArray<float3> directionArray = new(RAYS_PER_CIRCLE, Allocator.TempJob);

            // Compute directions for all desired scan points
            for (int nWorldPoint = 0; nWorldPoint < samplingPositions.Length; nWorldPoint++)
            {
                // For spiral loop around sphere
                for (int nRay = 0; nRay < RAYS_PER_CIRCLE; nRay++)
                {
                    float tAngle = math.PI2 * (nRay / (float) RAYS_PER_CIRCLE);

                    float xDirection = math.cos(RAYS_PER_CIRCLE * tAngle) * math.sin(tAngle);
                    float yDirection = math.sin(tAngle) * math.cos(tAngle);
                    float zDirection = math.cos(tAngle);

                    float3 direction = new(xDirection, yDirection, zDirection);

                    directionArray[nRay] = direction;
                }
            }

            // Calculate audio levels
            PerformAudioReflectionCalculations(samplingPositions, directionArray,
                audioSourcePositions, audioSourceVolumes, audioSourceRanges, ref audioLevels,
                AudibilityPlane.Euler3D);

            // Clean-up this mess
            directionArray.Dispose();
        }

        [BurstDiscard] private static void GetAudibility2D(
            in NativeArray<float3> samplingPositions,
            in NativeArray<float3> audioSourcePositions,
            in NativeArray<float> audioSourceVolumes,
            in NativeArray<float> audioSourceRanges,
            ref NativeArray<float> audioLevels,
            AudibilityPlane plane)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            NativeArray<float3> directionArray = new(RAYS_PER_CIRCLE, Allocator.TempJob);

            // Compute directions for all desired scan points
            for (int nWorldPoint = 0; nWorldPoint < samplingPositions.Length; nWorldPoint++)
            {
                // For all directions around circle
                for (int nRay = 0; nRay < RAYS_PER_CIRCLE; nRay++)
                {
                    // Compute direction vector
                    float tAngle = math.PI2 * (nRay / (float) RAYS_PER_CIRCLE);

                    float xDirection = math.cos(tAngle);
                    float yzDirection = math.sin(tAngle);

                    float3 direction = plane == AudibilityPlane.PlaneXY
                        ? new float3(xDirection, yzDirection, 0)
                        : new float3(xDirection, 0, yzDirection);

                    directionArray[nRay] = direction;
                }
            }

            // Calculate audio levels
            PerformAudioReflectionCalculations(samplingPositions, directionArray,
                audioSourcePositions, audioSourceVolumes, audioSourceRanges, ref audioLevels, plane);

            // Clean-up this mess
            directionArray.Dispose();
            sw.Stop();
            
            Debug.Log(nameof(GetAudibility2D) + $" took {sw.ElapsedMilliseconds}ms");
        }

        [BurstDiscard] private static void PerformAudioReflectionCalculations(
            in NativeArray<float3> samplingPositions,
            in NativeArray<float3> directions,
            in NativeArray<float3> audioSourcePositions,
            in NativeArray<float> audioSourceVolumes,
            in NativeArray<float> audioSourceRanges,
            ref NativeArray<float> audioLevels,
            AudibilityPlane plane)
        {
            // Prevent length mismatch
            if (samplingPositions.Length != audioLevels.Length)
            {
                Debug.LogError(
                    $"[{nameof(AudioUtilities)}] Audio levels count is invalid!");

                // Clear array
                for (int n = 0; n < audioLevels.Length; n++) audioLevels[n] = -1;
                return;
            }

            int nCommands = samplingPositions.Length * directions.Length;

            NativeArray<RaycastCommand> commands = new(nCommands, Allocator.TempJob);
            NativeArray<RaycastHit> results = new(nCommands, Allocator.TempJob);

            int reflectionsRemaining = MAX_REFLECTIONS;


            // Initially prepare commands
            for (int positionIndex = 0; positionIndex < samplingPositions.Length; positionIndex++)
            {
                for (int directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    commands[positionIndex * directions.Length + directionIndex] = new RaycastCommand(
                        samplingPositions[positionIndex],
                        directions[directionIndex],
                        QueryParameters.Default);
                }
            }

            // Loop until no reflections are left
            while (reflectionsRemaining > 0)
            {
                JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 1);
                handle.Complete();

                // Handle result for all scanning directions
                HandleResults(audioSourcePositions, audioSourceVolumes, audioSourceRanges, ref commands, results,
                    ref audioLevels, plane, directions.Length);

                // Reduce reflection count
                reflectionsRemaining--;
            }

            // Clear data
            commands.Dispose();
            results.Dispose();
        }

        [BurstCompile] private static void HandleResults(
            in NativeArray<float3> audioSourcePositions,
            in NativeArray<float> audioSourceVolumes,
            in NativeArray<float> audioSourceRanges,
            ref NativeArray<RaycastCommand> commands,
            in NativeArray<RaycastHit> results,
            ref NativeArray<float> audioLevels,
            AudibilityPlane plane,
            int nDirections)
        {
            // Loop through results
            for (int commandIndex = 0; commandIndex < results.Length; commandIndex++)
            {
                // Prepare data
                float3 endPosition;
                RaycastCommand command = commands[commandIndex];
                RaycastHit detectedHit = results[commandIndex];

                // Move command index into audio sample index
                int sampleIndex = (int) (commandIndex / nDirections);


                // Handle both cases for collision detection
                if (detectedHit.point == default)
                {
                    endPosition = command.from + command.direction * MAX_RAY_DISTANCE;
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
                }

                // Compute and update audio level based on positioning
                float audioLevel = ComputeInverseAudioLevelNearRay(audioSourcePositions,
                    audioSourceVolumes, audioSourceRanges, command.from,
                    endPosition);
                if (audioLevel < audioLevels[sampleIndex]) audioLevels[sampleIndex] = audioLevel;

                command.from = endPosition;
                commands[commandIndex] = command;
            }
        }

        [BurstCompile] public static float ComputeInverseAudioLevelNearRay(
            in NativeArray<float3> worldAudioSourceLocations,
            in NativeArray<float> audioSourceVolumesDecibels,
            in NativeArray<float> audioSourceRanges,
            in float3 rayStartPoint,
            in float3 rayEndPoint)
        {
            if (worldAudioSourceLocations.Length == 0) return float.PositiveInfinity;

            float minAudioLevel = float.PositiveInfinity;

            float3 lineVec = rayEndPoint - rayStartPoint;
            float lineLenSq = math.lengthsq(lineVec); // Avoid sqrt for efficiency

            for (int index = 0; index < worldAudioSourceLocations.Length; index++)
            {
                float3 audioLocation = worldAudioSourceLocations[index];
                float audioVolume = audioSourceVolumesDecibels[index];
                float audioRange = audioSourceRanges[index];

                // Vector from start to point
                float3 startToPoint = audioLocation - rayStartPoint;

                // Project point onto line (normalized to segment)
                float t = (lineLenSq > 0f) ? math.dot(startToPoint, lineVec) / lineLenSq : 0f;

                // Clamp t to stay within segment
                t = math.clamp(t, 0f, 1f);

                // Closest point on segment
                float3 closestPoint = rayStartPoint + t * lineVec;

                float audioLevel = GetInverseAudioLevel(audioLocation, closestPoint, audioVolume, audioRange);
                if (audioLevel < minAudioLevel) minAudioLevel = audioLevel;
            }

            return minAudioLevel;
        }

        [BurstCompile] public static float GetInverseAudioLevel(
            in float3 worldAudioSourceLocation,
            in float3 worldPoint,
            float audioVolumeDecibels,
            float audioSourceRange)
        {
            float distanceSq = math.distancesq(worldAudioSourceLocation, worldPoint);
            if (distanceSq > audioSourceRange) return float.MaxValue;
            
            // Distance to point, affected by sqrt of volume
            // Calculated using square version to improve performance
            return distanceSq /
                   audioVolumeDecibels;
        }
    }
}