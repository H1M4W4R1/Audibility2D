namespace Systems.Audibility.Components
{
#if false
    public sealed class OldCode
    {
                [NotNull] public static DecibelLevel[] ComputeMultiPointAudioLoudnessV2(
            [NotNull] float3[] points,
            [NotNull] [ItemNotNull] List<AudibleAudioSource> sources,
            LayerMask audioRaycastLayers)
        {
            const int MAX_HITS = 8;
            int nSamples = points.Length * sources.Count;

            Stopwatch stopwatch = Stopwatch.StartNew();

            // TODO: Move to a job to improve performance of this section
            DecibelLevel[] resultLevels = new DecibelLevel[points.Length];
            for (int n = 0; n < resultLevels.Length; n++) resultLevels[n] = Loudness.SILENCE;

            // Setup result table
            NativeArray<DecibelLevel> scannedLevels = new(nSamples, Allocator.TempJob);
            for (int nSample = 0; nSample < scannedLevels.Length; nSample++)
                scannedLevels[nSample] = sources[GetSourceIndex(nSample)].decibelLevel;

            // Setup raycast settings
            NativeArray<RaycastCommand> raycastCommands = new(nSamples, Allocator.TempJob);
            NativeArray<RaycastHit> raycastResults =
                new(nSamples * MAX_HITS, Allocator.TempJob);

            QueryParameters raycastParameters = new(audioRaycastLayers, true, QueryTriggerInteraction.Ignore);

            // Create all commands for ray-casting
            for (int nSample = 0; nSample < nSamples; nSample++)
            {
                AudioSource source = sources[GetSourceIndex(nSample)]._audioSource;
                float3 atPosition = points[GetPointIndex(nSample)];

                float3 sourcePosition = source.transform.position;
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
            }

            stopwatch.Stop();
            Debug.Log($"Preparation took {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();

            // Perform all raycasts
            JobHandle jobAwaiter = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, 1, MAX_HITS);
            jobAwaiter.Complete();

            stopwatch.Stop();
            Debug.Log($"Ray-casting took {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();

            // TODO: Move to a job to improve performance of this section
            for (int nSample = 0; nSample < nSamples; nSample++)
            {
                for (int nResult = 0; nResult < MAX_HITS; nResult++)
                {
                    // Acquire hit data
                    RaycastHit hit = raycastResults[nSample * MAX_HITS + nResult];
                    Collider detectedCollider = hit.collider;

                    // Check obstacle presence
                    if (ReferenceEquals(detectedCollider, null)) continue;
                    AudibleObstacle obstacle = detectedCollider.GetComponent<AudibleObstacle>();
                    if (ReferenceEquals(obstacle, null)) continue;

                    // Muffle sound if obstacle present
                    scannedLevels[nSample] = scannedLevels[nSample].MuffleBy(obstacle.GetMufflingLevel());
                }

                float distance = raycastCommands[nSample].distance;
                float maxDistance = sources[GetSourceIndex(nSample)]._audioSource.maxDistance;

                scannedLevels[nSample] = scannedLevels[nSample]
                    .MuffleAllFrequenciesBy((byte) math.lerp(0, Loudness.MAX,
                        math.clamp(distance / maxDistance, 0, 1)));
            }
            
            stopwatch.Stop();
            Debug.Log($"Computing audio levels took {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();

            // Compute the largest known value
            for (int nPointIndex = 0; nPointIndex < points.Length; nPointIndex++)
            {
                for (int nSourceIndex = 0; nSourceIndex < sources.Count; nSourceIndex++)
                {
                    int nSampleIndex = GetSampleIndex(nPointIndex, nSourceIndex);
                    resultLevels[nPointIndex] =
                        DecibelLevel.Max(resultLevels[nPointIndex], scannedLevels[nSampleIndex]);
                }
            }

            stopwatch.Stop();
            Debug.Log($"Converging audio levels took {stopwatch.ElapsedMilliseconds}ms");
      
            // Clean memory
            scannedLevels.Dispose();
            raycastCommands.Dispose();
            raycastResults.Dispose();

            return resultLevels;

            int GetSampleIndex(int pointIndex, int sourceIndex) => pointIndex * sources.Count + sourceIndex;
            int GetSourceIndex(int sampleIndex) => sampleIndex % sources.Count;
            int GetPointIndex(int sampleIndex) => sampleIndex / sources.Count;
        }

        public static DecibelLevel ComputeMultiPointAudioLoudnessV1(
            float3 atPoint,
            [NotNull] [ItemNotNull] List<AudibleAudioSource> sources,
            LayerMask audioRaycastLayers)
        {
            const int MAX_HITS = 8;
            DecibelLevel resultLevel = Loudness.SILENCE;

            // Setup result table
            NativeArray<DecibelLevel> scannedLevels = new(sources.Count, Allocator.TempJob);
            for (int nSource = 0; nSource < scannedLevels.Length; nSource++)
                scannedLevels[nSource] = sources[nSource].decibelLevel;

            // Setup raycast settings
            NativeArray<RaycastCommand> raycastCommands = new(sources.Count, Allocator.TempJob);
            NativeArray<RaycastHit> raycastResults = new(sources.Count * MAX_HITS, Allocator.TempJob);

            QueryParameters raycastParameters = new(audioRaycastLayers, true, QueryTriggerInteraction.Ignore);

            // Create all commands for ray-casting
            for (int nSource = 0; nSource < sources.Count; nSource++)
            {
                float3 sourcePosition = sources[nSource].transform.position;
                float3 direction = math.normalize(atPoint - sourcePosition);
                float distance = math.distance(atPoint, sourcePosition);

                RaycastCommand command = new()
                {
                    from = sourcePosition,
                    direction = direction,
                    distance = distance,
                    queryParameters = raycastParameters,
                    physicsScene = Physics.defaultPhysicsScene
                };
                raycastCommands[nSource] = command;
            }

            // Perform all raycasts
            JobHandle jobAwaiter = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, 1, MAX_HITS);
            jobAwaiter.Complete();

            for (int nSource = 0; nSource < raycastCommands.Length; nSource++)
            {
                for (int nResult = 0; nResult < MAX_HITS; nResult++)
                {
                    // Acquire hit data
                    RaycastHit hit = raycastResults[nResult];
                    Collider detectedCollider = hit.collider;

                    // Check obstacle presence
                    if (ReferenceEquals(detectedCollider, null)) continue;
                    AudibleObstacle obstacle = detectedCollider.GetComponent<AudibleObstacle>();
                    if (ReferenceEquals(obstacle, null)) continue;

                    // Muffle sound if obstacle present
                    scannedLevels[nSource] = scannedLevels[nSource].MuffleBy(obstacle.GetMufflingLevel());
                }

                float distance = raycastCommands[nSource].distance;
                float maxDistance = sources[nSource]._audioSource.maxDistance;

                scannedLevels[nSource] = scannedLevels[nSource]
                    .MuffleAllFrequenciesBy((byte) math.lerp(0, Loudness.MAX,
                        math.clamp(distance / maxDistance, 0, 1)));
            }

            // Compute the largest known value
            for (int nAudioLevel = 0; nAudioLevel < scannedLevels.Length; nAudioLevel++)
            {
                resultLevel = DecibelLevel.Max(resultLevel, scannedLevels[nAudioLevel]);
            }

            // Clean memory
            scannedLevels.Dispose();
            raycastCommands.Dispose();
            raycastResults.Dispose();

            return resultLevel;
        }

    }
#endif
}