using Unity.Burst;
using Unity.Mathematics;

namespace Systems.Audibility2D.Data.Native
{
    /// <summary>
    ///     Core information about 2-dimensional grid
    /// </summary>
    public readonly struct GridInfo2D
    {
        public readonly int2 originPoint; // 8B
        public readonly int2 size; // 8B
        public readonly float3 worldOriginPoint; // 8B
        public readonly float2 tileSize; // 8B
        public readonly float diagonalDistance;
        
        public GridInfo2D(int2 originPoint, int2 size, float3 worldOriginPoint, float2 tileSize)
        {
            this.originPoint = originPoint;
            this.size = size;
            this.worldOriginPoint = worldOriginPoint;
            this.tileSize = tileSize;
            diagonalDistance = math.length(tileSize);
        }

        /// <summary>
        ///     Computes world position of a node (using relative input)
        /// </summary>
        [BurstCompile] public float3 GetWorldPositionRelative(int2 relativeCellPosition)
            => worldOriginPoint + new float3(tileSize * relativeCellPosition, 0);

        /// <summary>
        ///     Computes world position of a node (using absolute input)
        /// </summary>
        [BurstCompile] public float3 GetWorldPositionAbsolute(int2 absoluteCellPosition) =>
            GetWorldPositionRelative(absoluteCellPosition - originPoint);
        
        /// <summary>
        ///     Computes relative position from world one
        /// </summary>
        [BurstCompile] public int2 GetRelativePositionFromWorld(float3 worldPosition)
        {
            // Transform world position into grid-local space
            float2 local = (worldPosition - worldOriginPoint).xy / tileSize;

            // Round/truncate to nearest integer cell index
            return (int2) math.round(local);
        }
        
        /// <summary>
        ///     Computes absolute position from world one
        /// </summary>
        [BurstCompile]
        public int2 GetAbsolutePositionFromWorld(float3 worldPosition)
        {
            // Get relative first
            int2 relative = GetRelativePositionFromWorld(worldPosition);

            // Convert to absolute by adding origin offset
            return relative + originPoint;
        }
    }
}