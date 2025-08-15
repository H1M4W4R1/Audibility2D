using UnityEngine;

namespace Systems.Audibility3D.Data
{
    [CreateAssetMenu(menuName = "Systems/Audibility/MuffleMaterialData", fileName = "MuffleMaterialData")]
    public sealed class AudioMufflingMaterialData : ScriptableObject
    {
        [field:SerializeField] public DecibelLevel MuffleLevel { get; private set; }
    }
}