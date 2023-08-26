using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawRoundDisplay : MonoBehaviour
{
    public Text roundNumber;
    public Text sentGroup;
    public EventRound round;
    public EventRoundFlight flight;
    // Start is called before the first frame update
    void Start()
    {
        if (round != null) Prime(round, flight);
    }

    public void Prime(EventRound round, EventRoundFlight flight)
    {
        this.round = round;
        this.flight = flight;
        if (roundNumber != null)
            roundNumber.text = "Round " + round.round_number + "    " + "Group " + flight.group;
    }
}
