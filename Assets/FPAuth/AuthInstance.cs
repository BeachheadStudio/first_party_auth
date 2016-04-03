using System;

namespace FPAuth
{
    public class AuthInstance
    {
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

        private static void Init()
        {
#if UNITY_IOS  
            instance = new iOSAuthManager();
#elif UNITY_ANDROID && !KINDLE_BUILD
            instance = new AndroidAuthManager();
#elif KINDLE_BUILD
            instance = new AmazonAuthManager();
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

