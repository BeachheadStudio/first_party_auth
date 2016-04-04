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

        private static AAuthManager instance = null;
        private static AuthSettings settings = null;

        public static AuthSettings Settings
        {
            get { return settings; }
            set { }
        }

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

        private static void Init()
        {
#if UNITY_IOS  
            instance = new iOSAuthManager();
#elif UNITY_ANDROID && !KINDLE_BUILD
            instance = new AndroidAuthManager();
#elif KINDLE_BUILD
            instance = new AmazonAuthManager();
#endif

#if UNITY_ANDROID && !KINDLE_BUILD
            // grab settings from disk
            string settingsFilename = "authSettings";
            TextAsset jsonAsset = Resources.Load<TextAsset>(settingsFilename);
            settings = JsonUtility.FromJson<AuthSettings>(jsonAsset.text);
#endif
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

