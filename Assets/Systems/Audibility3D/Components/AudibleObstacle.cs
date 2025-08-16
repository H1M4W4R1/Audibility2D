using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.Audibility.Common.Data;
using UnityEngine;

namespace Systems.Audibility3D.Components
{
    /// <summary>
    ///     Obstacle that reduces audibility by muffling it via specified amount
    /// </summary>
    /// <remarks>
    ///     Intended to operate in 3D, but raycast hit-scanning is seriously laggy, so it's not used
    ///     at this moment.
    /// </remarks>
    public sealed class AudibleObstacle : MonoBehaviour
    {
        /// <summary>
        ///     Material obstacle is made of, representing sound dampening properties
        /// </summary>
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