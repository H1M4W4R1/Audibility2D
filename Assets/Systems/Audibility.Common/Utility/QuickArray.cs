using Unity.Burst;
using Unity.Collections;

namespace Systems.Audibility.Common.Utility
{
    [BurstCompile]
    public static class QuickArray
    {
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