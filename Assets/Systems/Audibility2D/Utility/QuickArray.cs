using Unity.Burst;
using Unity.Collections;

namespace Systems.Audibility2D.Utility
{
    [BurstCompile]
    public static class QuickArray
    {
        /// <summary>
        ///     Allocates array if length has changed, otherwise leaves old array to be cleaned up.
        /// </summary>
        [BurstCompile]
        public static void PerformEfficientAllocation<TDataType>(
            ref NativeArray<TDataType> source,
            int nLength,
            Allocator allocator)
            where TDataType : struct
        {
            if (!source.IsCreated)
            {
                source = new NativeArray<TDataType>(nLength, allocator);
                return;
            }

            if (source.Length == nLength) return;

            source.Dispose();
            source = new NativeArray<TDataType>(nLength, allocator);
        }
        
    }
}