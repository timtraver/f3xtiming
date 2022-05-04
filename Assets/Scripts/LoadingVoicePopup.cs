using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingVoicePopup : MonoBehaviour
{
    public Text loadingMessage;
    public int loadedPhrases;

    private void Awake()
    {
        Hide();
    }
    public void Show()
    {
        loadedPhrases = 0;
        gameObject.SetActive(true);
        loadingMessage.text = "";
        return;
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        return;
    }

}
