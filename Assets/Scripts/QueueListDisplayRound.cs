using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QueueListDisplayRound : MonoBehaviour
{
    public Text queueSequence;
    public Image queuePlayImage;
    public Text queueDescription;
    public PlayQueueEntry entry;
    public QueueControl queueControl;
    public float doubleClickTime = .2f, lastClickTime;

    // Start is called before the first frame update
    void Start()
    {
        if (entry != null) { Prime(entry); }
    }

    public void Prime(PlayQueueEntry entry)
    {
        queueSequence.text = entry.sequenceID.ToString();
        queuePlayImage.enabled = false;
        queueDescription.text = entry.textDescription;
    }
    public void onQueueEntryRoundClick()
    {
        // Search for the QueueControl object
        queueControl = FindObjectOfType<QueueControl>();
        queueControl.lastQueueEntry = queueControl.currentQueueEntry;
        queueControl.QueueToSpecificEntry(System.Convert.ToInt32(queueSequence.text) - 1);
        return;
    }
    public void onQueueEntryRoundDoubleClick()
    {
        // Checking mouse double click
        float timeSinceLastClick = Time.time - lastClickTime;
        if (timeSinceLastClick <= doubleClickTime)
        {
            onQueueEntryRoundClick();
        }
        lastClickTime = Time.time;
    }
}
