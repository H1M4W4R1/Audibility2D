#if UNITY_EDITOR
using JetBrains.Annotations;
using UnityEditor;

namespace Systems.Audibility2D.Settings
{
    /// <summary>
    ///     Editor for audibility settings
    /// </summary>
    internal static class AudibilitySettingsEditor
    {
        private const string EDITOR_NAME = "Audibility Settings";
        
        [SettingsProvider] [NotNull] public static SettingsProvider CreateAudibilitySettingsProvider()
        {
            SettingsProvider provider = new($"Project/{EDITOR_NAME}", SettingsScope.Project)
            {
                label = EDITOR_NAME,

                guiHandler = (searchContext) =>
                {
                    AudibilitySettings settings = AudibilitySettings.Instance;

                    SerializedObject so = new(settings);
                    /*EditorGUILayout.PropertyField(so.FindProperty("masterVolume"));
                    EditorGUILayout.PropertyField(so.FindProperty("enableOcclusion"));
                    EditorGUILayout.PropertyField(so.FindProperty("maxHearingDistance"));
                    EditorGUILayout.PropertyField(so.FindProperty("falloffCurve"));
                    EditorGUILayout.PropertyField(so.FindProperty("showDebugGizmos"));*/
                    
                    // TODO: Implement settings
                    
                    so.ApplyModifiedProperties();
                }
            };

            return provider;
        }
    }
}
#endif