using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace FPAuth
{
    public class DevAuthManager : AAuthManager
    {
        public override void Init()
        {
            string playerId = PlayerPrefs.GetString(AAuthManager.PLAYER_ID_KEY, null);
            if(string.IsNullOrEmpty(playerId))
            {
                playerId = System.Guid.NewGuid().ToString();
            }

            mStatus = Status.Working;
        }

        public override void OnPause()
        {
            // do nothing
        }

        public override void OnResume()
        {
            // do nothing
        }

        public override string FailureError()
        {
            return null;
        }

        public override void Log(AAuthManager.LogLevel level, string message)
        {
            Debug.Log(message);
        }

        public override string PlayerName()
        {
            throw new Exception("Not Implemented");
        }

        public override string PlayerId()
        {
            return PlayerPrefs.GetString(AAuthManager.PLAYER_ID_KEY);
        }

        public override string FirstPartyPlayerId()
        {
            throw new Exception("Not Implemented");
        }

        public override string SessionToken()
        {
            // TODO: implement
            return null;
        }

        public override bool IsAnonymous()
        {
            return false;
        }

        public override Dictionary<string, string> GetAuthParams()
        {
            return new Dictionary<string, string>();
        }

        public override void AwardAchievement(string achievementId)
        {
            // do nothing
        }
    }
}
#endif