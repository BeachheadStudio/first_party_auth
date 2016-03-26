using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;

namespace FPAuth
{
    public class FPAuthTool : EditorWindow
    {
        public static readonly string AMAZON_BUILD_DIR = "AmazonAuth/Assets/Plugins/Android/Amazon";
        public static readonly string AMAZON_PLUGIN_DIR = "Assets/Plugins/Android/Amazon";

        string bundleId = "com.example.bundleid";

        [MenuItem("Window/FPAuth")]
        public static void  ShowWindow()
        {
            EditorWindow.GetWindow(typeof(FPAuthTool));
        }

        void OnGUI()
        {
            GUILayout.Label("FPAuthTool Settings", EditorStyles.boldLabel);
            bundleId = EditorGUILayout.TextField("Bundle", bundleId);

            EditorGUILayout.Space();

            if (GUILayout.Button("Setup"))
            {
                
            }
        }

        private void BuildProject()
        {
            if (Directory.Exists(AMAZON_PLUGIN_DIR))
            {
                Directory.Delete(AMAZON_PLUGIN_DIR);
            }
            Directory.CreateDirectory(AMAZON_PLUGIN_DIR);

            DirectoryCopy(AMAZON_BUILD_DIR, AMAZON_PLUGIN_DIR);
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
    }
}
