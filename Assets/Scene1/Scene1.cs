using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FPAuth;

public class Scene1 : MonoBehaviour
{
    public Text header;
    public Button changeScene;

    void Start()
    {
        changeScene.gameObject.SetActive(false);
        changeScene.onClick.AddListener(OnChangeScenePress);
    }

    void Update()
    {
        switch (AuthInstance.Instance.CurrentStatus)
        {
            case AAuthManager.Status.FirstPartyWorking:
                header.text = "Starting First Party login...";
                changeScene.gameObject.SetActive(true);
                break;
            case AAuthManager.Status.FirstPartySuccess:
                header.text = string.Format("First Party Gamer ID:\n {0}\nGamer Tag: {1}", AuthInstance.Instance.FirstPartyPlayerId(), 
                    AuthInstance.Instance.PlayerName());
                changeScene.gameObject.SetActive(true);
                break;
            case AAuthManager.Status.FirstPartyFailure:
                header.text = "First Party login failed. Check logs for reason.";
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
