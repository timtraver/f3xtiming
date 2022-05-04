using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EventListDisplay : MonoBehaviour
{
    public InputField eventID;
    public Text eventName;
    public Text eventDate;
    public Text eventType;
    public Text eventLocation;

    public EventSearchInfo entry;

    private void Start()
    {
        if (entry != null) Prime(entry);
    }

    public void Prime(EventSearchInfo entry)
    {
        this.entry = entry;
        eventID.text = entry.event_id.ToString();
        if (eventDate != null)
            eventDate.text = entry.event_start_date;
        if (eventName != null)
            eventName.text = entry.event_name;
        if (eventType != null)
            eventType.text = entry.event_type_name;
        if (eventLocation != null)
            eventLocation.text = entry.location_name;

    }
    public void onButtonClick()
    {
        //Debug.Log(" The eventID = " + this.eventID.text);
        //Debug.Log(" The event name = " + this.eventName.text);
        PlayerPrefs.SetInt("eventID", this.entry.event_id);
        PlayerPrefs.Save();
        SceneManager.LoadScene("EventDetails");
        return;
    }

}
