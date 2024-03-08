using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PetrushevskiApps.Utilities;

public class MainMenu : MonoBehaviour
{
    public Button resumeButton;
    public Button searchButton;
    public Button connectedButton;
    public Text connectedButtonText;
    public Text softwareVersion;

    private void Awake()
    {
        // Create connectivity manager listener
        ConnectivityManager.Instance.AddConnectivityListener(OnConnectivityChange);
    }
    // Start is called before the first frame update
    void Start()
    {
        int maxFrameRate = 40;
        Application.targetFrameRate = maxFrameRate;
        // 1 = match monitor refresh rate. 0 = Don't use vsync: use targetFrameRate instead.
        QualitySettings.vSyncCount = 0;

        connectionCheck();

        // Let's resume the event timing for an event if the eventTimer or queueTimer are going
        if( PlayerPrefs.GetInt("queueTimerRunning") == 1 && PlayerPrefs.GetInt("queueEventID") != 0 )
        {
            resumeButton.gameObject.SetActive(true);
        }
        else
        {
            resumeButton.gameObject.SetActive(false);
        }
        softwareVersion.text = new SoftwareVersion().versionString;
        Debug.Log("version = " + softwareVersion.text);
    }

    public void OnConnectivityChange( bool isConnected, string errorMsg)
    {
        connectionCheck();
    }
    public void connectionCheck()
    {
        // Method to do whatever is needed when connection is up or down
        if (ConnectivityManager.Instance.IsConnected)
        {
            Debug.Log("isConnected is true");
            // Make search button active and turn it green
            searchButton.interactable = true;
            searchButton.image.color = new Color(0.1595837f, 0.6981132f, 0.06256673f, 1f);
            connectedButtonText.text = "Internet Connected";
            connectedButton.image.color = new Color(0.1595837f, 0.6981132f, 0.06256673f, 1f);
        }
        else
        {
            Debug.Log("isConnected is false");
            // Make search button inactive and turn it red
            searchButton.interactable = false;
            searchButton.image.color = new Color(0.7215686f, 0.1820968f, 0.1764706f, 1f);
            connectedButtonText.text = "Internet NOT Connected";
            connectedButton.image.color = new Color(0.7215686f, 0.1820968f, 0.1764706f, 1f);
        }
        return;
    }
    public void onClickResumeButton()
    {
        // Change the scene directly to the event view for the saved event
        SceneManager.LoadScene("EventDetails");
        return;
    }
}
