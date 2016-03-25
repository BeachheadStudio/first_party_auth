using System;

namespace FPAuth
{
    public class AuthManager
    {
        private static AAuthManager instance;

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
            // return
#elif UNITY_ANDROID && !KINDLE_BUILD
            instance = new AndroidAuthManager();
#elif KINDLE_BUILD
            // return 
#endif
        }

        public static void Log(String message, object[] args)
        {
            if (args.Length > 0)
            {
                Console.WriteLine("AuthManager: " + message);
            }
            else
            {
                Console.WriteLine("AuthManager: " + message);

                foreach (object obj in args)
                {
                    Console.WriteLine(string.Format("{0}", obj));
                }
                 
            }
        }
    }
}

