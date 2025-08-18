using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Data.Native
{
    public readonly struct TilemapInfo
    {
        public readonly int3 originPoint;
        public readonly int3 size;
        public readonly float3 worldOriginPoint;
        public readonly float3 tileSize;

        public TilemapInfo(int3 originPoint, int3 size, float3 worldOriginPoint, float3 tileSize)
        {
            this.originPoint = originPoint;
            this.size = size;
            this.worldOriginPoint = worldOriginPoint;
            this.tileSize = tileSize;
        }

        public TilemapInfo([NotNull] Tilemap tilemap)
        {
            // Cache original values
            Vector3Int tilemapOriginPoint = tilemap.origin;
            Vector3Int tilemapSize = tilemap.size;

            // Compute
            originPoint = new int3(tilemapOriginPoint.x, tilemapOriginPoint.y, tilemapOriginPoint.z);
            size = new int3(tilemapSize.x, tilemapSize.y, tilemapSize.z);
            tileSize = tilemap.cellSize;
            
            worldOriginPoint = (float3) tilemap.CellToWorld(tilemapOriginPoint) + 0.5f * tileSize;
        }
    }
}