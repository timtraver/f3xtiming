using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PilotMeetingFocus : MonoBehaviour
{
    InputField pilotMeeting;
    void Start()
    {
        //Fetch the Input Field component from the GameObject
        pilotMeeting = GetComponent<InputField>();
    }

    void Update()
    {
        if (pilotMeeting.isFocused)
        {
            pilotMeeting.GetComponent<Image>().color = Color.green;
            pilotMeeting.Select();
        }
        if (!pilotMeeting.isFocused && pilotMeeting.text == "")
        {
            pilotMeeting.GetComponent<Image>().color = Color.white;
        }
    }
}
