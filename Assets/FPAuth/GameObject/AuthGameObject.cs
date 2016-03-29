using UnityEngine;
using System;
using System.Collections;

namespace FPAuth
{
    public class AuthGameObject : MonoBehaviour
    {
        public enum FPLoginStatus
        {
            Working,
            Success,
            Failure,
            Cancel
        }

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

            AuthInstance.Instance.Init();
        }

        void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }
                
            Instance = null;
        }

        void Update()
        {
	
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
                
            }
        }

        void LoginResult(string result)
        {
            FPLoginStatus status = (FPLoginStatus)Enum.Parse(typeof(FPLoginStatus), result, true);
            
            switch (status)
            {
                case FPLoginStatus.Success:
                    AuthInstance.Instance.FireFirstPartyAuthSuccess();
                    break;
                case FPLoginStatus.Cancel:
                    AuthInstance.Instance.FireFirstPartyAuthCancel();
                    break;
                case FPLoginStatus.Failure:
                    AuthInstance.Instance.FireFirstPartyAuthFailure(AuthInstance.Instance.FailureError());
                    break;
                case FPLoginStatus.Working:
                default:
                    Debug.Log("LoginResult returned working or garbage");
                    break;
            }
        }
    }
}