using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;

namespace Systems.Audibility2D.Data.Native.Wrappers
{
    [BurstCompile] public readonly struct TileIndex // 4B
    {
        // No tile...
        public const int NONE = -1;

        /// <summary>
        ///     Index value storage
        /// </summary>
        public readonly int value; // 4B

        /// <summary>
        ///     Create new tile index from value
        /// </summary>
        public TileIndex(int value)
        {
            this.value = value;
        }

        /// <summary>
        ///     Create new tile index from tile position (absolute)
        /// </summary>
        public TileIndex(int x, int y, int z, in TilemapInfo tilemapInfo) :
            this(ToIndexAbsolute(x, y, z, tilemapInfo))
        {
        }

        /// <summary>
        ///     Create new tile index from tile position (absolute)
        /// </summary>
        public TileIndex(int3 tilePosition, in TilemapInfo tilemapInfo) :
            this(ToIndexAbsolute(tilePosition.x, tilePosition.y, tilePosition.z, tilemapInfo))
        {
        }

        /// <summary>
        ///     Get tilemap position from this index
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int3 GetTilemapPosition(in TilemapInfo tilemapInfo)
        {
            FromIndexAbsolute(value, tilemapInfo, out int3 result);
            return result;
        }

        /// <summary>
        ///     Get world location of this tile
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] public float3
            GetWorldPosition(in TilemapInfo tilemapInfo)
        {
            // Convert back into tilemap position
            FromIndexAbsolute(value, tilemapInfo, out int3 result);

            // Convert into world position
            result -= tilemapInfo.originPoint; // We move by origin point to get offset of the tile from origin
            return tilemapInfo.worldOriginPoint + tilemapInfo.tileSize * result;
        }

        /// <summary>
        /// Converts 3D coordinates (x, y, z) into a 1D tile index.
        /// Uses absolute coordinates of a tile
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToIndexAbsolute(in int3 tilePosition, in TilemapInfo tilemapInfo) =>
            ToIndexAbsolute(tilePosition.x, tilePosition.y, tilePosition.z, tilemapInfo);

        /// <summary>
        /// Converts 3D coordinates (x, y, z) into a 1D tile index.
        /// Uses absolute coordinates of a tile
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToIndexAbsolute(int x, int y, int z, in TilemapInfo tilemapInfo)
        {
            // Compute real tilemap offset, we subtract origin point
            // to orient index values around left bottom corner of Tilemap
            int3 tilemapOffset = new int3(x, y, z) - new int3(tilemapInfo.originPoint.x, tilemapInfo.originPoint.y,
                tilemapInfo.originPoint.z);

            return ToIndexRelative(tilemapOffset, tilemapInfo);
        }

        /// <summary>
        /// Converts 3D coordinates (x, y, z) into a 1D tile index.
        /// Uses relative coordinates of a tile
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToIndexRelative(int x, int y, int z, in TilemapInfo tilemapInfo)
            => ToIndexRelative(new int3(x, y, z), tilemapInfo);

        /// <summary>
        /// Converts 3D coordinates (x, y, z) into a 1D tile index.
        /// Uses relative coordinates of a tile
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToIndexRelative(in int3 tileOffset, in TilemapInfo tilemapInfo)
            => (tileOffset.x * tilemapInfo.size.y * tilemapInfo.size.z) +
               (tileOffset.y * tilemapInfo.size.z) + tileOffset.z;


        /// <summary>
        /// Converts 1D tile index back into absolute 3D coordinates (x, y, z).
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromIndexAbsolute(int index, in TilemapInfo tilemapInfo, out int3 tilePosition)
        {
            FromIndexAbsolute(index, tilemapInfo, out int x, out int y, out int z);

            // Re-apply origin
            tilePosition = new int3(x, y, z);
        }

        /// <summary>
        /// Converts 1D tile index back into absolute 3D coordinates (x, y, z).
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void FromIndexAbsolute(
            int index,
            in TilemapInfo tilemapInfo,
            out int x,
            out int y,
            out int z)
        {
            FromIndexRelative(index, tilemapInfo, out x, out y, out z);

            // Re-apply origin
            x += tilemapInfo.originPoint.x;
            y += tilemapInfo.originPoint.y;
            z += tilemapInfo.originPoint.z;
        }

        /// <summary>
        /// Converts 1D tile index back into relative 3D coordinates (x, y, z).
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void FromIndexRelative(
            int index,
            in TilemapInfo tilemapInfo,
            out int3 tileOffset)
        {
            FromIndexRelative(index, tilemapInfo, out int x, out int y, out int z);
            tileOffset = new int3(x, y, z);
        }

        /// <summary>
        /// Converts 1D tile index back into relative 3D coordinates (x, y, z).
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void FromIndexRelative(
            int index,
            in TilemapInfo tilemapInfo,
            out int x,
            out int y,
            out int z)
        {
            int sizeY = tilemapInfo.size.y;
            int sizeZ = tilemapInfo.size.z;

            // Decode offsets
            x = index / (sizeY * sizeZ);
            int remainder = index % (sizeY * sizeZ);
            y = remainder / sizeZ;
            z = remainder % sizeZ;
        }

        public static implicit operator int(TileIndex tileIndex) => tileIndex.value;

#region NEIGHBORS

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNorthTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(0, +1, 0, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSouthTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(0, -1, 0, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEastTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(+1, 0, 0, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWestTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(-1, 0, 0, tilemapInfo);
        
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNorthEastTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(+1, +1, 0, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNorthWestTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(-1, +1, 0, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSouthEastTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(+1, -1, 0, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSouthWestTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(-1, -1, 0, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetUpTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(0, 0, +1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDownTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(0, 0, -1, tilemapInfo);
        
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEastUpTileIndex(in TilemapInfo tilemapInfo) =>
            GetNeighborTileIndex(+1, 0, +1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEastDownTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(+1, 0, -1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWestUpTileIndex(in TilemapInfo tilemapInfo) =>
            GetNeighborTileIndex(-1, 0, +1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWestDownTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(-1, 0, -1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNorthUpTileIndex(in TilemapInfo tilemapInfo) =>
            GetNeighborTileIndex(0, +1, +1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNorthDownTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(0, +1, -1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSouthUpTileIndex(in TilemapInfo tilemapInfo) =>
            GetNeighborTileIndex(0, -1, +1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSouthDownTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(0, -1, -1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNorthEastUpTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(+1, +1, +1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNorthEastDownTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(+1, +1, -1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNorthWestUpTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(-1, +1, +1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNorthWestDownTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(-1, +1, -1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSouthEastUpTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(+1, -1, +1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSouthEastDownTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(+1, -1, -1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSouthWestUpTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(-1, -1, +1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSouthWestDownTileIndex(in TilemapInfo tilemapInfo)
            => GetNeighborTileIndex(-1, -1, -1, tilemapInfo);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNeighborTileIndex(int dx, int dy, int dz, in TilemapInfo tilemapInfo)
        {
            FromIndexRelative(value, tilemapInfo, out int x, out int y, out int z);

            int nx = x + dx;
            int ny = y + dy;
            int nz = z + dz;

            return Hint.Likely(nx >= 0 && nx < tilemapInfo.size.x && ny >= 0 && ny < tilemapInfo.size.y &&
                               nz >= 0 && nz < tilemapInfo.size.z)
                ? ToIndexRelative(nx, ny, nz, tilemapInfo)
                : NONE;
        }

#endregion
    }
}