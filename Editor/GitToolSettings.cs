using UnityEditor;
using UnityEngine;

namespace UnityEditorGitTool.Editor {
    // Create a new type of Settings Asset.
    class GitToolSettings : ScriptableObject {
        public const string SettingsFilePath = "Assets/Editor/GitToolSettings.asset";

        [SerializeField] public string GitHashTextAssetPath;

        internal static GitToolSettings GetOrCreateSettings() {
            var settings = AssetDatabase.LoadAssetAtPath<GitToolSettings>(SettingsFilePath);
            if(settings == null) {
                settings = ScriptableObject.CreateInstance<GitToolSettings>();
                settings.GitHashTextAssetPath = EditorGitTool.DefaultGitHashFilePath;
                AssetDatabase.CreateAsset(settings, SettingsFilePath);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        internal static SerializedObject GetSerializedSettings() {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    // Register a SettingsProvider using IMGUI for the drawing framework:
    static class GitToolSettingsProvider {
        [SettingsProvider]
        public static SettingsProvider CreateGitToolSettingsProvider() {
            var provider = new SettingsProvider("Project/GitTool", SettingsScope.Project) {
                label = "Git Tool",
                guiHandler = (searchContext) => {
                    var settings = GitToolSettings.GetSerializedSettings();
                    EditorGUILayout.PropertyField(settings.FindProperty("GitHashTextAssetPath"), new GUIContent("Hash file path:"));
                    EditorGUILayout.HelpBox("Defines the path where the hash is stored as a text asset file.\n" +
                                            "If you want to load this at runtime then store it in Resources, example: 'Assets/Resources/GitHash.asset'.\n" +
                                            "\nLoad with :\n" +
                                            "  var gitHash = UnityEngine.Resources.Load<TextAsset>(\"GitHash\"); \n" +
                                            "  if (gitHash != null)\n" +
                                            "  {\n" +
                                            "      string versionHash = gitHash.text;\n" +
                                            "  }\n", MessageType.None);
                    settings.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting.
                keywords = new System.Collections.Generic.HashSet<string>(new[] {"git", "hash", "build"})
            };

            return provider;
        }
    }
}