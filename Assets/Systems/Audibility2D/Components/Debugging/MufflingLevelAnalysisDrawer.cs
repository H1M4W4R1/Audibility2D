using JetBrains.Annotations;
using Systems.Audibility2D.Tiles;
using Systems.Audibility2D.Utility;
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
            
            Gizmos.color = Color.cyan;

            Vector3Int tilemapSize = audioTilemap.size;
            Vector3Int tilemapOrigin = audioTilemap.origin;
            
            float3 cellSize = audioTilemap.cellSize;
            float3 worldOrigin = (float3) audioTilemap.CellToWorld(tilemapOrigin) + 0.5f * cellSize;
      
            // Prepare analysis data
            for (int x = 0; x < tilemapSize.x; x++)
            {
                for (int y = 0; y < tilemapSize.y; y++)
                {
                    // Pre-compute Unity-based data
                    Vector3Int cellPosition = tilemapOrigin + new Vector3Int(x, y, 0);
                   
                    // Get data from tile, we're pre-caching it early and using multiplication
                    // to improve performance, as it's faster than casting external calls to Unity API
                    float3 worldPosition = worldOrigin + new float3(x * cellSize.x, y * cellSize.y, 0);
                    
                    AudioTile audioTile = audioTilemap.GetTile(cellPosition) as AudioTile;
                    if (ReferenceEquals(audioTile, null)) continue;

                    // Compute percentage and remap into 0~1 range
                    float percentage = (float) audioTile.GetMufflingData().GetAverage() /
                                       AudibilityLevel.LOUDNESS_MAX;
                    percentage = math.remap(-1, 1, 0f, 1f, percentage);
                    
                    // Compute gizmo color
                    Gizmos.color = Color.Lerp(Color.green, Color.red, percentage);
                    
                    Gizmos.DrawSphere(worldPosition, 0.15f);
                }
            }
        }
    }
}