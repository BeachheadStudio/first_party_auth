using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;
using System.Diagnostics;
using System.Xml;
using UnityEditor.AnimatedValues;

namespace FPAuth
{
    public class FPAuthTool : EditorWindow
    {
        public static readonly string AMAZON_BUILD_DIR = "AmazonAuth/Assets/Plugins/Android/Amazon";
        public static readonly string AMAZON_PLUGIN_DIR = "Assets/Plugins/Android/Amazon";
        public static readonly string ANDROID_BASE_DIR = "Assets/Plugins/Android";
        public static readonly string ANDROID_BUILD_BASE_DIR = "AndroidAuth/Assets/Plugins/Android";
        public static readonly string IOS_BUILD_DIR = "iOSAuth/iOSAuth";
        public static readonly string IOS_PLUGIN_DIR = "Assets/Plugins/iOS/iOSAuth";

        public static readonly string[] DEFINE_SYMBOLS = new string[]
        {
            "DEVELOPER_TOOLS", "KINDLE_BUILD" 
        };
        public static readonly string PLAY_VERSION = "8.4.0";
        public static readonly string[] PLAY_AARS = new string[]
        {
            "play-services-auth-{0}.aar",
            "play-services-base-{0}.aar",
            "play-services-basement-{0}.aar",
            "play-services-games-{0}.aar",
            "play-services-plus-{0}.aar"
        };

        private Process process = null;

        string bundleId;
        string androidHome;
        string amazonAPIKey;
        bool debugBuild;
        Platform platform = Platform.None;
        string outputLocation;
        bool buildPlugins;
        string androidAppId;
        string androidClientId;
        string authServerUrl;
        string androidKeyLocation;
        string androidKeyPassword;
        string amazonKeyLocation;
        string amazonKeyPassword;
        string androidAliasName;
        string androidAliasPassword;
        string amazonAliasName;
        string amazonAliasPassword;
        AnimBool useAndroidKeyForAmazon = new AnimBool(false);

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
            EditorWindow window = EditorWindow.GetWindow(typeof(FPAuthTool), false, "FPAuthTool", true);
            window.maxSize = new Vector2(500f, 705f);
            window.minSize = window.maxSize;
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

            // Main
            GUILayout.Label("FPAuthTool Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            bundleId = EditorGUILayout.TextField("Bundle Identifier", bundleId);
            buildPlugins = EditorGUILayout.Toggle("Build plugins from src", buildPlugins);
            debugBuild = EditorGUILayout.Toggle("Debug Build", debugBuild);
            EditorGUILayout.BeginHorizontal();
            outputLocation = EditorGUILayout.TextField("Output Location", outputLocation);
            GUI.SetNextControlName("Browse");
            if (GUILayout.Button("Browse"))
            {
                outputLocation = EditorUtility.SaveFolderPanel("Select Location", outputLocation, "Builds");
                GUI.FocusControl("Browse");
            }
            EditorGUILayout.EndHorizontal();

            // Android
            EditorGUILayout.Space();
            GUILayout.Label("Android Settings", EditorStyles.boldLabel);
            androidHome = EditorGUILayout.TextField("Android SDK Location", androidHome);
            androidAppId = EditorGUILayout.TextField("Android App ID", androidAppId);
            androidClientId = EditorGUILayout.TextField("Android Client ID", androidClientId);
            EditorGUILayout.BeginHorizontal();
            androidKeyLocation = EditorGUILayout.TextField("Android Key File", androidKeyLocation);
            GUI.SetNextControlName("Browse");
            if (GUILayout.Button("Browse"))
            {
                androidKeyLocation = EditorUtility.OpenFilePanel("Select Location", androidKeyLocation, "keystore");
                GUI.FocusControl("Browse");
            }
            EditorGUILayout.EndHorizontal();
            androidKeyPassword = EditorGUILayout.PasswordField("Android Key Password", androidKeyPassword);
            androidAliasName = EditorGUILayout.TextField("Android Alias", androidAliasName);
            androidAliasPassword = EditorGUILayout.PasswordField("Android Alias Password", androidAliasPassword);

            // Amazon
            EditorGUILayout.Space();
            GUILayout.Label("Amazon Settings", EditorStyles.boldLabel);
            amazonAPIKey = EditorGUILayout.TextField("Amazon API Key", amazonAPIKey, GUILayout.Height(200));
            useAndroidKeyForAmazon.target = EditorGUILayout.Toggle("Use Amazon Keystore", useAndroidKeyForAmazon.target);
            if (EditorGUILayout.BeginFadeGroup(useAndroidKeyForAmazon.faded))
            {
                this.maxSize = new Vector2(500f, 773f);
                this.minSize = this.maxSize;
                EditorGUILayout.BeginHorizontal();
                amazonKeyLocation = EditorGUILayout.TextField("Amazon Key File", amazonKeyLocation);
                GUI.SetNextControlName("Browse");
                if (GUILayout.Button("Browse"))
                {
                    amazonKeyLocation = EditorUtility.OpenFilePanel("Select Location", amazonKeyLocation, "keystore");
                    GUI.FocusControl("Browse");
                }
                EditorGUILayout.EndHorizontal();

                amazonKeyPassword = EditorGUILayout.PasswordField("Amazon Key Password", amazonKeyPassword);
                amazonAliasName = EditorGUILayout.TextField("Amazon Alias", amazonAliasName);
                amazonAliasPassword = EditorGUILayout.PasswordField("Amazon Alias Password", amazonAliasPassword);
            } else {
                this.maxSize = new Vector2(500f, 705f);
                this.minSize = this.maxSize;
            }
            EditorGUILayout.EndFadeGroup();

            // Auth server
            EditorGUILayout.Space();
            GUILayout.Label("Server Settings", EditorStyles.boldLabel);
            authServerUrl = EditorGUILayout.TextField("Auth Server Url", authServerUrl);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Launch Server", GUILayout.Width(200)))
            {
                // TODO: launch 
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Build
            EditorGUILayout.Space();
            GUILayout.Label("Build", EditorStyles.boldLabel);
            GUIContent label = new GUIContent();
            label.text = "Platform";
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            platform = (Platform)EditorGUILayout.EnumPopup(platform, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            if (GUILayout.Button("Setup Plugins", GUILayout.Width(200)))
            {
                //TODO: update
                if (string.IsNullOrEmpty(bundleId) || string.IsNullOrEmpty(androidHome) ||
                    string.IsNullOrEmpty(amazonAPIKey) || string.IsNullOrEmpty(androidClientId) ||
                    string.IsNullOrEmpty(authServerUrl))
                {
                    UnityEngine.Debug.Log("All input values have to be set");
                }
                else
                {
                    BuildProject();
                }
            }
            if (GUILayout.Button("Build", GUILayout.Width(200)))
            {
                if(platform != Platform.None && !string.IsNullOrEmpty(outputLocation))
                {
                    BuildGame(platform);
                }
            }
        }

        private void BuildProject()
        {
            UnityEngine.Debug.ClearDeveloperConsole();
            UnityEngine.Debug.Log("Starting...");

            BuildAmazonPlugin();
            BuildAndroidPlugin();
            BuildiOSPlugin();
            SaveSettings();

            UnityEngine.Debug.Log("Finished!");
        }

        private void BuildAmazonPlugin()
        {
            if (buildPlugins)
            {
                RunCommand(Application.dataPath + "/../AmazonAuth/gradlew", "AmazonAuth/", "makeUnityPlugin");
            }

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

        private void BuildAndroidPlugin()
        {
            if (buildPlugins)
            {
                RunCommand(Application.dataPath + "/../AndroidAuth/gradlew", "AndroidAuth/", "makeUnityPlugin");
            }

            if (!Directory.Exists(ANDROID_BASE_DIR))
            {
                Directory.CreateDirectory(ANDROID_BASE_DIR);
            }

            // clean out old files
            if (Directory.Exists(ANDROID_BASE_DIR + "/GooglePlay"))
            {
                Directory.Delete(ANDROID_BASE_DIR + "/GooglePlay", true);
            }
            Directory.CreateDirectory(ANDROID_BASE_DIR + "/GooglePlay");

            foreach (string filename in PLAY_AARS)
            {
                FileUtil.DeleteFileOrDirectory(ANDROID_BASE_DIR + "/" + string.Format(filename, PLAY_VERSION));
            }

            FileUtil.DeleteFileOrDirectory(ANDROID_BASE_DIR + "/appcompat-v7-23.2.1.aar");

            DirectoryCopy(ANDROID_BUILD_BASE_DIR, ANDROID_BASE_DIR);

            // Add app id to the manifest
            // <meta-data android:name="com.google.android.gms.games.APP_ID" android:value="\ YOUR_APP_ID"/>
            XmlDocument doc = new XmlDocument();
            doc.Load(ANDROID_BASE_DIR + "/GooglePlay/AndroidManifest.xml");

            XmlNodeList nodes = doc.SelectNodes("manifest/application/meta-data");

            foreach (XmlNode node in nodes)
            {
                XmlAttribute nodeName = node.Attributes["android:name"];
                if (nodeName.Value == "com.google.android.gms.games.APP_ID")
                {
                    XmlAttribute nodeValue = node.Attributes["android:value"];
                    nodeValue.Value = string.Format("\\ {0}", androidAppId);
                    doc.Save(ANDROID_BASE_DIR + "/GooglePlay/AndroidManifest.xml");
                    break;
                }
            }
        }

        private void BuildiOSPlugin()
        {
            if (Directory.Exists(IOS_PLUGIN_DIR))
            {
                Directory.Delete(IOS_PLUGIN_DIR, true);
            }
            Directory.CreateDirectory(IOS_PLUGIN_DIR);

            DirectoryCopy(IOS_BUILD_DIR + "/Auth", IOS_PLUGIN_DIR);
            DirectoryCopy(IOS_BUILD_DIR + "/Unity", IOS_PLUGIN_DIR);
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

            PlayerSettings.bundleIdentifier = bundleId;
            PlayerSettings.Android.keystoreName = androidKeyLocation;
            PlayerSettings.Android.keystorePass = androidKeyPassword;
            PlayerSettings.Android.keyaliasName = androidAliasName;
            PlayerSettings.Android.keyaliasPass = androidAliasPassword;

            string[] scenes = new string[]
            { 
                Application.dataPath + "/Scene1/Scene1.unity"
            };
            
            ResetDefineSymbols(platform);

            if (debugBuild)
            {
                AddDefineSymbol(DEFINE_SYMBOLS[0], platform);
            }

            switch (platform)
            {
                case Platform.iOS:
                    
                    break;
                case Platform.Android:
                    
                    break;
                case Platform.Kindle:
                    AddDefineSymbol(DEFINE_SYMBOLS[1], platform);

                    if(useAndroidKeyForAmazon.target)
                    {
                        PlayerSettings.Android.keystoreName = amazonKeyLocation;
                        PlayerSettings.Android.keystorePass = amazonKeyPassword;
                        PlayerSettings.Android.keyaliasName = amazonAliasName;
                        PlayerSettings.Android.keyaliasPass = amazonAliasPassword;
                    }

                    break;
            }

            if(platform != Platform.iOS)
            {
                
                UnityEngine.Debug.Log(string.Format("Using key {0} and alias {1}", PlayerSettings.Android.keystoreName, PlayerSettings.Android.keyaliasName));
            }

            BuildPlayer(scenes, platform);

            UnityEngine.Debug.Log("Clearing PlayerSettings...");
            PlayerSettings.bundleIdentifier = "";
            PlayerSettings.Android.keyaliasName = "";
            PlayerSettings.Android.keystoreName = "";

            EditorApplication.SaveAssets();

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

            if (EditorPrefs.HasKey("FPAuthTool.buildPlugins"))
            {
                buildPlugins = EditorPrefs.GetBool("FPAuthTool.buildPlugins");
            }

            if (androidAppId == null && EditorPrefs.HasKey("FPAuthTool.androidAppId"))
            {
                androidAppId = EditorPrefs.GetString("FPAuthTool.androidAppId");
            }

            if (androidClientId == null && EditorPrefs.HasKey("FPAuthTool.androidClientId"))
            {
                androidClientId = EditorPrefs.GetString("FPAuthTool.androidClientId");
            }

            if (authServerUrl == null && EditorPrefs.HasKey("FPAuthTool.authServerUrl"))
            {
                authServerUrl = EditorPrefs.GetString("FPAuthTool.authServerUrl");
            }

            if (androidKeyLocation == null && EditorPrefs.HasKey("FPAuthTool.androidKeyLocation"))
            {
                androidKeyLocation = EditorPrefs.GetString("FPAuthTool.androidKeyLocation");
            }

            if (androidKeyPassword == null && EditorPrefs.HasKey("FPAuthTool.androidKeyPassword"))
            {
                androidKeyPassword = EditorPrefs.GetString("FPAuthTool.androidKeyPassword");
            }

            if (amazonKeyLocation == null && EditorPrefs.HasKey("FPAuthTool.amazonKeyLocation"))
            {
                amazonKeyLocation = EditorPrefs.GetString("FPAuthTool.amazonKeyLocation");
            }

            if (amazonKeyPassword == null && EditorPrefs.HasKey("FPAuthTool.amazonKeyPassword"))
            {
                amazonKeyPassword = EditorPrefs.GetString("FPAuthTool.amazonKeyPassword");
            }

            if (androidAliasName == null && EditorPrefs.HasKey("FPAuthTool.androidAliasName"))
            {
                androidAliasName = EditorPrefs.GetString("FPAuthTool.androidAliasName");
            }

            if (androidAliasPassword == null && EditorPrefs.HasKey("FPAuthTool.androidAliasPassword"))
            {
                androidAliasPassword = EditorPrefs.GetString("FPAuthTool.androidAliasPassword");
            }

            if (amazonAliasName == null && EditorPrefs.HasKey("FPAuthTool.amazonAliasName"))
            {
                amazonAliasName = EditorPrefs.GetString("FPAuthTool.amazonAliasName");
            }

            if (amazonAliasPassword == null && EditorPrefs.HasKey("FPAuthTool.amazonAliasPassword"))
            {
                amazonAliasPassword = EditorPrefs.GetString("FPAuthTool.amazonAliasPassword");
            }

            if (EditorPrefs.HasKey("FPAuthTool.useAndroidKeyForAmazon"))
            {
                useAndroidKeyForAmazon = new AnimBool(EditorPrefs.GetBool("FPAuthTool.useAndroidKeyForAmazon"));
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

            EditorPrefs.SetBool("FPAuthTool.buildPlugins", buildPlugins);

            if (androidAppId != null)
            {
                EditorPrefs.SetString("FPAuthTool.androidAppId", androidAppId);
            }

            if (androidClientId != null)
            {
                EditorPrefs.SetString("FPAuthTool.androidClientId", androidClientId);
            }

            if (authServerUrl != null)
            {
                EditorPrefs.SetString("FPAuthTool.authServerUrl", authServerUrl);
            }

            if (androidKeyLocation != null)
            {
                EditorPrefs.SetString("FPAuthTool.androidKeyLocation", androidKeyLocation);
            }

            if (androidKeyPassword != null)
            {
                EditorPrefs.SetString("FPAuthTool.androidKeyPassword", androidKeyPassword);
            }

            if (amazonKeyLocation != null)
            {
                EditorPrefs.SetString("FPAuthTool.amazonKeyLocation", amazonKeyLocation);
            }

            if (androidAliasName != null)
            {
                EditorPrefs.SetString("FPAuthTool.androidAliasName", androidAliasName);
            }

            if (androidAliasPassword != null)
            {
                EditorPrefs.SetString("FPAuthTool.androidAliasPassword", androidAliasPassword);
            }

            if (amazonKeyPassword != null)
            {
                EditorPrefs.SetString("FPAuthTool.amazonKeyPassword", amazonKeyPassword);
            }

            if (amazonAliasName != null)
            {
                EditorPrefs.SetString("FPAuthTool.amazonAliasName", amazonAliasName);
            }

            if (amazonAliasPassword != null)
            {
                EditorPrefs.SetString("FPAuthTool.amazonAliasPassword", amazonAliasPassword);
            }

            EditorPrefs.SetBool("FPAuthTool.useAndroidKeyForAmazon", useAndroidKeyForAmazon.target);
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

            //TODO: add toggle
            PlayerSettings.Android.useAPKExpansionFiles = false;

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

        public void SaveSettings()
        {
            AuthInstance.AuthSettings settings = new AuthInstance.AuthSettings();
            settings.clientId = androidClientId;
            settings.authServerUrl = authServerUrl;

            // grab settings from disk
            string json = JsonUtility.ToJson(settings);
            string settingsFilename = "Assets/Resources/authSettings.json";
            StreamWriter sw = new StreamWriter(settingsFilename);
            sw.Write(json);
            sw.Close();

            AssetDatabase.Refresh();
        }
    }
}