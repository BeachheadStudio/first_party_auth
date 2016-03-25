using System;
using System.Collections.Generic;

namespace FPAuth
{
    public abstract class AAuthManager : IAuthManager
    {
        // event handlers
        public static event Action FirstPartyAuthSuccess;
        public static event Action<string> FirstPartyAuthFailure;
        public static event Action FirstPartyAuthCancel;
        public static event Action ServerAuthSuccess;
        public static event Action<string> ServerAuthFailure;

        // parameters
        protected bool isAnonymous = true;

        public bool IsAnonymous
        {
            get { return isAnonymous; }
            set { }
        }

        protected bool isAuthed = false;

        public bool IsAuthed
        {
            get { return isAuthed; }
            set { }
        }

        public enum Status
        {
            Init,
            FirstPartyWorking,
            FirstPartySuccess,
            FirstPartyFailure,
            ServerWorking,
            ServerSuccess,
            ServerFailure
        }

        protected Status mStatus;

        public Status CurrentStatus
        {
            get { return mStatus; }
            set { }
        }

        protected string playerName;
        protected string firstPartyPlayerId;
        protected string playerId;
        protected Dictionary<string, string> serverCreds;

        // methods
        public virtual void Start()
        {
        }

        public string PlayerName()
        {
            return playerName;
        }

        public string PlayerId()
        {
            return playerId;
        }

        public string FirstPartyPlayerId()
        {
            return firstPartyPlayerId;
        }

        public Dictionary<string, string> ServerCreds()
        {
            return new Dictionary<string, string>();
        }

        protected void FireFirstPartySuccess()
        {
            if (FirstPartyAuthSuccess != null)
            {
                FirstPartyAuthSuccess();
            }
        }

        protected void FireFirstPartyFailure(string error)
        {
            if (FirstPartyAuthFailure != null)
            {
                FirstPartyAuthFailure(error);
            }
        }

        protected void FireFirstPartyAuthCancel()
        {
            if (FirstPartyAuthCancel != null)
            {
                FirstPartyAuthCancel();
            }
        }

        protected void FireServerAuthSuccess()
        {
            if (ServerAuthSuccess != null)
            {
                ServerAuthSuccess();
            }
        }

        protected void FireServerAuthFailure(string error)
        {
            if (ServerAuthFailure != null)
            {
                ServerAuthFailure(error);
            }
        }

        protected void ClearFirstPartyEvents()
        {
            FirstPartyAuthSuccess = null;
            FirstPartyAuthFailure = null;
            FirstPartyAuthCancel = null;
        }

        protected void ClearServerEvents()
        {
            ServerAuthSuccess = null;
            ServerAuthFailure = null;
        }

        protected void ClearEvents()
        {
            FirstPartyAuthSuccess = null;
            FirstPartyAuthFailure = null;
            FirstPartyAuthCancel = null;
            ServerAuthSuccess = null;
            ServerAuthFailure = null;
        }
    }
}
