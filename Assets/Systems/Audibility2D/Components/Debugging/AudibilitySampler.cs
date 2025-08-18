using JetBrains.Annotations;
using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Data.Native.Wrappers;
using Systems.Audibility2D.Data.Tiles;
using Systems.Audibility2D.Utility;
using Systems.Audibility2D.Utility.Internal;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Components.Debugging
{
    /// <summary>
    ///     Debug script used to sample 2D audio for specified tilemap
    /// </summary>
    public sealed class AudibilitySampler : MonoBehaviour
    {
        /// <summary>
        ///     Audio tilemap that will be used to compute data
        ///     Must use <see cref="AudioTile"/> to modify audio behaviour
        /// </summary>
        [SerializeField] [CanBeNull] private Tilemap audioTilemap;

        private NativeArray<AudioTileInfo> _tileComputeData;
        private NativeArray<AudioSourceInfo> _audioSourceComputeData;
        private NativeArray<AudioLoudnessLevel> _loudnessData;

        private void OnDrawGizmos()
        {
            // Ensure tilemap is set
            if (!audioTilemap) return;

            // Use Scene camera in Editor (falls back to main camera if missing)
            // In case no camera was found we don't want anything
            Camera gizmosCamera = Camera.current ? Camera.current : Camera.main;
            if (!gizmosCamera) return;

            Vector3Int tilemapSize = audioTilemap.size;
            int tilesCount = tilemapSize.x * tilemapSize.y * tilemapSize.z;
            QuickArray.PerformEfficientAllocation(ref _loudnessData, tilesCount, Allocator.TempJob);

            // Compute audibility in 2D space
            AudibilityLevel.UpdateAudibilityLevel(audioTilemap, ref _audioSourceComputeData, ref _tileComputeData);

            // Compute average tile loudness
            AudibilityTools.GetAverageLoudnessData(in _tileComputeData, ref _loudnessData);

            TilemapInfo tilemapInfo = new(audioTilemap);

            // Compute camera planes
            NativeArray<float4> frustrumPlanes = new(6, Allocator.TempJob);
            gizmosCamera.ExtractFrustumPlanes(ref frustrumPlanes);

            // Draw gizmos
            for (int n = 0; n < _loudnessData.Length; n++)
            {
                // Quickly compute tile position using tilemap
                TileIndex index = new(n);

                // TODO: Improve perf of this line using some Black Magic F*$#ery
                float3 worldTilePosition = index.GetWorldPosition(tilemapInfo);

                // Quickly check camera point in view frustrum
                if (!MakeGizmosFasterUtility.PointInFrustum(worldTilePosition, frustrumPlanes)) continue;

                Gizmos.color = Color.Lerp(Color.red, Color.green,
                    _loudnessData[n] / (float) AudibilityLevel.LOUDNESS_MAX);
                Gizmos.DrawSphere(worldTilePosition, 0.2f);
            }

            _loudnessData.Dispose();
            frustrumPlanes.Dispose();
        }
    }
}