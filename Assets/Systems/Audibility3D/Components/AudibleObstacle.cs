using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.Audibility3D.Data;
using UnityEngine;

namespace Systems.Audibility3D.Components
{
    /// <summary>
    ///     Obstacle that reduces audibility by muffling it via specified amount
    /// </summary>
    public sealed class AudibleObstacle : MonoBehaviour
    {
        [SerializeField] private AudioMufflingMaterialData audioMaterialData;

        /// <summary>
        ///     Set material used in this muffling obstacle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMufflingLevel([NotNull] AudioMufflingMaterialData materialData) =>
            audioMaterialData = materialData;

        /// <summary>
        ///     Get muffling strength of this obstacle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DecibelLevel GetMufflingLevel() => audioMaterialData.MuffleLevel;
    }
}