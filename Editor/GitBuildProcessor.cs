#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UnityEditorGitTool.Editor {
    /// <summary>
    /// Hooks up to the BuildProcess and calls Git.
    /// </summary>
    class GitBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log($"GitTools: OnPreprocessBuild: {report}");

            // Export git hash to text asset for runtime use.
            // Add a "+" to the hash to indicate that this was built without commiting pending changes.
            EditorGitTool.SaveHash();
        }
    }
}
#endif