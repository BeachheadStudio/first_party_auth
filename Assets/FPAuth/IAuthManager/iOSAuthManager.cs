using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace FPAuth
{
    public class iOSAuthManager : AAuthManager
    {
        // Native calls
        [DllImport("__Internal")]
        private static extern void NativeLog(string message);

        [DllImport("__Internal")]
        private static extern void AuthLocalPlayer(string serverUrl, string playerId);

        [DllImport("__Internal")]
        private static extern string GetPlayerName();

        [DllImport("__Internal")]
        private static extern string GetPlayerId();

        [DllImport("__Internal")]
        private static extern string GetFirstPartyPlayerId();

        [DllImport("__Internal")]
        private static extern string GetFailureError();

        [DllImport("__Internal")]
        private static extern string GetSessionToken();

        [DllImport("__Internal")]
        private static extern void NativeOnPause();

        [DllImport("__Internal")]
        private static extern void NativeOnResume();

        [DllImport("__Internal")]
        private static extern bool NativeIsAnonymous();

        public override void Init()
        {
            mStatus = Status.Working;
            string playerId = PlayerPrefs.GetString("FPAuth.PlayerId", null);

            Log(LogLevel.DEBUG, "Starting auth in thread");
            AuthLocalPlayer(AuthInstance.Settings.authServerUrl, playerId);
        }

        public override string PlayerName()
        {
            return GetPlayerName();
        }

        public override string PlayerId()
        {
            return GetPlayerId();
        }

        public override string FirstPartyPlayerId()
        {
            return GetFirstPartyPlayerId();
        }

        public override string FailureError()
        {
            return GetFailureError();
        }

        public override string SessionToken()
        {
            return GetSessionToken();
        }

        public override void OnPause()
        {
            NativeOnPause();
        }

        public override void OnResume()
        {
            NativeOnResume();
        }

        public override bool IsAnonymous()
        {
            return NativeIsAnonymous();
        }

        public override void Log(AAuthManager.LogLevel level, string message)
        {
            NativeLog(string.Format("{0}: {1}", level, message));
        }

        public override void FireAuthSuccess()
        {
            bool isAnonymous = IsAnonymous();
            string firstPartyPlayerId = FirstPartyPlayerId(), playerId = PlayerId(), playerName = PlayerName(), oauthToken = SessionToken();

            Log(LogLevel.DEBUG, string.Format("isAnonymous {0} firstPartyPlayerId {1} playerId {2} playerName {3} sessionToken {4}",
                    isAnonymous, firstPartyPlayerId, playerId, playerName, oauthToken));

            PlayerPrefs.SetString("FPAuth.PlayerId", playerId);

            base.FireAuthSuccess();
        }
    }
}
