using UnityEngine;
using System;
using System.Collections;
using System.Threading;

namespace FPAuth
{
    public class AuthGameObject : MonoBehaviour
    {
        public static AuthGameObject Instance = null;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject); // There can be only one
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
#if !UNITY_EDITOR && (UNITY_ANDROID && !KINDLE_BUILD)
			AuthInstance.Instance.MainThread = Thread.CurrentThread;
            AuthInstance.Instance.MainClass = new AndroidJavaClass("com.singlemalt.googleplay.auth.googleplayauth.AuthInstance");
#elif KINDLE_BUILD && !UNITY_EDITOR
			AuthInstance.Instance.MainThread = Thread.CurrentThread;
            AuthInstance.Instance.MainClass = new AndroidJavaClass("com.singlemalt.amazon.auth.amazonauth.AuthInstance");
#endif // UNITY_ANDROID && !KINDLE_BUILD
            
        }

        void Start()
        {
            if(!AuthInstance.Instance.IsAuthenticated())
            {
                AuthInstance.Instance.Init();
            }
        }

        void Update()
        {

        }

        void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                AuthInstance.Instance.OnPause();
#if UNITY_ANDROID && !UNITY_EDITOR
                AuthInstance.Instance.MainClass.Dispose();
                AuthInstance.Instance.MainClass = null;
                AuthInstance.Instance.MainThread = null;
#endif
            }
            else
            {
#if !UNITY_EDITOR && (UNITY_ANDROID && !KINDLE_BUILD)
			AuthInstance.Instance.MainThread = Thread.CurrentThread;
            AuthInstance.Instance.MainClass = new AndroidJavaClass("com.singlemalt.googleplay.auth.googleplayauth.AuthInstance");
#elif KINDLE_BUILD && !UNITY_EDITOR
			AuthInstance.Instance.MainThread = Thread.CurrentThread;
            AuthInstance.Instance.MainClass = new AndroidJavaClass("com.singlemalt.amazon.auth.amazonauth.AuthInstance");
#endif // UNITY_ANDROID && !KINDLE_BUILD
                AuthInstance.Instance.OnResume();
            }
        }

        void PlayerChange(string result)
        {
            if (bool.Parse(result))
            {
                AuthInstance.Instance.FirePlayerChangeEvent();
            }
        }

        void LoginResult(string result)
        {
            AAuthManager.Status status = (AAuthManager.Status)Enum.Parse(typeof(AAuthManager.Status), result, true);

            switch (status)
            {
                case AAuthManager.Status.Success:
                    AuthInstance.FailCount = 0;
                    AuthInstance.Instance.FireAuthSuccess();
                    break;
                case AAuthManager.Status.Cancel:
                    AuthInstance.FailCount = 0;
                    AuthInstance.Instance.FireAuthCancel();
                    break;
                case AAuthManager.Status.Failure:
                    Debug.LogError("Auth Failed: " + AuthInstance.Instance.FailureError());
                    if(AuthInstance.FailCount < 3) {
                        AuthInstance.FailCount++;
                        AuthInstance.Instance.Init();
                    } else {
                        AuthInstance.FailCount = 0;
                        AuthInstance.Instance.FireAuthFailure(AuthInstance.Instance.FailureError());
                    }
                    break;
                case AAuthManager.Status.Working:
                default:
                    Debug.Log("LoginResult returned working or garbage");
                    break;
            }
        }
    }
}
