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

        Dictionary<string, string> ServerCreds();

        void OnPause();

        void OnResume();

        void Log(AAuthManager.LogLevel level, string message);
    }
}
