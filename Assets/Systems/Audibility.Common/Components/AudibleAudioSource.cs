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
        [SerializeField] private DecibelLevel decibelLevel = Loudness.MAX;

        /// <summary>
        ///     Audio source on this object
        /// </summary>
        private AudioSource _audioSource;

        /// <summary>
        ///     Cached transform to reduce computation time
        /// </summary>
        private Transform _transform;
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

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] public DecibelLevel GetDecibelLevel() => decibelLevel;
    }
}