using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Utility
{
    public static class TilemapExtensions
    {
        public static int3x2 FindTilemapCorners([NotNull] this Tilemap tilemap)
        {
            Vector3Int leftBottom = tilemap.origin;
            Vector3Int rightTop = leftBottom + tilemap.size;

            return new int3x2
            {
                c0 = new int3(leftBottom.x, leftBottom.y, leftBottom.z),
                c1 = new int3(rightTop.x, rightTop.y, rightTop.z),
            };
        }
        
    }
}