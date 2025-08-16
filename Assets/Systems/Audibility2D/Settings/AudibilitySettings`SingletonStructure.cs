using System.IO;
using UnityEngine;

namespace Systems.Audibility2D.Settings
{
    /// <summary>
    ///     Global audibility settings for all known audibility solutions
    /// </summary>
    public sealed partial class AudibilitySettings : ScriptableObject
    {
        private const string RESOURCES_PATH = "AudibilitySettings3D";
        private static AudibilitySettings _instance;

        /// <summary>
        ///     Instance of <see cref="AudibilitySettings"/>
        /// </summary>
        public static AudibilitySettings Instance
        {
            get
            {
                if (!_instance) _instance = LoadOrCreateSettings();
                return _instance;
            }
        }

        /// <summary>
        ///     If settings are missing we attempt to load or create them
        /// </summary>
        private static AudibilitySettings LoadOrCreateSettings()
        {
            const string PATH = "Assets/Resources/" + RESOURCES_PATH + ".asset";
            
            // Load from Resources in runtime
            AudibilitySettings settings = Resources.Load<AudibilitySettings>(RESOURCES_PATH);

#if UNITY_EDITOR
            // If not found, auto-create in Editor
            if (settings == null)
            {
                // Create instance of settings
                settings = CreateInstance<AudibilitySettings>();
                if (!Directory.Exists("Assets/Resources")) Directory.CreateDirectory("Assets/Resources");
                UnityEditor.AssetDatabase.CreateAsset(settings, PATH);
                UnityEditor.AssetDatabase.SaveAssets();
                Debug.Log("[Audibility3D] Created default AudibilitySettings3D at " + PATH);
            }
#endif

            // If still null (e.g., stripped Resources), create a runtime default
            if (!settings) settings = CreateInstance<AudibilitySettings>();

            return settings;
        }
    }
}