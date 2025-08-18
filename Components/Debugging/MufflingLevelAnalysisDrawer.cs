using JetBrains.Annotations;
using Systems.Audibility2D.Data.Settings;
using Systems.Audibility2D.Data.Tiles;
using Systems.Audibility2D.Utility;
using Systems.Utilities.Frustum;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Components.Debugging
{
    /// <summary>
    ///     Debug script used to draw muffling level for all tiles on specific tilemap,
    ///     pretty inefficient but does its job, rarely used.
    /// </summary>
    public sealed class MufflingLevelAnalysisDrawer : MonoBehaviour
    {
        /// <summary>
        ///     Audio tilemap that will be used to compute data
        ///     Must use <see cref="AudioTile"/> to modify audio behaviour
        /// </summary>
        [SerializeField] [CanBeNull] private Tilemap audioTilemap;

        private void OnDrawGizmos()
        {
            // Find tilemap
            if (!audioTilemap) return;
            
            // Use Scene camera in Editor (falls back to main camera if missing)
            // In case no camera was found we don't want anything
            Camera gizmosCamera = Camera.current ? Camera.current : Camera.main;
            if (!gizmosCamera) return;
            
            Gizmos.color = Color.cyan;

            Vector3Int tilemapSize = audioTilemap.size;
            Vector3Int tilemapOrigin = audioTilemap.origin;
            
            float3 cellSize = audioTilemap.cellSize;
            float3 worldOrigin = (float3) audioTilemap.CellToWorld(tilemapOrigin) + 0.5f * cellSize;
      
            // Compute camera planes
            NativeArray<float4> frustumPlanes = new(6, Allocator.TempJob);
            gizmosCamera.ExtractFrustumPlanes(ref frustumPlanes);
            
            // Prepare analysis data
            for (int x = 0; x < tilemapSize.x; x++)
            {
                for (int y = 0; y < tilemapSize.y; y++)
                {
                    for (int z = 0; z < tilemapSize.z; z++)
                    {
                        // Pre-compute Unity-based data
                        Vector3Int cellPosition = tilemapOrigin + new Vector3Int(x, y, z);

                        // Get data from tile, we're pre-caching it early and using multiplication
                        // to improve performance, as it's faster than casting external calls to Unity API
                        float3 worldPosition =
                            worldOrigin + new float3(x * cellSize.x, y * cellSize.y, z * cellSize.z);

                        // Quickly check camera point in view frustrum
                        if (!FrustumUtil.PointInFrustum(worldPosition, frustumPlanes)) continue;

                        AudioTile audioTile = audioTilemap.GetTile(cellPosition) as AudioTile;
                        if (ReferenceEquals(audioTile, null)) continue;

                        // Compute percentage and remap into 0~1 range
                        float percentage = (float) audioTile.GetMufflingData().GetAverage() /
                                           AudibilityTools.LOUDNESS_MAX;
                        percentage = math.remap(-1, 1, 0f, 1f, percentage);

                        Color mufflingNoneColor = AudibilitySettings.Instance.gizmosColorMinMuffling;
                        Color mufflingFullColor = AudibilitySettings.Instance.gizmosColorMaxMuffling;

                        
                        // Compute gizmo color
                        Gizmos.color = Color.Lerp(mufflingNoneColor, mufflingFullColor, percentage);

                        Gizmos.DrawSphere(worldPosition, 0.15f);
                    }
                }
            }
            
            frustumPlanes.Dispose();
        }
    }
}