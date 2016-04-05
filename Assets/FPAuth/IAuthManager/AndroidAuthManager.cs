using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;

#if UNITY_ANDROID && !KINDLE_BUILD
namespace FPAuth
{
    public class AndroidAuthManager : AAuthManager
    {
        public override void Init()
        {
            string playerId = PlayerPrefs.GetString("FPAuth.PlayerId", null);

            Log(LogLevel.DEBUG, "starting...");
            mStatus = Status.Working;

            using (AndroidJavaClass clazz = new AndroidJavaClass("com.singlemalt.googleplay.auth.googleplayauth.AuthService"))
            using (AndroidJavaObject authService = clazz.CallStatic<AndroidJavaObject>("getInstance"))
            {
                authService.Call("init", AuthInstance.Settings.clientId, AuthInstance.Settings.authServerUrl, playerId);
                mStatus = Status.Working;
            }
        }

        public override void OnPause()
        {
            Log(LogLevel.DEBUG, "OnPause");

            using (AndroidJavaClass clazz = new AndroidJavaClass("com.singlemalt.googleplay.auth.googleplayauth.AuthService"))
            using (AndroidJavaObject authService = clazz.CallStatic<AndroidJavaObject>("getInstance"))
            {
                authService.Call("onPause");
            }
        }

        public override void OnResume()
        {
            Log(LogLevel.DEBUG, "OnResume");

            using (AndroidJavaClass clazz = new AndroidJavaClass("com.singlemalt.googleplay.auth.googleplayauth.AuthService"))
            using (AndroidJavaObject authService = clazz.CallStatic<AndroidJavaObject>("getInstance"))
            {
                authService.Call("onResume");
            }
        }

        public override string FailureError()
        {
            Log(LogLevel.ERROR, "FailureError");
            using (AndroidJavaClass clazz = new AndroidJavaClass("com.singlemalt.googleplay.auth.googleplayauth.AuthService"))
            using (AndroidJavaObject authService = clazz.CallStatic<AndroidJavaObject>("getInstance"))
            {
                return authService.Call<string>("getFailureError");
            }
        }

        public override void Log(LogLevel level, string message)
        {
            using (AndroidJavaClass logger = new AndroidJavaClass("android.util.Log"))
            {
                switch (level)
                {
                    case LogLevel.DEBUG:
                        logger.CallStatic<int>("d", "GooglePlayAuthManager", message);
                        break;
                    case LogLevel.VERBOSE:
                        logger.CallStatic<int>("v", "GooglePlayAuthManager", message);
                        break;
                    case LogLevel.INFO:
                        logger.CallStatic<int>("i", "GooglePlayAuthManager", message);
                        break;
                    case LogLevel.WARN:
                        logger.CallStatic<int>("w", "GooglePlayAuthManager", message);
                        break;
                    case LogLevel.ERROR:
                        logger.CallStatic<int>("e", "GooglePlayAuthManager", message);
                        break;
                    case LogLevel.ASSERT:
                        logger.CallStatic<int>("a", "GooglePlayAuthManager", message);
                        break;
                }
            }
        }

        public override string SessionToken()
        {
            using (AndroidJavaClass clazz = new AndroidJavaClass("com.singlemalt.googleplay.auth.googleplayauth.AuthService"))
            using (AndroidJavaObject authService = clazz.CallStatic<AndroidJavaObject>("getInstance"))
            {
                return authService.Call<string>("getSessionToken");
            }
        }

        public override string PlayerName()
        {
            using (AndroidJavaClass clazz = new AndroidJavaClass("com.singlemalt.googleplay.auth.googleplayauth.AuthService"))
            using (AndroidJavaObject authService = clazz.CallStatic<AndroidJavaObject>("getInstance"))
            {
                return authService.Call<string>("getPlayerName");
            }
        }

        public override string PlayerId()
        {
            using (AndroidJavaClass clazz = new AndroidJavaClass("com.singlemalt.googleplay.auth.googleplayauth.AuthService"))
            using (AndroidJavaObject authService = clazz.CallStatic<AndroidJavaObject>("getInstance"))
            {
                return authService.Call<string>("getServerPlayerId");
            }
        }

        public override string FirstPartyPlayerId()
        {
            using (AndroidJavaClass clazz = new AndroidJavaClass("com.singlemalt.googleplay.auth.googleplayauth.AuthService"))
            using (AndroidJavaObject authService = clazz.CallStatic<AndroidJavaObject>("getInstance"))
            {
                return authService.Call<string>("getPlayerId");
            }
        }

        public override bool IsAnonymous()
        {
            using (AndroidJavaClass clazz = new AndroidJavaClass("com.singlemalt.googleplay.auth.googleplayauth.AuthService"))
            using (AndroidJavaObject authService = clazz.CallStatic<AndroidJavaObject>("getInstance"))
            {
                return authService.Call<bool>("IsAnonymous");
            }
        }

        public override void FireAuthSuccess()
        {
            bool isAnonymous = IsAnonymous();
            string firstPartyPlayerId = FirstPartyPlayerId(), playerName = PlayerName(), sessionToken = SessionToken(), 
            playerId = PlayerId();

            Log(LogLevel.DEBUG, string.Format("isAnonymous {0} firstPartyPlayerId {1} playerName {2} sessionToken {3}",
                isAnonymous, firstPartyPlayerId, playerName, sessionToken));

            PlayerPrefs.SetString("FPAuth.PlayerId", playerId);

            base.FireAuthSuccess();
        }
    }
}
#endif
