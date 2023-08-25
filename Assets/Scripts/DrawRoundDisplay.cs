using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawRoundDisplay : MonoBehaviour
{
    public Text roundNumber;
    public Text groupText;
    public EventRound round;
    // Start is called before the first frame update
    void Start()
    {
        if (round != null) Prime(round, groupText.text);
    }

    public void Prime(EventRound round, string group)
    {
        this.round = round;
        if (roundNumber != null)
            roundNumber.text = "Round " + round.round_number + "    " + "Group " + group;
    }
}
