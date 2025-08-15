using System.Diagnostics;
using Systems.Audibility.Common.Data;
using Systems.Audibility.Common.Utility;
using Systems.Audibility2D.Components;
using Systems.Audibility2D.Data;
using Systems.Audibility2D.Tiles;
using Systems.Audibility2D.Utility;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;


namespace Systems.Audibility2D.Debugging
{
    public sealed class AudioSampler2D : MonoBehaviour
    {
        [SerializeField] private Tilemap audioTilemap;

        private NativeArray<AudioTile2DComputeData> _tileComputeData;
        private NativeArray<AudioSource2DComputeData> _audioSourceComputeData;

        private void OnDrawGizmos()
        {
            // Initialize tilemap arrays
            Audibility2DTools.TilemapToArray(audioTilemap, ref _tileComputeData);

            // Prepare array of audio sources
            AudibleAudioSource2D[] sources =
                FindObjectsByType<AudibleAudioSource2D>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            QuickArray.PerformEfficientAllocation(ref _audioSourceComputeData, sources.Length,
                Allocator.Persistent);

            for (int nIndex = 0; nIndex < sources.Length; nIndex++)
            {
                // Get basic information
                AudibleAudioSource2D source = sources[nIndex];
                float3 worldPosition = source.transform.position;

                // Compute tilemap index
                Vector3Int tileMapPosition =
                    audioTilemap.WorldToCell(worldPosition) - audioTilemap.origin;
                int tileIndex = tileMapPosition.x * audioTilemap.size.y + tileMapPosition.y;

                // Assign value
                _audioSourceComputeData[nIndex] = new AudioSource2DComputeData(tileIndex,
                    worldPosition + 0.5f * (float3) audioTilemap.cellSize,
                    source.GetDecibelLevel(), source.UnitySourceReference.maxDistance);
            }

            // Handle computation
            AudibilityLevel.UpdateAudibilityLevel(_audioSourceComputeData, ref _tileComputeData);

            // Draw gizmos
            for (int n = 0; n < _tileComputeData.Length; n++)
            {
                float averageAudioLevel = _tileComputeData[n].currentAudioLevel.GetAverage();
                Gizmos.color = Color.Lerp(Color.red, Color.green, averageAudioLevel / Loudness.MAX);
                Gizmos.DrawSphere(_tileComputeData[n].worldPosition, 0.2f);
            }
        }
    }
}