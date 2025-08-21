using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Systems.Audibility2D.Utility;
using UnityEngine;

namespace Systems.Audibility2D.Data.Native.Wrappers
{
    /// <summary>
    ///     Represents decibel level for easier audio mapping and falloff calculation
    /// </summary>
    [Serializable] [StructLayout(LayoutKind.Explicit)]
    public struct AudioLoudnessLevel : IEquatable<AudioLoudnessLevel> // 2B
    {
        [FieldOffset(0)] [Range(AudibilityTools.LOUDNESS_NONE, AudibilityTools.LOUDNESS_MAX)]
        [SerializeField]
        private short _value; // 2B

        /// <summary>
        ///     Create level from decibel value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public AudioLoudnessLevel(int loudnessDb)
        {
            //loudnessDb = math.clamp(loudnessDb, AudibilityTools.LOUDNESS_NONE, AudibilityTools.LOUDNESS_MAX);
            _value = (short) loudnessDb;
        }

        /// <summary>
        ///     Compute muffled decibel level
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public AudioLoudnessLevel MuffleBy(float muffleLevel)
        {
            _value -= (short) muffleLevel;
            return this;
        }

        /// <summary>
        ///     Compute muffled decibel level
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AudioLoudnessLevel MuffleBy(AudioLoudnessLevel muffleLevel)
        {
            _value -= muffleLevel._value;
            return this;
        }

        /// <summary>
        ///     Get value of this audio loudness level
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public short GetValue() => _value;

        /// <summary>
        ///     Get maximum of two loudness values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AudioLoudnessLevel Max(AudioLoudnessLevel a, AudioLoudnessLevel b)
            => a._value > b._value ? a : b;

        /// <summary>
        ///     Get minimum of two loudness values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AudioLoudnessLevel Min(AudioLoudnessLevel a, AudioLoudnessLevel b)
            => a._value < b._value ? a : b;

        /// <summary>
        ///     Convert number into loudness
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AudioLoudnessLevel(int audioLevel)
        {
            return new AudioLoudnessLevel((short) audioLevel);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator short(AudioLoudnessLevel audioLoudnessLevel) => audioLoudnessLevel._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(AudioLoudnessLevel a, AudioLoudnessLevel b)
            => a._value == b._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(AudioLoudnessLevel a, AudioLoudnessLevel b) => !(a == b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Equals(AudioLoudnessLevel other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override bool Equals(object obj)
            => obj is AudioLoudnessLevel other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override int GetHashCode()
            => _value.GetHashCode();
    }
}