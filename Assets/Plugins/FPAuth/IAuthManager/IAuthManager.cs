using System;
using System.Collections.Generic;

namespace FPAuth
{
    public interface IAuthManager
    {
        void Init();

        string PlayerName();

        string PlayerId();

        string FirstPartyPlayerId();

        string FailureError();

        string SessionToken();

        void OnPause();

        void OnResume();

        bool IsAuthenticated();

        bool IsAnonymous();

        void Log(AAuthManager.LogLevel level, string message);
    }
}
