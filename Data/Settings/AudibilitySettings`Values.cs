using System;
using Systems.Audibility2D.Utility;
using UnityEngine;

namespace Systems.Audibility2D.Data.Settings
{
    /// <summary>
    ///     Global audibility settings for all known audibility solutions
    /// </summary>
    [Serializable] public sealed partial class AudibilitySettings
    {
        [SerializeField] public Color gizmosColorMinMuffling = Color.red;
        [SerializeField] public Color gizmosColorMaxMuffling = Color.green;
        [SerializeField] public Color gizmosColorMinAudibility = Color.red;
        [SerializeField] public Color gizmosColorMaxAudibility = Color.green;
        [SerializeField] [Tooltip("How many decibels will decay per unit of distance")]
        [Range(AudibilityTools.LOUDNESS_NONE, AudibilityTools.LOUDNESS_MAX)]
        public short soundDecayPerUnit = (short) (AudibilityTools.LOUDNESS_MAX * 1 / 25f);
    }
}