using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;

namespace Systems.Audibility2D.Data.Native
{
    [StructLayout(LayoutKind.Explicit)]
    [BurstCompile]
    public struct AudioTileNeighbourData
    {
        public const int MAX_INDEX = 8;
        
        [FieldOffset(0)] public int4x2 vectorized;
        
        [FieldOffset(0)] public int tile0;
        [FieldOffset(4)] public int tile1;
        [FieldOffset(8)] public int tile2;
        [FieldOffset(12)] public int tile3;
        [FieldOffset(16)] public int tile4;
        [FieldOffset(20)] public int tile5;
        [FieldOffset(24)] public int tile6;
        [FieldOffset(28)] public int tile7;
     
        public unsafe int this[int index]
        {
            [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // Prevent out of bounds, optimized-out
                if (Hint.Unlikely(index is < 0 or >= MAX_INDEX)) return -1;
                
                fixed (int* startPtr = &tile0)
                {
                    return *(startPtr + index);
                }
            }

            [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                // Prevent out of bounds, optimized-out
                if (Hint.Unlikely(index is < 0 or >= MAX_INDEX)) return;
                
                fixed (int* startPtr = &tile0)
                {
                    *(startPtr + index) = value;
                }
            }
        }

        /// <summary>
        ///     Creates new instance of this class and sets-up all tiles to be -1 (not existing).
        ///     
        /// </summary>
        public static AudioTileNeighbourData New()
        {
            return new AudioTileNeighbourData()
            {
                tile0 = -1,
                tile1 = -1,
                tile2 = -1,
                tile3 = -1,
                tile4 = -1,
                tile5 = -1,
                tile6 = -1,
                tile7 = -1
            };
        }
        
    }
}