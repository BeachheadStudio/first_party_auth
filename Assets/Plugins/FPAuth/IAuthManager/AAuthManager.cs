using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace FPAuth
{
    public abstract class AAuthManager : IAuthManager
    {
#if UNITY_ANDROID
        // Thread reference for JNI calls
        protected Thread mainThread;
        public Thread MainThread
        {
            get { return mainThread; }
            set { mainThread = value;}
        }

        protected AndroidJavaClass mainClass;

        public AndroidJavaClass MainClass
        {
            get { return mainClass; }
            set { mainClass = value; }
        }
#endif
        public static readonly string PLAYER_ID_KEY = "FPAuth.PlayerId";

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
        public event Action AuthSuccess;
        public event Action<string> AuthFailure;
        public event Action AuthCancel;
        public event Action PlayerChangeEvent;

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
            throw new Exception("Not Implemented");
        }

        public virtual void OnPause()
        {
            throw new Exception("Not Implemented");
        }

        public virtual void OnResume()
        {
            throw new Exception("Not Implemented");
        }

        public virtual string FailureError()
        {
            throw new Exception("Not Implemented");
        }

        public virtual void Log(AAuthManager.LogLevel level, string message)
        {
            throw new Exception("Not Implemented");
        }

        public virtual string PlayerName()
        {
            throw new Exception("Not Implemented");
        }

        public virtual string PlayerId()
        {
            throw new Exception("Not Implemented");
        }

        public virtual string FirstPartyPlayerId()
        {
            throw new Exception("Not Implemented");
        }

        public virtual string SessionToken()
        {
            throw new Exception("Not Implemented");
        }

        public virtual bool IsAnonymous()
        {
            throw new Exception("Not Implemented");
        }

        public virtual Dictionary<string, string> GetAuthParams()
        {
            throw new Exception("Not Implemented");
        }

        public virtual void AwardAchievement(string achievementId)
        {
            throw new Exception("Not Implemented");
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

#if UNITY_ANDROID && !UNITY_EDITOR
        protected bool IsMainThread()
        {
        return mainThread == Thread.CurrentThread;
        }
#endif
    }
}
