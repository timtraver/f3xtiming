using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawDisplay : MonoBehaviour
{
    public Text pilotGroup;
    public Text pilotName;
    public Text pilotLane;
    public EventRoundFlight flight;

    // Start is called before the first frame update
    void Start()
    {
        if (flight != null) Prime(flight);
    }

    public void Prime(EventRoundFlight flight)
    {
        this.flight = flight;
        if (pilotGroup != null)
            pilotGroup.text = flight.group;
        if (pilotName != null)
            pilotName.text = flight.pilot_first_name + " " + flight.pilot_last_name;
        if (pilotLane != null)
            pilotLane.text = flight.lane;
    }
}
