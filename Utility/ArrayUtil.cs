using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Systems.Audibility2D.Utility
{
    [BurstCompile] public static class ArrayUtil
    {
        /// <summary>
        ///     Re-allocates array if necessary
        /// </summary>
        /// <remarks>
        ///     If array length is greater or equal to expected it simply clears the memory
        ///     otherwise disposes of old array and creates a new one.
        /// </remarks>
        [BurstCompile] public static void Reallocate<TDataType>(
            ref NativeArray<TDataType> source,
            int nLength,
            Allocator allocator)
            where TDataType : struct
        {
            // Clear memory array if same or greater length
            if (source.Length >= nLength)
            {
                unsafe
                {
                    UnsafeUtility.MemClear(
                        source.GetUnsafePtr(), 
                        nLength * UnsafeUtility.SizeOf<TDataType>());
                    return;
                }
            }

            source.Dispose();
            source = new NativeArray<TDataType>(nLength, allocator);
        }

        /// <summary>
        ///     Allocates array if length has changed, otherwise leaves old array to be cleaned up.
        /// </summary>
        [BurstCompile] public static void AllocateIfInvalid<TDataType>(
            ref NativeArray<TDataType> source,
            int nLength,
            Allocator allocator)
            where TDataType : struct
        {
            if (Hint.Unlikely(!source.IsCreated))
            {
                source = new NativeArray<TDataType>(nLength, allocator);
                return;
            }

            if (Hint.Likely(source.Length == nLength)) return;

            source.Dispose();
            source = new NativeArray<TDataType>(nLength, allocator);
        }
    }
}