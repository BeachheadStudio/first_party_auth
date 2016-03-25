using System;
using System.Collections.Generic;

namespace FPAuth
{
    public interface IAuthManager
    {
        void Start();

        string PlayerName();

        string PlayerId();

        string FirstPartyPlayerId();

        Dictionary<string, string> ServerCreds();
    }
}
