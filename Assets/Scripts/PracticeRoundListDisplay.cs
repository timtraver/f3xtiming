using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PracticeRoundListDisplay : MonoBehaviour
{
    public Text roundNum;
    public Text roundDescription;
    public Text minuteTask;
    public EventTask roundTask;
    public Practice practice;

    // Start is called before the first frame update
    void Start()
    {
        if (roundTask != null) Prime(roundTask);
    }

    public void Prime(EventTask roundTask)
    {
        this.roundTask = roundTask;
        if (roundNum != null)
            roundNum.text = roundTask.round_number.ToString();
        if (roundDescription != null)
            roundDescription.text = roundTask.flight_type_name;
        if (minuteTask != null)
            minuteTask.text = (roundTask.event_task_time_choice / 60).ToString();
    }
    public void deleteEntry()
    {
        // Search for the roundList object
        practice = FindObjectOfType<Practice>();
        practice.roundList.Remove(roundTask);
        // now remove this object
        //Object.Destroy(this.gameObject);
        practice.DisplayTasks();
        return;
    }
}
