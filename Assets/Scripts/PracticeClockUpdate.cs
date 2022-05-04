using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PracticeClockUpdate : MonoBehaviour
{
    public Text ClockTime;
    public int hour;
    public int minutes;
    public int seconds;
    public int secondsOld;

    // Start is called before the first frame update
    void Start()
    {
        this.UpdateClockTime();
    }

    void UpdateClockTime()
    {
        if (System.DateTime.Now.Second % 2 == 0) {
            ClockTime.text = System.DateTime.Now.ToString("hh:mm tt").ToLower();
        }
        else
        {
            ClockTime.text = System.DateTime.Now.ToString("hh mm tt").ToLower();
        }
        secondsOld = seconds;
        return;
    }
    // Update is called once per frame
    void Update()
    {
        seconds = System.DateTime.Now.Second;
        if (seconds != secondsOld)
        {
            hour = System.DateTime.Now.Hour;
            minutes = System.DateTime.Now.Minute;
            this.UpdateClockTime();
        }
        return;
    }
}
