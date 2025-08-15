using System.Runtime.CompilerServices;
using Systems.Audibility.Common.Data;
using Systems.Audibility.Common.Utility;
using Systems.Audibility3D.Components;
using Systems.Audibility3D.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.Audibility3D.Utility
{
    public static class AudibilityLevel
    {
        private static readonly RaycastHit[] _raycastHitsSingleCheck = new RaycastHit[8];
        private static NativeArray<DecibelLevel> _scannedLevels;
        private static NativeArray<RaycastHit> _raycastResults;
        private static NativeArray<RaycastCommand> _raycastCommands;
        
        /// <summary>
        ///     Compute audio loudness at desired points based on all available audio sources provided to this method.
        /// </summary>
        public static void GetMultiPoint(
            in NativeArray<float3> points,
            in NativeArray<float3> sourcePositions,
            in NativeArray<float> sourceRanges,
            in NativeArray<DecibelLevel> sourceDecibelLevels,
            in LayerMask audioRaycastLayers,
            ref NativeArray<DecibelLevel> resultLevels)
        {
            Assert.AreEqual(sourcePositions.Length, sourceRanges.Length);
            Assert.AreEqual(sourcePositions.Length, sourceDecibelLevels.Length);
            Assert.AreEqual(resultLevels.Length, points.Length);

            // Global variables 
            const int MAX_HITS = 8;
            int nSamples = points.Length * sourcePositions.Length;
            int nSources = sourcePositions.Length;
            QueryParameters raycastParameters = new(audioRaycastLayers, true, QueryTriggerInteraction.Ignore);

            // Allocate raycast table and start job to fill it with contents
            QuickArray.PerformEfficientAllocation(ref _raycastCommands, nSamples, Allocator.Persistent);
            CreateAudioSamplingRaycastsJob createRaycastDataJob = new()
            {
                points = points,
                sourcePositions = sourcePositions,
                raycastCommands = _raycastCommands,
                raycastParameters = raycastParameters
            };
            JobHandle raycastPreparationHandle = createRaycastDataJob.Schedule(nSamples, math.min(nSamples, 16));

            // Allocate remaining tables while waiting for job to complete
            QuickArray.PerformEfficientAllocation(ref _scannedLevels, nSamples, Allocator.Persistent);
            QuickArray.PerformEfficientAllocation(ref _raycastResults, nSamples * MAX_HITS, Allocator.Persistent);

            // Perform reset
            ResetScannedAudioLevelsJob resetScannedAudioLevelsJob = new()
            {
                scannedLevels = _scannedLevels,
                sourceDecibelLevels = sourceDecibelLevels,
                sourcesCount = nSources
            };
            JobHandle resetScannedAudioLevelsHandle =
                resetScannedAudioLevelsJob.Schedule(nSamples, math.min(nSamples, 16));

            // Clear raycast results table                      
            ResetRaycastResultTableJob resetRaycastResultsJob = new()
            {
                raycastHits = _raycastResults
            };

            // Table should be prepared at this moment as it's synchronous
            JobHandle resetRaycastResultsHandle =
                resetRaycastResultsJob.Schedule(_raycastResults.Length, math.min(_raycastResults.Length, 16));

            // Wait for jobs to complete
            resetRaycastResultsHandle.Complete();
            raycastPreparationHandle.Complete();

            // Perform all raycasts
            JobHandle jobAwaiter = RaycastCommand.ScheduleBatch(_raycastCommands, _raycastResults, 1, MAX_HITS);

            // Wait for everything to be ready
            jobAwaiter.Complete();
            resetScannedAudioLevelsHandle.Complete();

            FastSimpleAudibilityLevelCalculatorJob simpleAudibilityLevelCalculatorJobJob =
                new()
                {
                    raycastCommands = _raycastCommands,
                    raycastResults = _raycastResults,
                    scannedLevels = _scannedLevels,
                    sourceRanges = sourceRanges,
                    raycastMaxHits = MAX_HITS
                };

            JobHandle audibilityCalculatorHandle =
                simpleAudibilityLevelCalculatorJobJob.Schedule(nSamples, math.min(nSamples, 16));
            audibilityCalculatorHandle.Complete();
       
            // Compute the largest known value
            for (int nPointIndex = 0; nPointIndex < points.Length; nPointIndex++)
            {
                for (int nSourceIndex = 0; nSourceIndex < sourceRanges.Length; nSourceIndex++)
                {
                    int nSampleIndex = GetSampleIndex(nPointIndex, nSourceIndex);
                    resultLevels[nPointIndex] =
                        DecibelLevel.Max(resultLevels[nPointIndex], _scannedLevels[nSampleIndex]);
                }
            }

            return;

            [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
            int GetSampleIndex(int pointIndex, int sourceIndex) => pointIndex * nSources + sourceIndex;
        }

        /// <summary>
        ///     Compute audio loudness at desired point, inefficient way to do so... very inefficient.
        ///     Should be used only for debugging purposes.
        /// </summary>
        public static DecibelLevel GetSinglePoint(float3 atPoint, float3 sourcePosition, float sourceRange,
            DecibelLevel sourceLevel, LayerMask audioRaycastLayers)
        {
            float distance = math.distance(sourcePosition, atPoint);
            if (distance > sourceRange) return Loudness.SILENCE;

            DecibelLevel resultLevel = sourceLevel;

            float3 direction = math.normalize(sourcePosition - atPoint);
            int size = Physics.RaycastNonAlloc(atPoint, direction, _raycastHitsSingleCheck, distance, audioRaycastLayers,
                QueryTriggerInteraction.Ignore);

            for (int hitIndex = 0; hitIndex < size; hitIndex++)
            {
                // Get hit obstacle if any exists
                RaycastHit hit = _raycastHitsSingleCheck[hitIndex];
                AudibleObstacle obstacle = hit.collider.GetComponent<AudibleObstacle>();
                if (ReferenceEquals(obstacle, null)) continue;
                resultLevel.MuffleBy(obstacle.GetMufflingLevel());
            }

            resultLevel = resultLevel.MuffleAllFrequenciesBy((byte) math.lerp(0, Loudness.MAX,
                math.clamp(distance / sourceRange, 0, 1)));

            return resultLevel;
        }
    }
}