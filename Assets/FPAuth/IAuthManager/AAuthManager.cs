using System;
using System.Collections.Generic;

namespace FPAuth
{
    public abstract class AAuthManager : IAuthManager
    {
        // log level enum
        public enum LogLevel
        {
            VERBOSE,
            DEBUG,
            INFO,
            WARN,
            ERROR,
            ASSERT
        }

        // event handlers
        public static event Action FirstPartyAuthSuccess;
        public static event Action<string> FirstPartyAuthFailure;
        public static event Action FirstPartyAuthCancel;
        public static event Action ServerAuthSuccess;
        public static event Action<string> ServerAuthFailure;

        // parameters
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

        // methods
        public virtual void Init()
        {
        }

        public virtual void OnPause()
        {
        }

        public virtual void OnResume()
        {
        }

        public virtual string FailureError()
        {
            return null;
        }

        public virtual void Log(AAuthManager.LogLevel level, string message)
        {   
        }

        public virtual string PlayerName()
        {
            return null;
        }

        public virtual string PlayerId()
        {
            return null;
        }

        public virtual string FirstPartyPlayerId()
        {
            return null;
        }

        public virtual string SessionToken()
        {
            return null;
        }

        public virtual void FireFirstPartyAuthSuccess()
        {
            mStatus = Status.FirstPartySuccess;

            if (FirstPartyAuthSuccess != null)
            {
                FirstPartyAuthSuccess();
            }
            ClearFirstPartyEvents();
        }

        public void FireFirstPartyAuthFailure(string error)
        {
            mStatus = Status.FirstPartyFailure;
            if (FirstPartyAuthFailure != null)
            {
                FirstPartyAuthFailure(error);
            }
            ClearFirstPartyEvents();
        }

        public void FireFirstPartyAuthCancel()
        {
            mStatus = Status.FirstPartyFailure;
            if (FirstPartyAuthCancel != null)
            {
                FirstPartyAuthCancel();
            }
            ClearFirstPartyEvents();
        }

        public void FireServerAuthSuccess()
        {
            if (ServerAuthSuccess != null)
            {
                ServerAuthSuccess();
            }
            ClearServerEvents();
        }

        public void FireServerAuthFailure(string error)
        {
            if (ServerAuthFailure != null)
            {
                ServerAuthFailure(error);
            }
            ClearServerEvents();
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
