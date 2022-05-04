using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Button resumeButton;

    // Start is called before the first frame update
    void Start()
    {
        int maxFrameRate = 40;
        Application.targetFrameRate = maxFrameRate;
        // 1 = match monitor refresh rate. 0 = Don't use vsync: use targetFrameRate instead.
        QualitySettings.vSyncCount = 0;

        // Let's resume the event timing for an event if the eventTimer or queueTimer are going
        if( PlayerPrefs.GetInt("queueTimerRunning") == 1 && PlayerPrefs.GetInt("queueEventID") != 0 )
        {
            resumeButton.gameObject.SetActive(true);
        }
        else
        {
            resumeButton.gameObject.SetActive(false);
        }
    }

    public void onClickResumeButton()
    {
        // Change the scene directly to the event view for the saved event
        SceneManager.LoadScene("EventDetails");
        return;
    }
}
