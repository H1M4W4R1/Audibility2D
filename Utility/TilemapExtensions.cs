using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.Audibility2D.Data.Native;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Utility
{
    public static class TilemapExtensions
    {
        [BurstDiscard] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GridInfo2D AsGridInfo([NotNull] this Tilemap tilemap)
        {
            Vector3Int origin = tilemap.origin;
            Vector3Int size = tilemap.size;

            float3 worldPoint = tilemap.GetCellCenterWorld(origin);

            return new(new int2(origin.x, origin.y),
                new int2(size.x, size.y), worldPoint, ((float3) tilemap.cellSize).xy);
        }
    }
}