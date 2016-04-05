using UnityEngine;
using System;
using System.Collections;

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

            if(!AuthInstance.Instance.IsAuthenticated())
            {
                AuthInstance.Instance.Init();
            }
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
            }
            else
            {
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