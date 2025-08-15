using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility.Debugging
{
    public sealed class AudibilityScanner : MonoBehaviour
    {
        [SerializeField] private int gridSize = 7;
        [SerializeField] private float gridDistance = 1;

        private void OnDrawGizmos()
        {
            float3 worldPosition = transform.position;
            float3[] positions = new float3[gridSize * gridSize];


            for (int gridX = 0; gridX < gridSize; gridX++)
            {
                for (int gridY = 0; gridY < gridSize; gridY++)
                {
                    positions[gridX * gridSize + gridY] = worldPosition +
                                                          new float3(
                                                              gridX * gridDistance -
                                                              gridSize * gridDistance * 0.5f, 0,
                                                              gridY * gridDistance -
                                                              gridSize * gridDistance * 0.5f);
                }
            }

            float[] audioLevels = AudioUtilities.GetAudibilityLevel(positions);


            // Compute max audio level
            float maxAudioLevel = 0;
            for (int n = 0; n < audioLevels.Length; n++)
            {
                if (Mathf.Approximately(audioLevels[n], float.MaxValue)) continue;
                if (audioLevels[n] > maxAudioLevel) maxAudioLevel = audioLevels[n];
            }

            // Normalize and average audio levels
            float[] averagedAudioLevels = new float[audioLevels.Length];
            for (int n = 0; n < averagedAudioLevels.Length; n++)
            {
                if (Mathf.Approximately(audioLevels[n], float.MaxValue))
                {
                    audioLevels[n] = 0;
                    continue;;
                }
                float inverseNormalizedAudioLevel = audioLevels[n] / maxAudioLevel;
                averagedAudioLevels[n] = 1 - inverseNormalizedAudioLevel;
            }

            for (int gridX = 0; gridX < gridSize; gridX++)
            {
                for (int gridY = 0; gridY < gridSize; gridY++)
                {
                    float avgSample = averagedAudioLevels[gridX * gridSize + gridY];
                    int nEdges = 1;

                    if (gridX > 0)
                    {
                        avgSample += averagedAudioLevels[(gridX - 1) * gridSize + gridY];
                        nEdges++;
                    }

                    if (gridX < gridSize - 1)
                    {
                        avgSample += averagedAudioLevels[(gridX + 1) * gridSize + gridY];
                        nEdges++;
                    }

                    if (gridY > 0)
                    {
                        avgSample += averagedAudioLevels[gridX * gridSize + gridY - 1];
                        nEdges++;
                    }

                    if (gridY < gridSize - 1)
                    {
                        avgSample += averagedAudioLevels[gridX * gridSize + gridY + 1];
                        nEdges++;
                    }

                    avgSample /= nEdges;
                    averagedAudioLevels[gridX * gridSize + gridY] = avgSample;
                }
            }

            for (int nPosition = 0; nPosition < positions.Length; nPosition++)
            {
                // Prevent going out of range
                if (audioLevels.Length < nPosition) break;


                Gizmos.color = Color.Lerp(Color.red, Color.green, averagedAudioLevels[nPosition]);


                Gizmos.DrawSphere(positions[nPosition], 0.1f);
            }

            Gizmos.color = Color.white;
        }
    }
}