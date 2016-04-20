using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FPAuth;

public class Scene1 : MonoBehaviour
{
    public Text playerId;
    public Text serverPlayerId;
    public Text gamerTag;
    public Text isAnonymous;
    public Text sessionToken;
    public Text error;

//    public Button changeScene;
   
    public bool workingFlag = false;
    public bool successfulFlag = false;
    public bool failureFlag = false;

    void Start()
    {
		playerId.text = "First Party Player Id:";
        serverPlayerId.text = "Server Player Id:";
        gamerTag.text = "Gamer Tag:";
        isAnonymous.text = "Is Anonymous:";
        sessionToken.text = "Session token:";
    }

    void Update()
    {
        switch (AuthInstance.Instance.CurrentStatus)
        {
            case AAuthManager.Status.Working:
                if (!workingFlag)
                {
                    error.text = "Starting First Party login...";
                    workingFlag = true;
                }
                break;
            case AAuthManager.Status.Success:
                if (!successfulFlag)
                {
                    playerId.text = string.Format("First Party Player Id: {0}", AuthInstance.Instance.FirstPartyPlayerId());
                    serverPlayerId.text = string.Format("Server Player Id: {0}", AuthInstance.Instance.PlayerId());
                    gamerTag.text = string.Format("Gamer Tag: {0}", AuthInstance.Instance.PlayerName());
                    isAnonymous.text = string.Format("Is Anonymous: {0}", AuthInstance.Instance.IsAnonymous());
                    sessionToken.text = string.Format("Session token: {0}", AuthInstance.Instance.SessionToken());
                    error.text = "Finished";
                    successfulFlag = true;
                }

                break;
            case AAuthManager.Status.Failure:
                if(!failureFlag)
                {
                    error.text = string.Format("Error: {0}", AuthInstance.Instance.FailureError());
                    failureFlag = true;
                }
                break;
            default:
                break;
        }
    }
}
