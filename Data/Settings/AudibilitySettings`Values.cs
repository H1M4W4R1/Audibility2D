using UnityEngine;

namespace Systems.Audibility2D.Data.Settings
{
    /// <summary>
    ///     Global audibility settings for all known audibility solutions
    /// </summary>
    public sealed partial class AudibilitySettings
    {
        public Color gizmosColorMinMuffling = Color.red;
        public Color gizmosColorMaxMuffling = Color.green;
        public Color gizmosColorMinAudibility = Color.red;
        public Color gizmosColorMaxAudibility = Color.green;
    }
}