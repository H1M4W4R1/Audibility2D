using Systems.Audibility.Common.Data;
using Systems.Audibility.Common.Utility;
using Systems.Audibility3D.Components;
using Systems.Audibility3D.Utility;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility3D.Debugging
{
    public sealed class MufflingLevelAnalysisDrawer3D : MonoBehaviour
    {
        [SerializeField] private LayerMask audioRaycastLayers;
        [SerializeField] private float sphereSize = 0.16f;
        [SerializeField] private int gridSize = 15;
        [SerializeField] private float gridDistance = 1;

        private RaycastHit[] _hits = new RaycastHit[8];
        private NativeArray<float3> _samplePositionsArray;
        private NativeArray<DecibelLevel> _muffleStrengthArray;
        private float3 _lastPosition = float3.zero;

        private void OnDrawGizmosSelected()
        {
            SampleData();

            for (int xIndex = 0; xIndex < gridSize; xIndex++)
            {
                for (int yIndex = 0; yIndex < gridSize; yIndex++)
                {
                    int nIndex = xIndex * gridSize + yIndex;

                    float percentage = _muffleStrengthArray[nIndex].GetAverage() / (float) Loudness.MAX;
                    Gizmos.color = Color.Lerp(Color.white, Color.blue,
                        _muffleStrengthArray[nIndex].GetAverage() > 5 ? 0 : 1);
                    Gizmos.DrawSphere(_samplePositionsArray[nIndex], sphereSize);
                }
            }

            Gizmos.color = Color.white;
        }

        private void SampleData()
        {
            float3 objPos = transform.position;
            bool arraySizeChanged = gridSize * gridSize != (_samplePositionsArray.IsCreated
                ? _samplePositionsArray.Length
                : -1);

            // TODO: Verify if no obstacle modification was performed
            if (Mathf.Approximately(math.distancesq(_lastPosition, objPos), 0.0f) && !arraySizeChanged) return;

            float distance = 100;
            float3 vectorDistance = new(0, distance, 0);
            float3 vectorDown = new(0, -1, 0);

            // Ensure tables are properly initialized
            QuickArray.PerformEfficientAllocation(ref _muffleStrengthArray, gridSize * gridSize,
                Allocator.Persistent);
            QuickArray.PerformEfficientAllocation(ref _samplePositionsArray, gridSize * gridSize,
                Allocator.Persistent);

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

                    // Sample muffling strength
                    int nCollisions = Physics.RaycastNonAlloc(position + vectorDistance,
                        vectorDown, _hits, distance, audioRaycastLayers, QueryTriggerInteraction.Ignore);

                    _muffleStrengthArray[nIndex] = Loudness.SILENCE;
                    for (int collisionIndex = 0; collisionIndex < nCollisions; collisionIndex++)
                    {
                        // Verify if not null
                        RaycastHit hit = _hits[collisionIndex];
                        if (hit.colliderInstanceID == 0) break;

                        // Get obstacle
                        AudibleObstacle obstacle = hit.collider.GetComponent<AudibleObstacle>();
                        if (ReferenceEquals(obstacle, null)) continue;

                        _muffleStrengthArray[nIndex] = obstacle.GetMufflingLevel();
                    }
                }
            }

            _lastPosition = objPos;
        }
    }
}