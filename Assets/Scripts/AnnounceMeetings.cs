using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.RTVoice;

public class AnnounceMeetings : MonoBehaviour
{
    public InputField pilotMeetingTime;
    public InputField startContestTime;
    private DateTime currentDate;
    public EventView e;
    public QueueControl queueControl;
    public int pilotMeetingSpokenOn;
    public int startContestSpokenOn;
    private bool startPilotMeeting;
    private bool startContest;
    public AudioSource audioSource;
    public SerialClockBoard serialClockBoard;
    private int oldSecond;
    private int diffMeetingSeconds;
    private int diffMeetingMinutes;
    private int diffStartSeconds;
    private int diffStartMinutes;

    private void Start()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        pilotMeetingSpokenOn = -1;
        startContestSpokenOn = -1;
        startPilotMeeting = false;
        startContest = false;
    }
    public void DetermineTimes()
    {
        string speakText = "";
        DateTime currentDate = DateTime.Now;

        int currentSecond = Convert.ToInt32(Math.Floor((double)currentDate.Second));

        // Lets do all of these calcs not on the same second
        if (currentSecond == oldSecond) { return; }

        // Determine pilotMeetingTime values
        if ( pilotMeetingTime.text != "" && pilotMeetingTime.text.Length == 5 )
        {
            DateTime meetingDate = DateTime.Parse(currentDate.ToString("yyyy-MM-dd") + " " + pilotMeetingTime.text);
            diffMeetingSeconds = (int)Math.Ceiling(((meetingDate - currentDate).TotalSeconds));
            diffMeetingMinutes = (int)Math.Ceiling((double)diffMeetingSeconds / 60);
            int secondsRemaining = (int)Math.Ceiling((double)diffMeetingSeconds % 60);
            if (diffMeetingSeconds >= 0 && ((diffMeetingSeconds % 300) == 0 || diffMeetingSeconds == 0 || diffMeetingSeconds == 120) && startPilotMeeting == false)
            {
                // This is on a 5 minute from time, so lets do the announcement
                if (diffMeetingMinutes == 0 && diffMeetingSeconds == 0)
                {
                    speakText = " ATTENTION : Pilots meeting is starting now! Please make your way to the contest organization area. Repeat. Pilots meeting is starting now!";
                    startPilotMeeting = true;
                }
                else
                {
                    // Let them know when the pilots meeting will be starting
                    string pluralMinutes = "";
                    if(diffMeetingMinutes > 1) { pluralMinutes = "s"; }
                    if(diffMeetingMinutes > 60)
                    {
                        int diffHours = diffMeetingMinutes / 60;
                        int diffMin = diffMeetingMinutes % 60;
                        string pluralHours = "";
                        if (diffHours > 1) { pluralHours = "s"; }
                        if (diffMin > 1) { pluralMinutes = "s"; }
                        speakText = " ATTENTION : Pilots meeting will be at " + meetingDate.ToShortTimeString() + ". " + diffHours.ToString() + " hour" + pluralHours + " " + diffMin.ToString() + " minute" + pluralMinutes + " from now. ";
                    }
                    else
                    {
                        speakText = " ATTENTION : Pilots meeting will be at " + meetingDate.ToShortTimeString() + ". " + diffMeetingMinutes.ToString() + " minute" + pluralMinutes + " from now. ";
                    }
                }
            }

            // If there is an entered time in the pilots meeting, and they don't want the clock shown, then lets show the hh:mm on the display until the meeting
            if( PlayerPrefs.GetInt("prefsSendClockTime") == 0 && diffMeetingSeconds > 0 )
            {
                // Lets show the minutes and seconds on the clock
                // If there are more minutes than an hour, then lets just show the current time
                if (diffMeetingMinutes > 60)
                {
                    serialClockBoard.SendSerialData(string.Format("{0:00}{1:00}", DateTime.Now.Hour == 0 ? 12 : DateTime.Now.Hour, DateTime.Now.Minute));
                }
                else
                {
                    if( secondsRemaining == 0)
                    {
                        serialClockBoard.SendSerialData(string.Format("{0:00}{1:00}", diffMeetingMinutes, secondsRemaining));
                    }
                    else
                    {
                        serialClockBoard.SendSerialData(string.Format("{0:00}{1:00}", diffMeetingMinutes - 1, secondsRemaining));
                    }
                    // Let's also set the clock on the local playlist
                    queueControl.clockText.text = e.convertSecondsToClockString(diffMeetingSeconds);
                }
            }
        }
        else
        {
            diffMeetingSeconds = 0;
        }
        // Determine contestStartTime values
        if (startContestTime.text != "" && startContestTime.text.Length == 5 && startPilotMeeting == false && startContest == false && (pilotMeetingTime.text == "" || diffMeetingSeconds <= 0))
        {
            DateTime startDate = DateTime.Parse(currentDate.ToString("yyyy-MM-dd") + " " + startContestTime.text);
            diffStartSeconds = (int)Math.Ceiling(((startDate - currentDate).TotalSeconds));
            diffStartMinutes = (int)Math.Ceiling((double)diffStartSeconds / 60);
            int secondsRemaining = (int)Math.Ceiling((double)diffStartSeconds % 60);
            if (diffStartSeconds >= 0 && ((diffStartSeconds % 300) == 0 || diffStartSeconds == 0 || diffStartSeconds == 120 ))
            {
                // This is on a 5 minute from time, so lets do the announcement
                if(diffStartMinutes == 0 && diffStartSeconds == 0)
                {
                    // Contest is starting !
                    speakText = " ATTENTION : Contest is beginning NOW! Good luck to all pilots. Have fun and fly safe!";
                    startContest = true;
                }
                else
                {
                    // Let them know when the pilots meeting will be starting
                    string pluralMinutes = "";
                    if (diffStartMinutes > 1) { pluralMinutes = "s"; }
                    if (diffStartMinutes > 60)
                    {
                        int diffHours = diffStartMinutes / 60;
                        int diffMin = diffStartMinutes % 60;
                        string pluralHours = "";
                        if (diffHours > 1) { pluralHours = "s"; }
                        if (diffMin > 1) { pluralMinutes = "s"; }
                        speakText = " ATTENTION : Contest will begin at " + startDate.ToShortTimeString() + ". " + diffHours.ToString() + " hour" + pluralHours + " " + diffMin.ToString() + " minute" + pluralMinutes + " from now. ";
                    }
                    else
                    {
                        speakText = " ATTENTION : Contest will begin at " + startDate.ToShortTimeString() + ". " + diffStartMinutes.ToString() + " minute" + pluralMinutes + " from now. ";
                    }
                }
            }
            // If there is an entered time in the start, and they don't want the clock shown, then lets show the hh:mm on the display until the start of the contest
            if (PlayerPrefs.GetInt("prefsSendClockTime") == 0 && diffStartSeconds > 0 && startPilotMeeting != true && diffMeetingSeconds <= 0)
            {
                // Lets show the minutes and seconds on the clock
                // If there are more minutes than an hour, then lets just show the current time
                if(diffStartMinutes > 60)
                {
                    serialClockBoard.SendSerialData(string.Format("{0:00}{1:00}", DateTime.Now.Hour == 0 ? 12 : DateTime.Now.Hour, DateTime.Now.Minute));
                }
                else
                {
                    if (secondsRemaining == 0)
                    {
                        serialClockBoard.SendSerialData(string.Format("{0:00}{1:00}", diffStartMinutes, secondsRemaining));
                    }
                    else
                    {
                        serialClockBoard.SendSerialData(string.Format("{0:00}{1:00}", diffStartMinutes - 1, secondsRemaining));
                    }
                    // Let's also set the clock on the local playlist
                    queueControl.clockText.text = e.convertSecondsToClockString(diffStartSeconds);
                }
            }

        }
        // Now let's speak what we need to speak
        if ( speakText != "")
        {
            if( startPilotMeeting == true || startContest == true )
            {
                // Play a horn to really get their attention
                queueControl.PlayHorn(3);
                // Then speak the delayed speech
                StartCoroutine(SpeakWithDelay( speakText, 4f) );
                startPilotMeeting = false;
            }
            else
            {
                queueControl.noSpeakEndEvent = true;
                Speaker.Instance.Speak(speakText, audioSource, Speaker.Instance.VoiceForName(e.prefs["voice"]), true);
            }
        }
        oldSecond = currentSecond;
        return;
    }
    IEnumerator SpeakWithDelay(string textToSpeak, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        queueControl.noSpeakEndEvent = true;
        Speaker.Instance.Speak(textToSpeak, audioSource, Speaker.Instance.VoiceForName(e.prefs["voice"]), true);
        startPilotMeeting = false;
        startContest = false;
    }

    public void SpeakPilotsMeetingTime()
    {
        string speakText = "";
        DateTime currentDate = DateTime.Now;
        if (pilotMeetingTime.text != "" && pilotMeetingTime.text.Length == 5)
        {
            DateTime meetingDate = DateTime.Parse(currentDate.ToString("yyyy-MM-dd") + " " + pilotMeetingTime.text);
            int diffSeconds = (int)((meetingDate - currentDate).TotalSeconds);

            if (diffSeconds > 0)
            {
                // Let's do the announcement
                int diffMinutes = (int)Math.Ceiling((double)diffSeconds / 60);
                string plural = "";
                if (diffMinutes > 1) { plural = "s"; }
                speakText = " Pilots meeting will be at " + meetingDate.ToShortTimeString() + ". " + diffMinutes.ToString() + " minute" + plural + " from now.";
                queueControl.noSpeakEndEvent = true;
                Speaker.Instance.Speak(speakText, audioSource, Speaker.Instance.VoiceForName(e.prefs["voice"]), true);
            }
        }
        return;
    }
    public void SpeakContestStartTime()
    {
        string speakText = "";
        DateTime currentDate = DateTime.Now;
        if (startContestTime.text != "" && startContestTime.text.Length == 5)
        {
            DateTime startDate = DateTime.Parse(currentDate.ToString("yyyy-MM-dd") + " " + startContestTime.text);
            int diffSeconds = (int)((startDate - currentDate).TotalSeconds);

            if (diffSeconds > 0)
            {
                // Let's do the announcement
                int diffMinutes = (int)Math.Ceiling((double)diffSeconds / 60);
                string plural = "";
                if (diffMinutes > 1) { plural = "s"; }
                speakText = " Contest will begin at " + startDate.ToShortTimeString() + ". " + diffMinutes.ToString() + " minute" + plural + " from now.";
                queueControl.noSpeakEndEvent = true;
                Speaker.Instance.Speak(speakText, audioSource, Speaker.Instance.VoiceForName(e.prefs["voice"]), true);
            }
        }
        return;
    }
    public void MeetingFormats()
    {
        // Method to check the meeting and start time formats and update them to make entry easier
        if (pilotMeetingTime.text.Length == 3 && Convert.ToInt32(pilotMeetingTime.text.Substring(0, 1)) < 10 && pilotMeetingTime.text.Substring(0, 1) != "0" && pilotMeetingTime.text.IndexOf(':') == -1)
        {
            pilotMeetingTime.text = "0" + pilotMeetingTime.text.Substring(0, 1) + ":" + pilotMeetingTime.text.Substring(1, 2);
        }
        if (pilotMeetingTime.text.Length == 4 && pilotMeetingTime.text.IndexOf(':') == 1)
        {
            pilotMeetingTime.text = "0" + pilotMeetingTime.text;
        }
        if (pilotMeetingTime.text.Length == 4 && pilotMeetingTime.text.IndexOf(':') == -1)
        {
            pilotMeetingTime.text = pilotMeetingTime.text.Substring(0, 2) + ":" + pilotMeetingTime.text.Substring(2, 2);
        }
        if (startContestTime.text.Length == 3 && Convert.ToInt32(startContestTime.text.Substring(0, 1)) < 10 && startContestTime.text.Substring(0, 1) != "0" && startContestTime.text.IndexOf(':') == -1)
        {
            startContestTime.text = "0" + startContestTime.text.Substring(0, 1) + ":" + startContestTime.text.Substring(1, 2);
        }
        if (startContestTime.text.Length == 4 && startContestTime.text.IndexOf(':') == 1)
        {
            startContestTime.text = "0" + startContestTime.text;
        }
        if (startContestTime.text.Length == 4 && startContestTime.text.IndexOf(':') == -1)
        {
            startContestTime.text = startContestTime.text.Substring(0, 2) + ":" + startContestTime.text.Substring(2, 2);
        }
        return;
    }
    // Update is called once per frame
    void Update()
    {
        if (queueControl.queueTimerRunning == false)
        {
            // We are going to be checking if there is a time entered in the box for pilots meeting
            DetermineTimes();
            if (startContest == true)
            {
                serialClockBoard.SendSerialData(string.Format("{0:00}{1:00}", 0, 0));
                queueControl.clockText.text = e.convertSecondsToClockString(0);
                StartCoroutine(StartContest());
                startContest = false;
            }
            MeetingFormats();
        }
        return;
    }

    IEnumerator StartContest()
    {
        // Trigger the audio playlist to start in 8 seconds after the announcement
        yield return new WaitForSecondsRealtime(12f);
        queueControl.TogglePlayPause();
    }

}
