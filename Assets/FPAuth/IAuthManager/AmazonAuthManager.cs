using UnityEngine;
using System.Collections.Generic;

namespace FPAuth
{
    public class AmazonAuthManager : AAuthManager
    {
        public AmazonAuthManager()
        {
            mStatus = Status.Init;
        }

        public override void Init()
        {
            Log(LogLevel.DEBUG, "starting...");
            using (AndroidJavaClass clazz = new AndroidJavaClass("com.singlemalt.amazon.auth.amazonauth.AuthService"))
            using (AndroidJavaObject authService = clazz.CallStatic<AndroidJavaObject>("getInstance"))
            {
                authService.Call("init", true, false, false);
                mStatus = Status.FirstPartyWorking;
            }
        }

        public override void OnPause()
        {
            Log(LogLevel.DEBUG, "OnPause");
            
            using (AndroidJavaClass clazz = new AndroidJavaClass("com.singlemalt.amazon.auth.amazonauth.AuthService"))
            using (AndroidJavaObject authService = clazz.CallStatic<AndroidJavaObject>("getInstance"))
            {
                authService.Call("onPause");
            }
        }

        public override void OnResume()
        {
            Log(LogLevel.DEBUG, "OnResume");

            using (AndroidJavaClass clazz = new AndroidJavaClass("com.singlemalt.amazon.auth.amazonauth.AuthService"))
            using (AndroidJavaObject authService = clazz.CallStatic<AndroidJavaObject>("getInstance"))
            {
                authService.Call("onResume");
            }
        }

        public override string FailureError()
        {
            Log(LogLevel.ERROR, "FailureError");
            using (AndroidJavaClass clazz = new AndroidJavaClass("com.singlemalt.amazon.auth.amazonauth.AuthService"))
            using (AndroidJavaObject authService = clazz.CallStatic<AndroidJavaObject>("getInstance"))
            {
                return authService.Call<string>("getFailureError");
            }
        }

        public override string SessionToken()
        {
            return "";
        }

        public override void Log(LogLevel level, string message)
        {
            using (AndroidJavaClass logger = new AndroidJavaClass("android.util.Log"))
            {
                switch (level)
                {
                    case LogLevel.DEBUG:
                        logger.CallStatic<int>("d", "AmazonAuthManager", message);
                        break;
                    case LogLevel.VERBOSE:
                        logger.CallStatic<int>("v", "AmazonAuthManager", message);
                        break;
                    case LogLevel.INFO:
                        logger.CallStatic<int>("i", "AmazonAuthManager", message);
                        break;
                    case LogLevel.WARN:
                        logger.CallStatic<int>("w", "AmazonAuthManager", message);
                        break;
                    case LogLevel.ERROR:
                        logger.CallStatic<int>("e", "AmazonAuthManager", message);
                        break;
                    case LogLevel.ASSERT:
                        logger.CallStatic<int>("a", "AmazonAuthManager", message);
                        break;
                }
            }
        }

        public override void FireFirstPartyAuthSuccess()
        {
            bool isAnonymous;
            string firstPartyPlayerId = "", playerName = "", oauthToken = "";

            using (AndroidJavaClass clazz = new AndroidJavaClass("com.singlemalt.amazon.auth.amazonauth.AuthService"))
            using (AndroidJavaObject authService = clazz.CallStatic<AndroidJavaObject>("getInstance"))
            {
                isAnonymous = authService.Call<bool>("isAnonymous");

                if (!isAnonymous)
                {
                    firstPartyPlayerId = authService.Call<string>("getPlayerId");
                    playerName = authService.Call<string>("getPlayerName");
                    oauthToken = authService.Call<string>("getOauthToken");
                }
            }

            Log(LogLevel.DEBUG, string.Format("isAnonymous {0} firstPartyPlayerId {1} playerName {2} oauthToken {3}",
                    isAnonymous, firstPartyPlayerId, playerName, oauthToken));

            base.FireFirstPartyAuthSuccess();
        }
    }
}
