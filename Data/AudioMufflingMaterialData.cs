using System.Runtime.CompilerServices;
using Systems.Audibility2D.Components;
using Systems.Audibility2D.Data.Native.Wrappers;
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private set;
        }

#if UNITY_EDITOR
        private void NotifyMaterialDataChangeToAudibilityUpdaters()
        {
            AudibilityUpdater[] updaters =
                FindObjectsByType<AudibilityUpdater>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int n = 0; n < updaters.Length; n++)
                updaters[n].OnMufflingMaterialDataChangedHandler(this);
        }

        
        private void OnValidate()
        {
            NotifyMaterialDataChangeToAudibilityUpdaters();
        }
#endif
    }
}