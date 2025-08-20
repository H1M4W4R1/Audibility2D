using System.Runtime.CompilerServices;
using Systems.Audibility2D.Data.Native.Wrappers;
using Systems.Audibility2D.Utility;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility2D.Components
{
    /// <summary>
    ///     Represents audible sound - one that can be received by any audibility sampler method.
    ///     In most cases this should be implemented on most Audio Sources in the game (maybe except music)
    /// </summary>
    public sealed class AudibleSound : MonoBehaviour
    {
        /// <summary>
        ///     Level of this audio source also known how loud this thing should relatively be as
        ///     gunshots can be louder than whispering.
        /// </summary>
        [SerializeField] [Tooltip("How loud is this audio source (dB)")] 
        private AudioLoudnessLevel audioLoudnessLevel = AudibilityTools.LOUDNESS_MAX;

        /// <summary>
        ///     Range of audio sound in 3D space
        /// </summary>
        [SerializeField] [Tooltip("Audio sound range")]
        private float range;
        
        /// <summary>
        ///     Cached transform to reduce computation time
        /// </summary>
        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        /// <summary>
        ///     Get range of this audio source
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public float GetRange() => range;
        
        /// <summary>
        ///     Get position of this audio source
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float3 GetPosition()
        {
            if(ReferenceEquals(_transform, null)) _transform = transform;
            return _transform.position;
        }
        
        /// <summary>
        ///     Change decibel level of this source
        /// </summary>
        /// <param name="newAudioLoudnessLevel">Decibel level of this source</param>
        public void SetDecibelLevel(AudioLoudnessLevel newAudioLoudnessLevel) => audioLoudnessLevel = newAudioLoudnessLevel;

        /// <summary>
        ///     Get loudness of this audio source in dB
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public AudioLoudnessLevel GetDecibelLevel() => audioLoudnessLevel;
    }
}