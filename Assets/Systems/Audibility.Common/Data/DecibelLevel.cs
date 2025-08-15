using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility.Common.Data
{
    /// <summary>
    ///     Represents decibel level across four different frequencies
    ///     for better audio mapping
    /// </summary>
    [Serializable] [StructLayout(LayoutKind.Explicit)] public struct DecibelLevel
    {
        [FieldOffset(0)] private int4 vectorized;

        [FieldOffset(0)] [Tooltip("About 20Hz")] public int lowFrequency; // 32-bit
        [FieldOffset(4)] [Tooltip("About 200Hz")] public int mid0Frequency; // 32-bit
        [FieldOffset(8)] [Tooltip("About 2kHz")] public int mid1Frequency; // 32-bit
        [FieldOffset(12)] [Tooltip("About 20kHz")] public int highFrequency; // 32-bit

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DecibelLevel(int4 vectorized)
        {
            lowFrequency = mid0Frequency = mid1Frequency = highFrequency = 0; // This will be overriden
            this.vectorized = vectorized;
        }
        
        /// <summary>
        ///     Create level from single value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DecibelLevel(int allFrequencies) : this(allFrequencies, allFrequencies, allFrequencies,
            allFrequencies)
        {
        }

        /// <summary>
        ///     Create level for all frequencies
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DecibelLevel(int lowFrequency, int mid0Frequency, int mid1Frequency, int highFrequency)
        {
            vectorized = 0; // Will be overriden anyway
            this.lowFrequency = lowFrequency;
            this.mid0Frequency = mid0Frequency;
            this.mid1Frequency = mid1Frequency;
            this.highFrequency = highFrequency;
        }

        /// <summary>
        ///     Compute muffled decibel level
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        [BurstCompile]
        public DecibelLevel MuffleAllFrequenciesBy(byte muffleLevelAllFrequencies)
        {
            int4 vectorizedMuffle = new(muffleLevelAllFrequencies, muffleLevelAllFrequencies,
                muffleLevelAllFrequencies, muffleLevelAllFrequencies);
            vectorizedMuffle = math.min(vectorized, vectorizedMuffle);

            vectorized -= vectorizedMuffle;
            return this;
        }
        
        /// <summary>
        ///     Compute muffled decibel level
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] [BurstCompile]
        public DecibelLevel MuffleBy(DecibelLevel muffleLevel)
        {
            vectorized -= math.min(vectorized, muffleLevel.vectorized);
            return this;
        }

        /// <summary>
        ///     Get average decibel level from all frequencies
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] [BurstCompile]
        public int GetAverage()
        {
            int total = lowFrequency + mid0Frequency + mid1Frequency + highFrequency;
            return total / 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] [BurstCompile]
        public static DecibelLevel Max(DecibelLevel a, DecibelLevel b)
        {
            return new DecibelLevel(math.max(a.vectorized, b.vectorized));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] [BurstCompile]
        public static DecibelLevel Min(DecibelLevel a, DecibelLevel b)
        {
            return new DecibelLevel(math.min(a.vectorized, b.vectorized));
        }
        
        [BurstCompile] public static implicit operator DecibelLevel(byte audioLevel) => new(audioLevel);
        
        
        [BurstCompile] public static bool operator ==(DecibelLevel a, DecibelLevel b)
        {
            return math.all(a.vectorized == b.vectorized);
        }

        [BurstCompile] public static bool operator !=(DecibelLevel a, DecibelLevel b)
        {
            return !(a == b);
        }
    }
}