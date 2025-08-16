using System.Runtime.CompilerServices;
using Systems.Audibility.Common.Data;
using Systems.Audibility.Common.Utility;
using Unity.Burst;
using UnityEngine;

namespace Systems.Audibility.Common.Components
{
    /// <summary>
    ///     Represents audible audio source - audio source that affects audibility maps.
    ///     In most cases this should be implemented on most Audio Sources in the game (maybe except music)
    /// </summary>
    [RequireComponent(typeof(AudioSource))] public sealed class AudibleAudioSource : MonoBehaviour
    {
        /// <summary>
        ///     Level of this audio source in four basic frequencies.
        ///     Also known how loud this thing should relatively be - gunshots can be louder
        ///     than regular whispering.
        /// </summary>
        [SerializeField] [Tooltip("How loud is this audio source (dB)")] 
        private DecibelLevel decibelLevel = Loudness.MAX;

        /// <summary>
        ///     Audio source on this object
        /// </summary>
        private AudioSource _audioSource;

        /// <summary>
        ///     Cached transform to reduce computation time
        /// </summary>
        private Transform _transform;
        
        /// <summary>
        ///     Reference to Unity audio source accessible from other scripts
        /// </summary>
        public AudioSource UnitySourceReference
        {
            get
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (!_audioSource) _audioSource = GetComponent<AudioSource>();
                return _audioSource;
            }
        }

        private void Awake()
        {
            _transform = transform;
            _audioSource = GetComponent<AudioSource>();
            _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }

        /// <summary>
        ///     Change decibel level of this source
        /// </summary>
        /// <param name="newDecibelLevel">Decibel level of this source</param>
        public void SetDecibelLevel(DecibelLevel newDecibelLevel) => decibelLevel = newDecibelLevel;

        /// <summary>
        ///     Get loudness of this audio source in dB (for four basic frequencies)
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] public DecibelLevel GetDecibelLevel() => decibelLevel;
    }
}