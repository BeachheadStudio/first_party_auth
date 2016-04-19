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
                error.text = "Starting First Party login...";
                break;
            case AAuthManager.Status.Success:
            	playerId.text = string.Format("First Party Player Id: {0}", AuthInstance.Instance.FirstPartyPlayerId());
                serverPlayerId.text = string.Format("Server Player Id: {0}", AuthInstance.Instance.PlayerId());
                gamerTag.text = string.Format("Gamer Tag: {0}", AuthInstance.Instance.PlayerName());
                isAnonymous.text = string.Format("Is Anonymous: {0}", AuthInstance.Instance.IsAnonymous());
                sessionToken.text = string.Format("Session token: {0}", AuthInstance.Instance.SessionToken());

                break;
            case AAuthManager.Status.Failure:
            	error.text = string.Format("Error: {0}", AuthInstance.Instance.FailureError());
                break;
            default:
                break;
        }
    }

    public void OnChangeScenePress()
    {
        SceneManager.LoadScene("Scene2");
    }
}
