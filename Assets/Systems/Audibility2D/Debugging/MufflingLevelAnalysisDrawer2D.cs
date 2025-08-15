using System;
using Systems.Audibility.Common.Utility;
using Systems.Audibility2D.Tiles;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Debugging
{
    public sealed class MufflingLevelAnalysisDrawer2D : MonoBehaviour
    {
        [SerializeField] private Tilemap audioTilemap;

        private void OnDrawGizmos()
        {
            // Find tilemap
            if (!audioTilemap)
            {
                Debug.LogError($"[{nameof(MufflingLevelAnalysisDrawer2D)}] No audioTilemap found!");
                return;
            }

            Gizmos.color = Color.cyan;
            
            // Compute all cell positions
            int3x2 corners = audioTilemap.FindTilemapCorners();
            float3 cellSize = audioTilemap.cellSize;

            // Get cell center positions
            float3 initialCellSpot = corners.c0 + 0.5f * cellSize;
            float3 endCellSpot = corners.c1 - 0.5f * cellSize;

            // Draw muffling gizmos on audio cells
            for (float xPosition = initialCellSpot.x; xPosition <= endCellSpot.x; xPosition += cellSize.x)
            {
                for (float yPosition = initialCellSpot.y; yPosition <= endCellSpot.y; yPosition += cellSize.y)
                {
                    float3 worldPosition = new(xPosition, yPosition, 0);
                    Vector3Int cellPosition = audioTilemap.WorldToCell(worldPosition);
                    
                    AudioTile audioTile = audioTilemap.GetTile(cellPosition) as AudioTile;
                    if (ReferenceEquals(audioTile, null)) continue;

                    // Compute percentage and remap into 0~1 range
                    float percentage = (float) audioTile.GetMufflingData().GetAverage() /
                                       Loudness.MAX;
                    percentage = math.remap(-1, 1, 0f, 1f, percentage);
                    
                    // Compute gizmo color
                    Gizmos.color = Color.Lerp(Color.green, Color.red, percentage);
                    
                    Gizmos.DrawSphere(worldPosition, 0.15f);
                }
            }
        }
    }
}