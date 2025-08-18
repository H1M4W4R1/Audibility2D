#if UNITY_EDITOR
using JetBrains.Annotations;
using UnityEditor;

namespace Systems.Audibility2D.Data.Settings
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
                    EditorGUILayout.LabelField("Muffling Debug Settings", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(
                        so.FindProperty(nameof(AudibilitySettings.gizmosColorMinMuffling)));
                    EditorGUILayout.PropertyField(
                        so.FindProperty(nameof(AudibilitySettings.gizmosColorMaxMuffling)));

                    EditorGUILayout.LabelField("", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Audibility Debug Settings", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(
                        so.FindProperty(nameof(AudibilitySettings.gizmosColorMinAudibility)));
                    EditorGUILayout.PropertyField(
                        so.FindProperty(nameof(AudibilitySettings.gizmosColorMaxAudibility)));

                    so.ApplyModifiedProperties();
                }
            };

            return provider;
        }
    }
}
#endif