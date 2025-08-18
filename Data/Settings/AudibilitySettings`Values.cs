using System;
using UnityEngine;

namespace Systems.Audibility2D.Data.Settings
{
    /// <summary>
    ///     Global audibility settings for all known audibility solutions
    /// </summary>
    [Serializable]
    public sealed partial class AudibilitySettings
    {
        [SerializeField] public Color gizmosColorMinMuffling = Color.red;
        [SerializeField] public Color gizmosColorMaxMuffling = Color.green;
        [SerializeField] public Color gizmosColorMinAudibility = Color.red;
        [SerializeField] public Color gizmosColorMaxAudibility = Color.green;
    }
}