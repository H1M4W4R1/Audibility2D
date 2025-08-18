using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility2D.Data.Native
{
    [BurstCompile] public readonly struct TileIndex
    {
        /// <summary>
        ///     Index value storage
        /// </summary>
        public readonly int value;

        /// <summary>
        ///     Create new tile index from value
        /// </summary>
        public TileIndex(int value)
        {
            this.value = value;
        }

        /// <summary>
        ///     Create new tile index from position
        /// </summary>
        public TileIndex(int x, int y, int z, in TilemapInfo tilemapInfo) :
            this(ToIndex(x, y, z, tilemapInfo))
        {
        }

        /// <summary>
        ///     Create new tile index from position
        /// </summary>
        public TileIndex(int3 tilePosition, in TilemapInfo tilemapInfo) :
            this(ToIndex(tilePosition.x, tilePosition.y, tilePosition.z, tilemapInfo))
        {
        }


        /// <summary>
        ///     Get tilemap position from this index
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int3 GetTilemapPosition(in TilemapInfo tilemapInfo)
        {
            FromIndex(value, tilemapInfo, out int3 result);
            return result;
        }

        /// <summary>
        ///     Get world location of this tile
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] public float3
            GetWorldPosition(in TilemapInfo tilemapInfo)
        {
            // Convert back into tilemap position
            FromIndex(value, tilemapInfo, out int3 result);

            // Convert into world position
            result -= tilemapInfo.originPoint; // We move by origin point to get offset of the tile from origin
            float3 worldLocation = tilemapInfo.worldOriginPoint + tilemapInfo.tileSize * (result +
                new float3(0.5f, 0.5f, 0.5f)); // Move by offset and half tile

            return worldLocation;
        }

        /// <summary>
        /// Converts 3D coordinates (x, y, z) into a 1D tile index.
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToIndex(int x, int y, int z, in TilemapInfo tilemapInfo)
        {
            // Compute real tilemap offset, we subtract origin point
            // to orient index values around left bottom corner of Tilemap
            int3 tilemapOffset = new int3(x, y, z) - new int3(tilemapInfo.originPoint.x, tilemapInfo.originPoint.y,
                tilemapInfo.originPoint.z);

            return (tilemapOffset.x * tilemapInfo.size.y * tilemapInfo.size.z) +
                   (tilemapOffset.y * tilemapInfo.size.z) + tilemapOffset.z;
        }

        /// <summary>
        /// Converts 1D tile index back into 3D coordinates (x, y, z).
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromIndex(int index, in TilemapInfo tilemapInfo, out int3 tilemapPosition)
        {
            int sizeY = tilemapInfo.size.y;
            int sizeZ = tilemapInfo.size.z;

            // Decode offsets
            int x = index / (sizeY * sizeZ);
            int remainder = index % (sizeY * sizeZ);
            int y = remainder / sizeZ;
            int z = remainder % sizeZ;

            // Re-apply origin
            tilemapPosition = new int3(
                x + tilemapInfo.originPoint.x,
                y + tilemapInfo.originPoint.y,
                z + tilemapInfo.originPoint.z
            );
        }

        public static implicit operator int(TileIndex tileIndex) => tileIndex.value;
    }
}