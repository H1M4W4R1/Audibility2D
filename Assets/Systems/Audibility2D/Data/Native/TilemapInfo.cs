using Unity.Mathematics;
using UnityEngine;

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
    }
}