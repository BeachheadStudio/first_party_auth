using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
using System.Threading;

#if UNITY_ANDROID && !KINDLE_BUILD && !UNITY_EDITOR
namespace FPAuth
{
    public class AndroidAuthManager : AAuthManager
    {
        public override void Init()
        {
            attachMainThread();
            string playerId = PlayerPrefs.GetString(AAuthManager.PLAYER_ID_KEY, null);

            mStatus = Status.Working;
            
            using (AndroidJavaObject authService = AuthInstance.Instance.MainClass.CallStatic<AndroidJavaObject>("getInstance"))
            {
                authService.Call("init", AuthInstance.Settings.clientId, AuthInstance.GetPlayerPrefsEnvironment() + "/auth", playerId);
            }
            detachMainThread();
        }

        public override void OnPause()
        {
            attachMainThread();

            using (AndroidJavaObject authService = AuthInstance.Instance.MainClass.CallStatic<AndroidJavaObject>("getInstance"))
            {
                authService.Call("onPause");
            }

            detachMainThread();
        }

        public override void OnResume()
        {
            attachMainThread();

            using (AndroidJavaObject authService = AuthInstance.Instance.MainClass.CallStatic<AndroidJavaObject>("getInstance"))
            {
                authService.Call("onResume");
            }

            detachMainThread();
        }

        public override string FailureError()
        {
            string failureError;
            attachMainThread();

            using (AndroidJavaObject authService = AuthInstance.Instance.MainClass.CallStatic<AndroidJavaObject>("getInstance"))
            {
                failureError = authService.Call<string>("getFailureError");
            }

            detachMainThread();
            return failureError;
        }

        public override void Log(LogLevel level, string message)
        {
            Assert.That(IsMainThread(), "Log must be called from main thread");
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
            string sessionToken;
            attachMainThread();

            using (AndroidJavaObject authService = AuthInstance.Instance.MainClass.CallStatic<AndroidJavaObject>("getInstance"))
            {
                sessionToken = authService.Call<string>("getSessionToken");
            }

            detachMainThread();
            return sessionToken;
        }

        public override string PlayerName()
        {
            string playerName;
            attachMainThread();

            using (AndroidJavaObject authService = AuthInstance.Instance.MainClass.CallStatic<AndroidJavaObject>("getInstance"))
            {
                playerName = authService.Call<string>("getPlayerName");
            }

            detachMainThread();
            return playerName;
        }

        public override string PlayerId()
        {
            string playerId;
            attachMainThread();

            using (AndroidJavaObject authService = AuthInstance.Instance.MainClass.CallStatic<AndroidJavaObject>("getInstance"))
            {
                playerId = authService.Call<string>("getServerPlayerId");
            }

            detachMainThread();
            return playerId;
        }

        public override string FirstPartyPlayerId()
        {
            string firstPartyPlayerId;
            attachMainThread();

            using (AndroidJavaObject authService = AuthInstance.Instance.MainClass.CallStatic<AndroidJavaObject>("getInstance"))
            {
                firstPartyPlayerId = authService.Call<string>("getPlayerId");
            }

            detachMainThread();
            return firstPartyPlayerId;
        }

        public override bool IsAnonymous()
        {
            bool anonymous;
            attachMainThread();

            using (AndroidJavaObject authService = AuthInstance.Instance.MainClass.CallStatic<AndroidJavaObject>("getInstance"))
            {
                anonymous = authService.Call<bool>("isAnonymous");
            }

            detachMainThread();
            return anonymous;
        }

        public override Dictionary<string, string> GetAuthParams()
        {
            string authParams;
            attachMainThread();

            using (AndroidJavaObject authService = AuthInstance.Instance.MainClass.CallStatic<AndroidJavaObject>("getInstance"))
            {
                authParams = authService.Call<string>("getAuthParams");
            }

            detachMainThread();
            return JsonUtility.FromJson<Dictionary<string, string>>(authParams);
        }

        public override void AwardAchievement(string achievementId)
        {
            attachMainThread();

            using (AndroidJavaObject authService = AuthInstance.Instance.MainClass.CallStatic<AndroidJavaObject>("getInstance"))
            {
                authService.Call("awardAchievement", achievementId);
            }

            detachMainThread();
        }

        public override void FireAuthSuccess()
        {

            Debug.Log(string.Format("isAnonymous {0} firstPartyPlayerId {1} playerName {2} sessionToken {3}",
                    IsAnonymous(), FirstPartyPlayerId(), PlayerName(), SessionToken()));

            PlayerPrefs.SetString(AAuthManager.PLAYER_ID_KEY, PlayerId());

            base.FireAuthSuccess();
        }

        private void attachMainThread()
        {
            if (mainThread != Thread.CurrentThread)
            {
                Debug.Log("calling AttachCurrentThread()");
                AndroidJNI.AttachCurrentThread();
            }
        }

        private void detachMainThread()
        {
            if (mainThread != Thread.CurrentThread)
            {
                Debug.Log("calling DetachCurrentThread()");
                AndroidJNI.DetachCurrentThread();
            }
        }
    }
}
#endif