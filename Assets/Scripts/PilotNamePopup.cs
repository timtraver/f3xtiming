using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.RTVoice;
using Crosstales.RTVoice.Model;

public class PilotNamePopup : MonoBehaviour
{
    public Text pilotName;
    public InputField phonemeName;
    public EventPilot pilot;
    public QueueControl queueControl;
    public EventView e;

    private void Awake()
    {
        Hide();
    }
    public void Show(EventPilot pilot)
    {
        gameObject.SetActive(true);
        pilotName.text = pilot.pilot_first_name + " " + pilot.pilot_last_name;
        phonemeName.text = PlayerPrefs.GetString(pilotName.text, pilotName.text);
        return;
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        return;
    }

    public void TestVoice()
    {
        queueControl.noSpeakEndEvent = true;
        Speaker.Instance.Speak(phonemeName.text, e.audioSource, Speaker.Instance.VoiceForName(e.prefs["voice"]), true);
        return;
    }
    public void SavePilotPronounciation()
    {
        if (phonemeName.text != pilotName.text)
        {
            PlayerPrefs.SetString(pilotName.text, phonemeName.text);
        }
        else
        {
            PlayerPrefs.DeleteKey(pilotName.text);
        }
        Hide();
    }
}
