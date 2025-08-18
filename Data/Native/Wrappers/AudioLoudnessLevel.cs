using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Systems.Audibility2D.Data.Native.Wrappers
{
    /// <summary>
    ///     Represents decibel level for easier audio mapping and falloff calculation
    /// </summary>
    [Serializable] [StructLayout(LayoutKind.Explicit)]
    public struct AudioLoudnessLevel : IEquatable<AudioLoudnessLevel> // 2B
    {
        [FieldOffset(0)] public short value; // 2B

        /// <summary>
        ///     Create level from decibel value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public AudioLoudnessLevel(int loudnessDb)
        {
            loudnessDb = math.clamp(loudnessDb, short.MinValue, short.MaxValue);
            value = (short) loudnessDb;
        }

        /// <summary>
        ///     Compute muffled decibel level
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] [BurstCompile]
        public AudioLoudnessLevel MuffleBy(float muffleLevel)
        {
            // We convert it to non-decimal numbers to improve computation precision in desired range
            // and get an insignificant boost performance
            value -= (short) math.clamp((int) muffleLevel, short.MinValue, short.MaxValue);
            return this;
        }
        
        /// <summary>
        ///     Compute muffled decibel level
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] [BurstCompile]
        public AudioLoudnessLevel MuffleBy(AudioLoudnessLevel muffleLevel)
        {
            value -= muffleLevel.value;
            return this;
        }

        /// <summary>
        ///     Get average decibel level, kept for compatibility
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] [BurstCompile] public short GetAverage() => value;

        /// <summary>
        ///     Get maximum of two loudness values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] [BurstCompile]
        public static AudioLoudnessLevel Max(AudioLoudnessLevel a, AudioLoudnessLevel b)
            => math.max(a.value, b.value);

        /// <summary>
        ///     Get minimum of two loudness values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] [BurstCompile]
        public static AudioLoudnessLevel Min(AudioLoudnessLevel a, AudioLoudnessLevel b)
            => math.min(a.value, b.value);

        /// <summary>
        ///     Convert number into loudness
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AudioLoudnessLevel(int audioLevel)
        {
            audioLevel = math.clamp(audioLevel, short.MinValue, short.MaxValue);
            return new AudioLoudnessLevel((short) audioLevel);
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator short(AudioLoudnessLevel audioLoudnessLevel) => audioLoudnessLevel.value;

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(AudioLoudnessLevel a, AudioLoudnessLevel b)
            => a.value == b.value;

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(AudioLoudnessLevel a, AudioLoudnessLevel b) => !(a == b);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(AudioLoudnessLevel other) => value == other.value;

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] public override bool Equals(object obj)
            => obj is AudioLoudnessLevel other && Equals(other);

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] public override int GetHashCode()
            => value.GetHashCode();
    }
}