using Systems.Audibility.Common.Components;
using Systems.Audibility.Common.Data;
using Systems.Audibility.Common.Utility;
using Systems.Audibility3D.Utility;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility3D.Debugging
{
    public sealed class AudioSampler3D : MonoBehaviour
    {
        [SerializeField] private LayerMask audioRaycastLayers;
        [SerializeField] private float sphereSize = 0.16f;
        [SerializeField] private int gridSize = 15;
        [SerializeField] private float gridDistance = 1;

        private NativeArray<float3> _samplePositionsArray;
        private NativeArray<float3> _sourcesPositionsArray;
        private NativeArray<DecibelLevel> _sourceDecibelLevelsArray;
        private NativeArray<float> _sourceRangesArray;
        private NativeArray<DecibelLevel> _decibelLevelResultsArray;

        private void OnDrawGizmos()
        {
            float3 objPos = transform.position;
            AudibleAudioSource[] sources =
                FindObjectsByType<AudibleAudioSource>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            // Re-allocate arrays if necessary
            QuickArray.PerformEfficientAllocation(ref _samplePositionsArray, gridSize * gridSize,
                Allocator.Persistent);
            QuickArray.PerformEfficientAllocation(ref _decibelLevelResultsArray, gridSize * gridSize,
                Allocator.Persistent);

            QuickArray.PerformEfficientAllocation(ref _sourcesPositionsArray, sources.Length, Allocator.Persistent);
            QuickArray.PerformEfficientAllocation(ref _sourceDecibelLevelsArray, sources.Length,
                Allocator.Persistent);
            QuickArray.PerformEfficientAllocation(ref _sourceRangesArray, sources.Length, Allocator.Persistent);

            // Setup arrays with default values
            for (int xIndex = 0; xIndex < gridSize; xIndex++)
            {
                float xPosition = -gridSize / 2f * gridDistance + xIndex * gridDistance;
                for (int yIndex = 0; yIndex < gridSize; yIndex++)
                {
                    int nIndex = xIndex * gridSize + yIndex;
                    float zPosition = -gridSize / 2f * gridDistance + yIndex * gridDistance;
                    float3 position = new float3(xPosition, 0, zPosition) + objPos;
                    _samplePositionsArray[nIndex] = position;
                    _decibelLevelResultsArray[nIndex] = Loudness.SILENCE;
                }
            }

            // Setup source data
            for (int nSource = 0; nSource < sources.Length; nSource++)
            {
                _sourcesPositionsArray[nSource] = sources[nSource].transform.position;
                _sourceDecibelLevelsArray[nSource] = sources[nSource].GetDecibelLevel();
                _sourceRangesArray[nSource] = sources[nSource].UnitySourceReference.maxDistance;
            }

            AudibilityLevel3D.GetMultiPoint(_samplePositionsArray, _sourcesPositionsArray,
                _sourceRangesArray,
                _sourceDecibelLevelsArray, audioRaycastLayers, ref _decibelLevelResultsArray);


            // Render data
            for (int xIndex = 0; xIndex < gridSize; xIndex++)
            {
                for (int yIndex = 0; yIndex < gridSize; yIndex++)
                {
                    int nIndex = xIndex * gridSize + yIndex;
                    DecibelLevel currentLevel = _decibelLevelResultsArray[nIndex];

                    Gizmos.color = Color.Lerp(Color.red, Color.green,
                        currentLevel.GetAverage() / (float) Loudness.MAX);
                    Gizmos.DrawSphere(_samplePositionsArray[nIndex], sphereSize);
                }
            }
        }
    }
}