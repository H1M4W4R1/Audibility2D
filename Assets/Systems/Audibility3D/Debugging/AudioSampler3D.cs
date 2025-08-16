using Systems.Audibility.Common.Components;
using Systems.Audibility.Common.Data;
using Systems.Audibility.Common.Utility;
using Systems.Audibility3D.Utility;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility3D.Debugging
{
    /// <summary>
    ///     Debug utility to render 3D sampling of audio data
    /// </summary>
    public sealed class AudioSampler3D : MonoBehaviour
    {
        /// <summary>
        ///     Layers to raycast for audio obstacles
        /// </summary>
        [SerializeField] private LayerMask audioRaycastLayers;
        
        /// <summary>
        ///     Size of debug sphere
        /// </summary>
        [SerializeField] private float sphereSize = 0.16f;
        
        /// <summary>
        ///     Size of grid
        /// </summary>
        [SerializeField] private int gridSize = 15;
        
        /// <summary>
        ///     Distance between grid objects
        /// </summary>
        [SerializeField] private float gridDistance = 1;

        // Local arrays to store all data
        private NativeArray<float3> _samplePositionsArray;
        private NativeArray<float3> _sourcesPositionsArray;
        private NativeArray<DecibelLevel> _sourceDecibelLevelsArray;
        private NativeArray<float> _sourceRangesArray;
        private NativeArray<DecibelLevel> _decibelLevelResultsArray;

        private void OnDrawGizmos()
        {
            int samplesSize = gridSize * gridSize * gridSize;
            float3 objPos = transform.position;
            AudibleSound[] sources =
                FindObjectsByType<AudibleSound>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            // Re-allocate arrays if necessary
            QuickArray.PerformEfficientAllocation(ref _samplePositionsArray, samplesSize,
                Allocator.Persistent);
            QuickArray.PerformEfficientAllocation(ref _decibelLevelResultsArray, samplesSize,
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
                    float yPosition = -gridSize / 2f * gridDistance + yIndex * gridDistance;
                    for (int zIndex = 0; zIndex < gridSize; zIndex++)
                    {
                        int nIndex = xIndex * gridSize * gridSize + yIndex * gridSize + zIndex;
                        float zPosition = -gridSize / 2f * gridDistance + zIndex * gridDistance;
                        float3 position = new float3(xPosition, yPosition, zPosition) + objPos;
                        _samplePositionsArray[nIndex] = position;
                        _decibelLevelResultsArray[nIndex] = Loudness.SILENCE;
                    }
                }
            }

            // Setup source data
            for (int nSource = 0; nSource < sources.Length; nSource++)
            {
                _sourcesPositionsArray[nSource] = sources[nSource].transform.position;
                _sourceDecibelLevelsArray[nSource] = sources[nSource].GetDecibelLevel();
                _sourceRangesArray[nSource] = sources[nSource].GetRange();
            }

            AudibilityLevel3D.GetMultiPoint(_samplePositionsArray, _sourcesPositionsArray,
                _sourceRangesArray,
                _sourceDecibelLevelsArray, audioRaycastLayers, ref _decibelLevelResultsArray);

            // Render data
            for (int xIndex = 0; xIndex < gridSize; xIndex++)
            {
                for (int yIndex = 0; yIndex < gridSize; yIndex++)
                {
                    for (int zIndex = 0; zIndex < gridSize; zIndex++)
                    {
                        int nIndex = xIndex * gridSize * gridSize + yIndex * gridSize + zIndex;
                        DecibelLevel currentLevel = _decibelLevelResultsArray[nIndex];
                        Gizmos.color = Color.Lerp(Color.red, Color.green,
                            currentLevel.GetAverage() / (float) Loudness.MAX);
                        Gizmos.DrawSphere(_samplePositionsArray[nIndex], sphereSize);
                    }
                }
            }
        }
    }
}