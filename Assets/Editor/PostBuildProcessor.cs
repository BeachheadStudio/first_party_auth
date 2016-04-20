using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using System;

public class PostBuildProcessor {

    [PostProcessBuild(int.MaxValue)]
    public static void AddGameKitFramework(BuildTarget target, string path)
    {
        //Only do this on iOS
        if (target != BuildTarget.iOS)
        {
            return;
        }

        Debug.Log ("Adding Gamekit to XCode project.");
        var pbxProjFilePath = string.Join(Path.DirectorySeparatorChar.ToString(), new string[3] { path, "Unity-iPhone.xcodeproj", "project.pbxproj" });
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(pbxProjFilePath));

        // This is the project name that Unity generates for iOS
        string iosTarget = proj.TargetGuidByName(PBXProject.GetUnityTargetName());

        proj.AddFrameworkToProject(iosTarget, "GameKit.framework", true);

        File.WriteAllText(pbxProjFilePath, proj.WriteToString());

        /*
            PlistDocument
            <key>UIRequiredDeviceCapabilities</key>
            <array>
                <string>armv7</string>
                <string>gamekit</string>
            </array>
        */
    }
}
