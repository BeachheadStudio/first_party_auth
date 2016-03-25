using UnityEngine;
using System;
using System.Collections.Generic;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

namespace FPAuth
{
    public class AndroidAuthManager : AAuthManager
    {
        public static readonly string AUTH_FAIL_ERROR = "GooglePlay auth was cancelled";

        public AndroidAuthManager()
        {
            mStatus = Status.Init;
        }

        public override void Start()
        {
            mStatus = Status.FirstPartyWorking;

            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                .RequireGooglePlus()
                .Build();

            PlayGamesPlatform.InitializeInstance(config);
#if DEVELOPER_TOOLS
            PlayGamesPlatform.DebugLogEnabled = true;
#endif
            PlayGamesPlatform.Activate();

            // now log in
            Social.localUser.Authenticate(HandleFPResponse);
        }

        public void HandleFPResponse(bool result)
        {
            if (result)
            {
                mStatus = Status.FirstPartySuccess;
                isAuthed = true;
                isAnonymous = false;

                playerName = Social.localUser.userName;
                firstPartyPlayerId = Social.localUser.id;

                FireFirstPartySuccess();
            }
            else
            {
                mStatus = Status.FirstPartyFailure;
                isAuthed = false;
                isAnonymous = true;

                // TODO: see if we can get more information to return
                FireFirstPartyFailure(AUTH_FAIL_ERROR);
            }

            ClearFirstPartyEvents();
        }
    }
}

