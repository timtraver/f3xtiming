using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawDisplay : MonoBehaviour
{
    public Text pilotGroup;
    public Text pilotName;
    public Text pilotLane;
    public Image pilotEntered;
    public Image pilotNotEntered;
    public EventRound round;
    public EventRoundFlight flight;
    public QueueControl queueControl;
    public int queueControlExists;

    // Start is called before the first frame update
    void Start()
    {
        if (FindObjectOfType<QueueControl>() != null)
        {
            queueControl = FindObjectOfType<QueueControl>();
            queueControlExists = 1;
        }
        else
        {
            queueControlExists = 0;
        }
        if (flight != null) Prime( round, flight );
    }

    public void Prime(EventRound round, EventRoundFlight flight)
    {
        this.round = round;
        this.flight = flight;
        if (pilotGroup != null)
            pilotGroup.text = flight.group;
        if (pilotName != null)
            pilotName.text = flight.pilot_first_name + " " + flight.pilot_last_name;
        if (pilotLane != null)
            pilotLane.text = flight.lane;
        if (pilotEntered != null)
        {
            // Ok lets find out what the current round is and only show an entered or not entered for previous rounds
            if (queueControlExists == 1)
            {
                if ((round.round_number >= queueControl.e.playList[queueControl.currentQueueEntry].round_number || queueControl.currentQueueEntry == 0) && queueControl.lastQueueEntry != 0 && queueControl.e.playList[queueControl.currentQueueEntry].round_number != 0)
                {
                    pilotEntered.enabled = false;
                    pilotNotEntered.enabled = false;
                }
                else
                {
                    pilotEntered.enabled = flight.entered == 1 ? true : false;
                    pilotNotEntered.enabled = flight.entered == 0 ? true : false;
                }
            }
            else
            {
                pilotEntered.enabled = false;
                pilotNotEntered.enabled = false;
            }
        }
    }
}