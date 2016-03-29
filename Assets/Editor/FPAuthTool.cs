using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;
using System.Diagnostics;
using System.Xml;

namespace FPAuth
{
    public class FPAuthTool : EditorWindow
    {
        public static readonly string AMAZON_BUILD_DIR = "AmazonAuth/Assets/Plugins/Android/Amazon";
        public static readonly string AMAZON_PLUGIN_DIR = "Assets/Plugins/Android/Amazon";
        public static readonly string[] DEFINE_SYMBOLS = new string[]
        {
            "DEVELOPER_TOOLS", "KINDLE_BUILD", "NO_GPGS"    
        };

        //        private event BuildFinishedDelegate BuildFinishedEvent;

        private Process process = null;

        string bundleId;
        string androidHome;
        string amazonAPIKey;
        bool debugBuild;
        Platform platform = Platform.None;
        string outputLocation;

        public enum Platform
        {
            None,
            iOS,
            Android,
            Kindle
        }

        [MenuItem("Window/FPAuth")]
        public static void  ShowWindow()
        {
            EditorWindow.GetWindow(typeof(FPAuthTool));
        }

        void OnDestroy()
        {
            if (process != null)
            {
                try
                {
                    process.Kill();
                }
                catch (InvalidOperationException e)
                {
                    // do nothing, it was already killed
                    UnityEngine.Debug.LogError(e);
                }
                finally
                {
                    process.Dispose();
                }
            }
        }

        void OnGUI()
        {
            EditorStyles.textField.wordWrap = true;

            GUILayout.Label("FPAuthTool Settings", EditorStyles.boldLabel);
            bundleId = EditorGUILayout.TextField("Bundle Identifier", bundleId);
            androidHome = EditorGUILayout.TextField("Android SDK Location", androidHome);
            amazonAPIKey = EditorGUILayout.TextField("Amazon API Key", amazonAPIKey, GUILayout.Height(200));

            EditorGUILayout.Space();
            GUILayout.BeginArea(new Rect(153, 270, 200, 100));
            if (GUILayout.Button("Setup", GUILayout.Width(200)))
            {
                if (string.IsNullOrEmpty(bundleId) || string.IsNullOrEmpty(androidHome) ||
                    string.IsNullOrEmpty(amazonAPIKey))
                {
                    UnityEngine.Debug.Log("All input values have to be set");
                }
                else
                {
                    BuildProject();
                }
            }
            GUILayout.EndArea();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Separator();

            debugBuild = EditorGUILayout.Toggle("Debug Build", debugBuild);
            GUIContent label = new GUIContent();
            label.text = "Platform";

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            platform = (Platform)EditorGUILayout.EnumPopup(platform, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            outputLocation = EditorGUILayout.TextField("Output Location", outputLocation);
            GUI.SetNextControlName("Browse");
            if (GUILayout.Button("Browse"))
            {
                outputLocation = EditorUtility.SaveFolderPanel("Select Location", outputLocation, "Builds");
                GUI.FocusControl("Browse");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.BeginArea(new Rect(153, 353, 200, 100));
            if (GUILayout.Button("Build", GUILayout.Width(200)) && platform != Platform.None && !string.IsNullOrEmpty(outputLocation))
            {
                BuildGame(platform);
            }
            GUILayout.EndArea();
        }

        private void BuildProject()
        {
            UnityEngine.Debug.ClearDeveloperConsole();
            UnityEngine.Debug.Log("Starting...");

            BuildAmazonPlugin();

            UnityEngine.Debug.Log("Finished!");
        }

        private void BuildAmazonPlugin()
        {
            RunCommand(Application.dataPath + "/../AmazonAuth/gradlew", "AmazonAuth/", "makeUnityPlugin");

            if (Directory.Exists(AMAZON_PLUGIN_DIR))
            {
                Directory.Delete(AMAZON_PLUGIN_DIR, true);
            }
            Directory.CreateDirectory(AMAZON_PLUGIN_DIR);

            DirectoryCopy(AMAZON_BUILD_DIR, AMAZON_PLUGIN_DIR);

            // Add bundle id to the manifest
            XmlDocument doc = new XmlDocument();
            doc.Load(AMAZON_PLUGIN_DIR + "/AndroidManifest.xml");

            XmlNode node = doc.SelectSingleNode("manifest/application/activity/intent-filter/data");

            XmlAttribute nodeAttr = node.Attributes["android:host"];

            if (nodeAttr != null)
            {
                nodeAttr.Value = bundleId;
                doc.Save(AMAZON_PLUGIN_DIR + "/AndroidManifest.xml");
            }

            // output api key
            string apiKeyFilename = "Assets/Plugins/Android/assets/api_key.txt";
            if (File.Exists(apiKeyFilename))
            {
                File.Delete(apiKeyFilename);
            }

            StreamWriter sw = File.CreateText(apiKeyFilename);
            sw.Write(amazonAPIKey);
            sw.Close();
        }

        private void DirectoryCopy(string sourceDirName, string destDirName)
        {
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);

                // set permissions
                System.IO.File.SetAttributes(temppath, System.IO.File.GetAttributes(temppath) & ~(System.IO.FileAttributes.ReadOnly));
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                // Create the subdirectory.
                string temppath = Path.Combine(destDirName, subdir.Name);

                // Copy the subdirectories.
                DirectoryCopy(subdir.FullName, temppath);
            }
        }

        private void RunCommand(string command, string directory, string arguments)
        {
            UnityEngine.Debug.Log(string.Format("Starting running command {0} with args {1}", command, arguments));

            process = new Process();
            process.StartInfo.WorkingDirectory = directory;
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.EnvironmentVariables["ANDROID_HOME"] = androidHome;

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            string err = process.StandardError.ReadToEnd();

            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                UnityEngine.Debug.LogError(string.Format("Command {0} error: {1} \n code: {2}", command, err, process.ExitCode));
                throw new Exception();
            }
            else
            {
                UnityEngine.Debug.Log(string.Format("Command {0} success", command));
            }
        }

        public void BuildGame(Platform platform)
        {
            UnityEngine.Debug.ClearDeveloperConsole();
            UnityEngine.Debug.Log("Starting...");

            string[] scenes = new string[]
            { 
                Application.dataPath + "/Scene1/Scene1.unity", 
                Application.dataPath + "/Scene2/Scene2.unity" 
            };

            ResetDefineSymbols(platform);

            if (debugBuild)
            {
                AddDefineSymbol(DEFINE_SYMBOLS[0], platform);
            }

            switch (platform)
            {
                case Platform.iOS:
                    AddDefineSymbol(DEFINE_SYMBOLS[2], platform);
                    break;
                case Platform.Android:
                    
                    break;
                case Platform.Kindle:
                    AddDefineSymbol(DEFINE_SYMBOLS[1], platform);
                    break;
            }

            BuildPlayer(scenes, platform);

            UnityEngine.Debug.Log("Finished!");
        }

        public void OnEnable()
        {
            if (bundleId == null && EditorPrefs.HasKey("FPAuthTool.bundleId"))
            {
                bundleId = EditorPrefs.GetString("FPAuthTool.bundleId");
            }

            if (androidHome == null && EditorPrefs.HasKey("FPAuthTool.androidHome"))
            {
                androidHome = EditorPrefs.GetString("FPAuthTool.androidHome");
            }

            if (amazonAPIKey == null && EditorPrefs.HasKey("FPAuthTool.amazonAPIKey"))
            {
                amazonAPIKey = EditorPrefs.GetString("FPAuthTool.amazonAPIKey");
            }

            if (EditorPrefs.HasKey("FPAuthTool.debugBuild"))
            {
                debugBuild = EditorPrefs.GetBool("FPAuthTool.debugBuild");
            }

            if (platform == Platform.None && EditorPrefs.HasKey("FPAuthTool.platform"))
            {
                platform = (Platform)Enum.Parse(typeof(Platform), EditorPrefs.GetString("FPAuthTool.platform"), true);
            }

            if (outputLocation == null && EditorPrefs.HasKey("FPAuthTool.outputLocation"))
            {
                outputLocation = EditorPrefs.GetString("FPAuthTool.outputLocation");
            }
        }

        public void OnDisable()
        {
            if (bundleId != null)
            {
                EditorPrefs.SetString("FPAuthTool.bundleId", bundleId);
            }

            if (androidHome != null)
            {
                EditorPrefs.SetString("FPAuthTool.androidHome", androidHome);
            }

            if (amazonAPIKey != null)
            {
                EditorPrefs.SetString("FPAuthTool.amazonAPIKey", amazonAPIKey);
            }

            EditorPrefs.SetBool("FPAuthTool.debugBuild", debugBuild);

            if (platform != Platform.None)
            {
                EditorPrefs.SetString("FPAuthTool.platform", platform.ToString());
            }

            if (outputLocation != null)
            {
                EditorPrefs.SetString("FPAuthTool.outputLocation", outputLocation);
            }
        }

        private void ResetDefineSymbols(Platform platform)
        {
            foreach (string symbol in DEFINE_SYMBOLS)
            {
                RemoveDefineSymbol(symbol, platform);
            }
        }

        private void AddDefineSymbol(string defineSymbol, Platform platform)
        {
            BuildTargetGroup btg = GetBuildTargetGroupFromPlatform(platform);
            string currentDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
            if (currentDefineSymbols.Contains(defineSymbol))
                return;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(GetBuildTargetGroupFromPlatform(platform), currentDefineSymbols + ";" + defineSymbol);
            UnityEngine.Debug.Log(string.Format("AddDefineSymbol({0}): Define symbols set to {1}", defineSymbol, PlayerSettings.GetScriptingDefineSymbolsForGroup(btg)));
        }

        private void RemoveDefineSymbol(string defineSymbol, Platform platform)
        {
            BuildTargetGroup btg = GetBuildTargetGroupFromPlatform(platform);
            string currentDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
            if (!currentDefineSymbols.Contains(defineSymbol))
            {
                return;
            }

            int pos = currentDefineSymbols.IndexOf(defineSymbol, StringComparison.Ordinal);
            currentDefineSymbols = currentDefineSymbols.Remove(pos, defineSymbol.Length);
            if (currentDefineSymbols.Length < pos && currentDefineSymbols[pos] == ';')
            {
                currentDefineSymbols = currentDefineSymbols.Remove(pos, 1);
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(GetBuildTargetGroupFromPlatform(platform), currentDefineSymbols);
            UnityEngine.Debug.Log(string.Format("RemoveDefineSymbol({0}): Define symbols set to {1}", defineSymbol, PlayerSettings.GetScriptingDefineSymbolsForGroup(btg)));
        }

        public static BuildTargetGroup GetBuildTargetGroupFromPlatform(Platform platform)
        {
            switch (platform)
            {
                case Platform.iOS:
                    return BuildTargetGroup.iOS;
                case Platform.Kindle:
                case Platform.Android:
                    return BuildTargetGroup.Android;
            }
            throw new Exception(string.Format("Unhandled platform {0}", platform.ToString()));
        }

        public static BuildTarget GetBuildTargetFromPlatform(Platform platform)
        {
            switch (platform)
            {
                case Platform.iOS:
                    return BuildTarget.iOS;
                case Platform.Kindle:
                case Platform.Android:
                    return BuildTarget.Android;
            }
            throw new Exception(string.Format("Unhandled platform {0}", platform.ToString()));
        }

        private void BuildPlayer(string[] scenes, Platform platform)
        {
            BuildOptions buildOptions = BuildOptions.None;

            if (debugBuild)
            {
                buildOptions |= BuildOptions.Development;
                buildOptions |= BuildOptions.AllowDebugging;
            }

            bool previousAPKExpansionSetting = false;
            bool resetCurrentAPKExpansionSetting = false;
            if (platform == Platform.Android)
            {
                previousAPKExpansionSetting = PlayerSettings.Android.useAPKExpansionFiles;
                resetCurrentAPKExpansionSetting = true;

                //TODO: add toggle
                PlayerSettings.Android.useAPKExpansionFiles = false;
            }

            string updatedOutputLocation = outputLocation + "/" + platform.ToString();

            if (!Directory.Exists(updatedOutputLocation))
            {
                Directory.CreateDirectory(updatedOutputLocation);
            }

            switch (platform)
            {
                case Platform.Android:
                case Platform.Kindle:
                    updatedOutputLocation = updatedOutputLocation + "/project.apk";
                    break;
                case Platform.iOS:
                    updatedOutputLocation = updatedOutputLocation + "/";
                    break;
            }

            BuildTargetGroup btg = GetBuildTargetGroupFromPlatform(platform);
            UnityEngine.Debug.Log("Building project in directory: " + outputLocation + " with scenes: " + string.Join(",", scenes) + " options: " + buildOptions);
            UnityEngine.Debug.Log(string.Format("Define symbols set to {0}", PlayerSettings.GetScriptingDefineSymbolsForGroup(btg)));

            string error = BuildPipeline.BuildPlayer(scenes, updatedOutputLocation, GetBuildTargetFromPlatform(platform), buildOptions);

            if (resetCurrentAPKExpansionSetting)
            {
                PlayerSettings.Android.useAPKExpansionFiles = previousAPKExpansionSetting;
            }

            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogError("Build Error: " + error);
                throw new Exception("Build Failed With: " + error);
            }

            if (UnityEditorInternal.InternalEditorUtility.isHumanControllingUs)
            {
                if (!string.IsNullOrEmpty(error))
                {
                    EditorUtility.DisplayDialog("Build Failed", "Build Error: " + error, "Close");
                }
                else
                {
                    EditorUtility.DisplayDialog("Build Complete", "Build has been completed successfully.", "Close");
                }
            }
        }
    }
}