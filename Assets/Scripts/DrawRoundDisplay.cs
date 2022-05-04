using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawRoundDisplay : MonoBehaviour
{
    public Text roundNumber;
    public EventRound round;
    // Start is called before the first frame update
    void Start()
    {
        if (round != null) Prime(round);
    }

    public void Prime(EventRound round)
    {
        this.round = round;
        if (roundNumber != null)
            roundNumber.text = "Round " + round.round_number;
    }
}
