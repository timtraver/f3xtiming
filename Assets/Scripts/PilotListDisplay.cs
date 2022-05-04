using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PilotListDisplay : MonoBehaviour
{
    public Text pilotBib;
    public Text pilotName;
    public EventPilot pilot;
    public PilotNamePopup pilotPopup;

    // Start is called before the first frame update
    void Start()
    {
        pilotPopup = FindObjectOfType<PilotNamePopup>(true);
        if (pilot != null) Prime(pilot);
    }

    public void Prime(EventPilot pilot)
    {
        this.pilot = pilot;
        if (pilotBib != null)
            pilotBib.text = pilot.pilot_bib.ToString();
        if (pilotName != null)
            pilotName.text = pilot.pilot_first_name + " " + pilot.pilot_last_name;
    }

    public void EditPhoneme()
    {
        // use the popup to edit the phoneme for this pilot name
        if(pilotPopup == null)
        {
            Debug.Log("pilotPopup is null");
        }
        pilotPopup.Show(pilot);
    }
}
