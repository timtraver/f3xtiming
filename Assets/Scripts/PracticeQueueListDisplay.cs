using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PracticeQueueListDisplay : MonoBehaviour
{
    public Text queueSequence;
    public Image queuePlayImage;
    public Text queueDescription;
    public PlayQueueEntry entry;
    public PracticeQueueControl queueControl;
    public float doubleClickTime = .4f, lastClickTime;

    // Start is called before the first frame update
    void Start()
    {
        if (entry != null){ Prime(entry); }
    }

    public void Prime(PlayQueueEntry entry)
    {
        queueSequence.text = entry.sequenceID.ToString();
        queuePlayImage.enabled = false;
        queueDescription.text = entry.textDescription;
    }
    public void onQueueEntryClick()
    {
        // Search for the QueueControl object
        queueControl = FindObjectOfType<PracticeQueueControl>();
        queueControl.lastQueueEntry = queueControl.currentQueueEntry;
        queueControl.QueueToSpecificEntry(System.Convert.ToInt32(queueSequence.text) - 1);
        return;
    }
    public void onQueueEntryDoubleClick()
    {
        // Checking mouse double click
        float timeSinceLastClick = Time.time - lastClickTime;
        if (timeSinceLastClick <= doubleClickTime)
        {
            onQueueEntryClick();
        }
        lastClickTime = Time.time;
    }

}
