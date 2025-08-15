using System;
using System.Runtime.CompilerServices;
using Systems.Audibility.Common.Utility;
using Systems.Audibility2D.Utility;
using Unity.Burst;
using UnityEngine;

namespace Systems.Audibility.Common.Data
{
    [CreateAssetMenu(menuName = "Systems/Audibility/MuffleMaterialData", fileName = "MuffleMaterialData")]
    public sealed class AudioMufflingMaterialData : ScriptableObject
    {
        [field: SerializeField] public DecibelLevel MuffleLevel
        {
            [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] get;
            [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] private set;
        }

        private void OnValidate()
        {
            // TODO: Do something with this crap
            AudibilitySystem2D.SetDirtyAll(true);
        }
    }
}