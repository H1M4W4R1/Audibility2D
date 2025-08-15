using System.Runtime.CompilerServices;
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
    }
}