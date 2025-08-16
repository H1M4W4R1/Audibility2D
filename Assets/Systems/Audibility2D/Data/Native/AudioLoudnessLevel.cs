using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility2D.Data.Native
{
    /// <summary>
    ///     Represents decibel level across four different frequencies
    ///     for better audio mapping
    /// </summary>
    /// <remarks>
    /// Frequencies: 20Hz, 200Hz, 2kHz, 20kHz
    /// </remarks>
    [Serializable] [StructLayout(LayoutKind.Explicit)] public struct AudioLoudnessLevel : IEquatable<AudioLoudnessLevel>
    {
        [FieldOffset(0)] private int4 vectorized;

        [FieldOffset(0)] [Tooltip("About 20Hz")] public int lowFrequency; // 32-bit
        [FieldOffset(4)] [Tooltip("About 200Hz")] public int mid0Frequency; // 32-bit
        [FieldOffset(8)] [Tooltip("About 2kHz")] public int mid1Frequency; // 32-bit
        [FieldOffset(12)] [Tooltip("About 20kHz")] public int highFrequency; // 32-bit

        /// <summary>
        ///     Internal constructor for vectorized creation of object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AudioLoudnessLevel(int4 vectorized)
        {
            lowFrequency = mid0Frequency = mid1Frequency = highFrequency = 0; // This will be overriden
            this.vectorized = vectorized;
        }
        
        /// <summary>
        ///     Create level from single value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AudioLoudnessLevel(int allFrequencies) : this(allFrequencies, allFrequencies, allFrequencies,
            allFrequencies)
        {
        }

        /// <summary>
        ///     Create level for all frequencies
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AudioLoudnessLevel(int lowFrequency, int mid0Frequency, int mid1Frequency, int highFrequency)
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
        public AudioLoudnessLevel MuffleAllFrequenciesBy(int muffleLevelAllFrequencies)
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
        public AudioLoudnessLevel MuffleBy(AudioLoudnessLevel muffleLevel)
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

        /// <summary>
        ///     Get maximum of two loudness values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] [BurstCompile]
        public static AudioLoudnessLevel Max(AudioLoudnessLevel a, AudioLoudnessLevel b)
        {
            return new AudioLoudnessLevel(math.max(a.vectorized, b.vectorized));
        }
        
        /// <summary>
        ///     Get minimum of two loudness values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] [BurstCompile]
        public static AudioLoudnessLevel Min(AudioLoudnessLevel a, AudioLoudnessLevel b)
        {
            return new AudioLoudnessLevel(math.min(a.vectorized, b.vectorized));
        }
        
        /// <summary>
        ///     Convert number into loudness, all frequencies will be set to desired loudness
        /// </summary>
        [BurstCompile] public static implicit operator AudioLoudnessLevel(byte audioLevel) => new(audioLevel);
        
        [BurstCompile] public static bool operator ==(AudioLoudnessLevel a, AudioLoudnessLevel b) => math.all(a.vectorized == b.vectorized);

        [BurstCompile] public static bool operator !=(AudioLoudnessLevel a, AudioLoudnessLevel b) => !(a == b);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(AudioLoudnessLevel other) => vectorized.Equals(other.vectorized);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is AudioLoudnessLevel other && Equals(other);
        
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => vectorized.GetHashCode();
        
    }
}