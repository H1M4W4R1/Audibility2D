using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.Utilities.Indexing.Grid;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Utility
{
    public static class TilemapExtensions
    {
        [BurstDiscard]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GridInfo3D AsGridInfo([NotNull] this Tilemap tilemap)
        {
            Vector3Int origin = tilemap.origin;
            Vector3Int size = tilemap.size;

            float3 worldPoint = tilemap.GetCellCenterWorld(origin);
            
            return new(new int3(origin.x, origin.y, origin.z),
                new int3(size.x, size.y, size.z), worldPoint, tilemap.cellSize);
        }
        
    }
}