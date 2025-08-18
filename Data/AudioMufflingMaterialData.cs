using System;
using System.Runtime.CompilerServices;
using Systems.Audibility2D.Data.Native.Wrappers;
using Systems.Audibility2D.Utility.Internal;
using Unity.Burst;
using UnityEngine;

namespace Systems.Audibility2D.Data
{
    /// <summary>
    ///     Object storing data about muffling effect of specific material. Used to easily modify muffling settings
    ///     across entire game world without going to every single object.
    /// </summary>
    [CreateAssetMenu(menuName = "Systems/Audibility/MuffleMaterialData", fileName = "MuffleMaterialData")]
    public sealed class AudioMufflingMaterialData : ScriptableObject
    {
        /// <summary>
        ///     Muffle level - how much sound will be dampened when reaching object made of this material.
        /// </summary>
        [field: SerializeField]
        [Tooltip("Sound loudness will be reduced by this value when reaching this material")]
        public AudioLoudnessLevel MuffleLevel
        {
            [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] get;
            [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] private set;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Events.OnMufflingMaterialDataChanged?.Invoke(this);
        }
#endif
    }
}