#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditorGitTool.Editor
{
    /// <summary>
    /// Saves the git hash into a text asset.
    /// </summary>
    public static class EditorGitTool
    {
        public const string DefaultGitHashFilePath = "Assets/Resources/GitHash.asset";

        /// <summary>
        /// Update the hash from the menu.
        /// </summary>
        [MenuItem("Tools/Git/SaveHash")]
        public static void SaveHashFromMenu()
        {
            SaveHash();
        }

        /// <summary>
        /// Fetch the hash from git, add postFix to it and then save it in gitHashFilePath.
        /// </summary>
        /// <param name="postFix">Text to be appended to the hash.</param>
        /// <param name="gitHashFilePath">Git hashfile path, will use the path set the settings if not specified. Example: "Assets/Resources/GitHash.asset"</param>
        public static void SaveHash()
        {
            var gitHashFilePath = GitToolSettings.GetOrCreateSettings().GitHashTextAssetPath;

            Debug.Log("GitTools: PreExport() - writing git hash into '" + gitHashFilePath + "'");

            string gitDetails = ExecAndReadFirstLine("git rev-parse --short HEAD");
            if (gitDetails == null)
            {
                Debug.LogError("GitTools: not git hash found!");
                gitDetails = "unknown";
            }

            gitDetails = $"{gitDetails}\n{DateTime.Now:yyyy.MM.dd HH:mm}";

            Debug.Log("GitTools: git hash is '" + gitDetails + "'");

            AssetDatabase.DeleteAsset(gitHashFilePath);
            var text = new TextAsset(gitDetails);
            AssetDatabase.CreateAsset(text, gitHashFilePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Counts modified, new or simply unknown files in working tree.
        /// </summary>
        /// <returns></returns>
        public static int CountChanges()
        {
            Debug.Log("GitTools: CountChanges() - counts modified, new or simply unknown files in working tree.");

            string statusResult = Exec("git status --porcelain");
            if (statusResult == null)
            {
                return 0;
            }

            return countLines(statusResult);
        }

        private static int countLines(string str)
        {
            if (str == null)
                throw new ArgumentNullException("str");
            if (str == string.Empty)
                return 0;
            int index = -1;
            int count = 0;
            while ( (index = str.IndexOf(Environment.NewLine, index + 1)) != -1 )
            {
                count++;
            }

            return count + 1;
        }
		
        public static string ExecAndReadFirstLine(string command, int maxWaitTimeInSec = 5)
        {
            string result = Exec(command, maxWaitTimeInSec);

            // first line only
            if (result != null)
            {
                int i = result.IndexOf("\n");
                if (i > 0)
                {
                    result = result.Substring(0, i);
                }
            }

            return result;
        }

        public static string Exec(string command, int maxWaitTimeInSec = 5)
        {
            try
            {
#if UNITY_EDITOR_WIN
                string shellCmd = "cmd.exe";
                string shellCmdArg = "/c";
#elif UNITY_EDITOR_OSX
			    string shellCmd = "bash";
                string shellCmdArg = "-c";
#endif

                string cmdArguments = shellCmdArg + " \"" + command + "\"";
                Debug.Log("GitTool.Exec: Attempting to execute command: " + (shellCmd + " " + cmdArguments));
                var procStartInfo = new System.Diagnostics.ProcessStartInfo(shellCmd, cmdArguments);
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;

                // Debug.Log("GitTool.Exec: Running process...");
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                proc.WaitForExit(maxWaitTimeInSec * 1000);
                string result = proc.StandardOutput.ReadToEnd();

                Debug.Log("GitTool.Exec: done");
                return result;
            }
            catch (System.Exception e)
            {
                Debug.Log("GitTool.Exec Error: " + e);
                return null;
            }
        }
    }
}
#endif