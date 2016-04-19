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
        public static event Action AuthSuccess;
        public static event Action<string> AuthFailure;
        public static event Action AuthCancel;
        public static event Action PlayerChangeEvent;

        // parameters
        public enum Status
        {
            Working,
            Success,
            Cancel,
            Failure
        }

        protected Status mStatus;

        public Status CurrentStatus
        {
            get { return mStatus; }
            set { }
        }

        public bool IsAuthenticated()
        {
            return (mStatus == Status.Success || mStatus == Status.Cancel);
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

        public virtual bool IsAnonymous()
        {
            return false;
        }

        public void FireAuthCancel()
        {
            mStatus = Status.Cancel;
            if (AuthCancel != null)
            {
                AuthCancel();
            }
        }

        public virtual void FireAuthSuccess()
        {
            mStatus = Status.Success;
            if (AuthSuccess != null)
            {
                AuthSuccess();
            }
        }

        public void FireAuthFailure(string error)
        {
            mStatus = Status.Failure;
            if (AuthFailure != null)
            {
                AuthFailure(error);
            }
        }

        public void FirePlayerChangeEvent()
        {
            if(PlayerChangeEvent != null)
            {
                PlayerChangeEvent();
            }
        }

        protected void ClearEvents()
        {
            AuthSuccess = null;
            AuthFailure = null;
            AuthCancel = null;
        }
    }
}
