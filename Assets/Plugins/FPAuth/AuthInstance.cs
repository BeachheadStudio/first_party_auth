using System;
using System.IO;
using UnityEngine;

namespace FPAuth
{
    public class AuthInstance
    {
        [Serializable]
        public class AuthSettings
        {
            public string clientId;
            public string authServerUrl;
        }

        private static AuthSettings settings = null;
        public static AuthSettings Settings
        {
            get { return settings; }
            set { }
        }

        private static AAuthManager instance = null;
        public static AAuthManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Init();
                }

                return instance;
            }
            private set { }
        }

        public static int FailCount = 0;

        private static void Init()
        {
#if UNITY_IOS && !UNITY_EDITOR
            instance = new iOSAuthManager();
#elif UNITY_ANDROID && !KINDLE_BUILD && !UNITY_EDITOR
            instance = new AndroidAuthManager();
#elif KINDLE_BUILD && !UNITY_EDITOR
            instance = new AmazonAuthManager();
#elif UNITY_EDITOR
            instance = new DevAuthManager();
#else
            Debug.LogError("Unsupported platform in AuthInstance::Init!");
#endif
            // grab settings from disk
            string settingsFilename = "authSettings";
            TextAsset jsonAsset = Resources.Load<TextAsset>(settingsFilename);
            settings = JsonUtility.FromJson<AuthSettings>(jsonAsset.text);
        }

        public static void Log(AAuthManager.LogLevel level, String message, object[] args)
        {
            if (args.Length > 0)
            {
                instance.Log(level, message);
            }
            else
            {
                instance.Log(level, message);

                foreach (object obj in args)
                {
                    instance.Log(level, string.Format("{0}", obj));
                }
            }
        }
    }
}

