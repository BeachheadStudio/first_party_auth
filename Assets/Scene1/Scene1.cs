using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using FPAuth;

public class Scene1 : MonoBehaviour
{
    public Text header;
    public Text timer;
    private int timeLeft = 10;
    private bool switchAvailable = false;

    // Use this for initialization
    void Start()
    {
        // start the auth manager
        AAuthManager manager = AuthManager.Instance;
    }
	
    // Update is called once per frame
    void Update()
    {
        switch (AuthManager.Instance.CurrentStatus)
        {
            case AAuthManager.Status.Init:
                timeLeft -= (int)Time.deltaTime;
                timer.text = string.Format("{0}", timeLeft);

                if (timeLeft < 0)
                {
                    StartFPAuth();
                }
                break;
            case AAuthManager.Status.FirstPartyWorking:
                if (!switchAvailable)
                {
                    timeLeft -= (int)Time.deltaTime;
                    timer.text = string.Format("{0}", timeLeft);

                    if (timeLeft < 0)
                    {
                        SwitchScene();
                    }
                }
                break;
            case AAuthManager.Status.FirstPartySuccess:

                break;
            case AAuthManager.Status.FirstPartyFailure:

                break;
        }
    }

    void StartFPAuth()
    {
        AuthManager.Instance.Start();

        timeLeft = 10;
    }

    void SwitchScene()
    {
        switchAvailable = true;

        // TODO: unhide button

    }
}
