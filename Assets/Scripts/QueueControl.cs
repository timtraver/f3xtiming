using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.RTVoice;

public class QueueControl : MonoBehaviour
{
    // UI Object definitions
    public Slider countDownSlider;
    public Button playButton;
    public Button forwardButton;
    public Button backwardButton;
    public Button forward10Button;
    public Button backward10Button;
    public Text clockText;
    public ScrollRect playListScroll;
    public Sprite playImage;
    public Sprite pauseImage;
    public LoadingVoicePopup loadingVoicePopup;

    public Crosstales.RTVoice.Model.Voice voice;

    // Main playlist List object
    public EventView e;

    public double clockCurrentSeconds;
    public double clockTotalSeconds;
    public int clockOldSeconds;
    public double clockToStamp;
    public bool clockTimerRunning;
    public bool queueTimerRunning;
    public int currentQueueEntry;
    public int lastQueueEntry;
    public bool goToNextQueueEntry;
    public float scrollValue = 1.0f;
    public List<PlayQueueEntry> playList;
    public bool noSpeakEndEvent;

    public int queueTimeRemaining;
    public Text timeRemaining;
    public Text endTime;
    public List<String> preloadPhrases = new List<string>();
    public GlobalCache globalCache;
    public int phrasesToLoad;

    private void Start()
    {
        // Use this to initialize any variables
        // This will also be used to load the last saved time if we are coming back
        clockCurrentSeconds = 0.0;
        clockTotalSeconds = 0.0;
        clockOldSeconds = 0;
        clockToStamp = 0.0;
        clockTimerRunning = false;
        queueTimerRunning = false;
        currentQueueEntry = 0;
        lastQueueEntry = -1;
        noSpeakEndEvent = false;
        playList = e.playList;
        playListScroll.verticalNormalizedPosition = 1.0f;
        // Now load the playlist state if needed
        LoadQueueState();
        return;
    }


    public void TogglePlayPause()
    {
        // Function to start and stop the main queue from running (including the clock)
        if( queueTimerRunning == true)
        {
            // Pause the timer and queue and stop any voices from playing
            queueTimerRunning = false;
            clockTimerRunning = false;
            // Line to get the speaking to stop
            noSpeakEndEvent = true;
            Speaker.Instance.Silence();
            e.audioSource.Stop();
            // Change sprite to pause image
            playButton.image.sprite = playImage;
            // Make the create playlist button available
            e.createPlayListButton.enabled = true;
            e.createPlayListButton.interactable = true;
            // Make the preload voice cache button available
            e.preloadButton.enabled = true;
            e.preloadButton.interactable = true;
        }
        else
        {
            e.audioSource.volume = 1.0f;
            // Make the create playlist button disabled
            e.createPlayListButton.enabled = false;
            e.createPlayListButton.interactable = false;
            // Make the preload voice cache button disabled
            e.preloadButton.enabled = false;
            e.preloadButton.interactable = false;

            // Set the voice
            voice = Speaker.Instance.VoiceForName(e.prefs["voice"]);

            // Change sprite to pause image
            playButton.image.sprite = pauseImage;

            // Start queue Timer
            queueTimerRunning = true;
            noSpeakEndEvent = false;
            if( playList[currentQueueEntry].hasTimer == true)
            {
                if( clockCurrentSeconds == clockTotalSeconds)
                {
                    goToNextQueueEntry = true;
                }
                else
                {
                    clockTimerRunning = true;
                    clockToStamp = getTimeStamp() + clockCurrentSeconds;
                }
            }
            else
            {
                goToNextQueueEntry = true;
            }
            UpdateVerticalScroll();
        }
        CalculateQueueTimeRemaining();
        return;
    }
    public void QueueForward()
    {
        // Function to move the queue Forward
        lastQueueEntry = currentQueueEntry;
        currentQueueEntry += 1;
        if (currentQueueEntry > playList.Count ) { currentQueueEntry = playList.Count; }
        QueueToSpecificEntry(currentQueueEntry);
        return;
    }
    public void QueueForward10Seconds()
    {
        // Queue the timer forward ten seconds
        if (clockTotalSeconds != 0)
        {
            clockCurrentSeconds -= 10;
            clockToStamp -= 10;
            if (clockCurrentSeconds <= 0)
            {
                clockCurrentSeconds = 0;
            }
            UpdateClockText();
            queueTimeRemaining -= 10;
            UpdateQueueTimeRemaining();
        }
        return;
    }
    public void QueueBackward()
    {
        // Function to move the queue Backward
        lastQueueEntry = currentQueueEntry;
        currentQueueEntry -= 1;
        if (currentQueueEntry < 0) { currentQueueEntry = 0; }
        QueueToSpecificEntry(currentQueueEntry);
        return;
    }
    public void QueueBackward10Seconds()
    {
        // Queue the timer backward ten seconds
        if (clockTotalSeconds != 0)
        {
            clockCurrentSeconds += 10;
            clockToStamp += 10;
            if (clockCurrentSeconds >= clockTotalSeconds)
            {
                clockCurrentSeconds = clockTotalSeconds;
                clockToStamp = getTimeStamp() + clockTotalSeconds;
            }
            UpdateClockText();
            queueTimeRemaining += 10;
            UpdateQueueTimeRemaining();
        }
        return;
    }
    public void QueueToSpecificEntry( int entry)
    {
        // Function to go to a specific queue entry
        bool wasTrue = queueTimerRunning;

        if (entry > playList.Count - 1){ entry = playList.Count - 1; }
        if (wasTrue == true)
        {
            TogglePlayPause();
        }
        currentQueueEntry = entry;
        clockTimerRunning = false;
        clockCurrentSeconds = playList[currentQueueEntry].timerSeconds;
        clockTotalSeconds = playList[currentQueueEntry].timerSeconds;
        clockToStamp = getTimeStamp() + clockTotalSeconds;
        clockText.text = e.convertSecondsToClockString(((int)clockCurrentSeconds));
        UpdateVerticalScroll();
        CalculateQueueTimeRemaining();
        return;
    }
    public void UpdateVerticalScroll()
    {
        if( currentQueueEntry != 0)
        {
            scrollValue = (1.0f - ((float)(currentQueueEntry - 3) / (float)( playList.Count - 13 ) ));
        }
        else
        {
            scrollValue = 1.0f;
        }
        if (scrollValue < 0) { scrollValue = 0f; }
        if (scrollValue > 1) { scrollValue = 1f; }
        return;
    }
    public void UpdateProgressBar()
    {
        // method to updatethe progress bar to the correct value
        float sliderPercent = 0;
        if( clockCurrentSeconds > 0)
        {
            sliderPercent = (float)(clockCurrentSeconds / clockTotalSeconds);
        }
        countDownSlider.value = sliderPercent;
        return;
    }

    public void ProcessQueueEntry( int entry)
    {
        // Function to process a particular queue entry
        // If the entry has a horn at the beginning, then set it off
        // Play a beginning horn if needed
        if ( playList[entry].hasBeginHorn ) {
            // play horn
            PlayHorn(playList[currentQueueEntry].beginHornLength);
        }

        // If the entry has a timer, then set the current timer value
        if ( playList[entry].hasTimer ) {
            clockTotalSeconds = playList[entry].timerSeconds;
            clockToStamp = getTimeStamp() + playList[entry].timerSeconds;
            clockCurrentSeconds = playList[entry].timerSeconds;
            clockTimerRunning = true;
            clockText.text = e.convertSecondsToClockString(Convert.ToInt32(Math.Ceiling(clockCurrentSeconds)));
        }

        // If the entry has some speech, then call the speech synthesizer
        if ( playList[entry].spokenText != "" ) {
            StartCoroutine( SpeakMainQueueText( playList[entry].spokenText, playList[entry].spokenPreDelay, playList[entry].spokenPostDelay ) );
        }
        SaveQueueState();
        // If it doesn't have a timer, then deduct the estimated seconds from teh queue time remaining
        if( playList[entry].hasTimer == false)
        {
            queueTimeRemaining -= playList[entry].estimatedSeconds;
        }
        return;

    }

    IEnumerator SpeakMainQueueText( string textToSpeak, double preTime, double postTime )
    {
        e.audioSource.volume = 1.0f;
        yield return new WaitForSecondsRealtime( (float)preTime );
        Speaker.Instance.Speak(textToSpeak, e.audioSource, voice, true);
        yield return new WaitForSecondsRealtime( (float)postTime );
    }

    public void SaveQueueState()
    {
        // Let us save the state of the queue
        PlayerPrefs.SetInt("queueEventID", e.eventID );
        PlayerPrefs.SetInt("queueTimerRunning", queueTimerRunning ? 1 : 0);
        PlayerPrefs.SetInt("currentQueueEntry", currentQueueEntry );
        PlayerPrefs.SetInt("clockTimerRunning", clockTimerRunning ? 1 : 0);
        PlayerPrefs.SetFloat("clockCurrentSeconds", (float)clockCurrentSeconds);
        PlayerPrefs.SetFloat("clockTotalSeconds", (float)clockTotalSeconds);
        PlayerPrefs.SetFloat("clockToStamp", (float)clockToStamp );
        return;
    }
    public void LoadQueueState()
    {
        // Function to load the queue state back to the saved value
        // but only if we have come back to the same event
        int queueEventID = PlayerPrefs.GetInt("queueEventID");

        if( queueEventID != e.eventID) { return; }

        // If we got here, then lets load up the event parameters and set them
        clockToStamp = (double)PlayerPrefs.GetFloat("clockToStamp");
        clockTotalSeconds = (double)PlayerPrefs.GetFloat("clockTotalSeconds");
        clockCurrentSeconds = (double)PlayerPrefs.GetFloat("clockCurrentSeconds");
        currentQueueEntry = PlayerPrefs.GetInt("currentQueueEntry");
        queueTimerRunning = PlayerPrefs.GetInt("queueTimerRunning") == 1 ? true : false;
        clockTimerRunning = PlayerPrefs.GetInt("clockTimerRunning") == 1 ? true : false;

        if(queueTimerRunning == true)
        {
            // Set the play button to the correct sprite
            e.createPlayListButton.enabled = false;
            e.createPlayListButton.interactable = false;
            // Set the voice
            voice = Speaker.Instance.VoiceForName(e.prefs["voice"]);
            // Change sprite to pause image
            playButton.image.sprite = pauseImage;
            lastQueueEntry = 0;
            if (playList[currentQueueEntry].hasTimer == false)
            {
                ProcessQueueEntry(currentQueueEntry);
            }
            UpdateVerticalScroll();
            playListScroll.verticalNormalizedPosition = scrollValue;
        }
        if (clockTimerRunning == true)
        {
            double diff = clockToStamp - getTimeStamp();
            if( diff < 0)
            {
                clockCurrentSeconds = 0;
            }
            else
            {
                clockCurrentSeconds = diff;
            }
        }
        if ( clockCurrentSeconds > 0)
        {
            int currentSeconds = Convert.ToInt32(Math.Ceiling(clockCurrentSeconds));
            clockText.text = e.convertSecondsToClockString(currentSeconds);
            UpdateProgressBar();
        }
        return;
    }
    public void PlayHorn( int length )
    {
        AudioClip clip;
        e.audioSource.volume = 1.0f;

        if ( length == 0)
        {
            return;
        }
        clip = e.airHorn1;

        // Method to play a particular horn
        switch( e.prefsHorn.options[e.prefsHorn.value].text )
        {
            case "Air Horn":
                if (length == 1) { clip = e.airHorn1; }
                if (length == 2) { clip = e.airHorn2; }
                if (length == 3) { clip = e.airHorn3; }
                break;
            case "Ship Horn":
                if (length == 1) { clip = e.shipHorn1; }
                if (length == 2) { clip = e.shipHorn2; }
                if (length == 3) { clip = e.shipHorn3; }
                break;
            case "Sports Arena":
                if (length == 1) { clip = e.sportArenaHorn1; }
                if (length == 2) { clip = e.sportArenaHorn2; }
                if (length == 3) { clip = e.sportArenaHorn3; }
                break;
            case "Inception Horn":
                if (length == 1) { clip = e.inceptionHorn1; }
                if (length == 2) { clip = e.inceptionHorn2; }
                if (length == 3) { clip = e.inceptionHorn3; }
                break;
            case "Epic Horn":
                if (length == 1) { clip = e.epicHorn1; }
                if (length == 2) { clip = e.epicHorn2; }
                if (length == 3) { clip = e.epicHorn3; }
                break;
            default:
                if (length == 1) { clip = e.airHorn1; }
                if (length == 2) { clip = e.airHorn2; }
                if (length == 3) { clip = e.airHorn3; }
                break;
        }
        // Now play the horn asynchronously
        e.audioSource.PlayOneShot(clip);
        return;
    }

    public void OnSpeakCompleted()
    {
        if(noSpeakEndEvent == true) { noSpeakEndEvent = false; return; }
        // This is the listener for when some text speech has finished
        if (playList.Count > 0)
        {
            if (playList[currentQueueEntry].hasTimer == false)
            {
                if (currentQueueEntry == playList.Count - 1)
                {
                    // Reached the end of the playlist, so stop everything
                    queueTimerRunning = false;
                    clockTimerRunning = false;
                    Speaker.Instance.Silence();
                    e.audioSource.Stop();
                    goToNextQueueEntry = false;
                }
                else
                {
                    lastQueueEntry = currentQueueEntry;
                    currentQueueEntry += 1;
                    goToNextQueueEntry = true;
                }
            }
        }
        return;
    }

    public void UpdateClockText()
    {
        int currentSeconds = Convert.ToInt32( Math.Ceiling(clockCurrentSeconds) );

        clockText.text = e.convertSecondsToClockString( currentSeconds );
        UpdateProgressBar();

        // Now lets see if we need to announce the time remaining
        if( clockOldSeconds != currentSeconds)
        {
            // Set a boolean on whether or not to announce the time
            bool speak = false;

            if (playList[currentQueueEntry].timerLastTen && currentSeconds <= 10) { speak = true; }
            if (playList[currentQueueEntry].timerLastTwenty && currentSeconds <= 20) { speak = true; }
            if (playList[currentQueueEntry].timerLastThirty && currentSeconds <= 30) { speak = true; }
            if (playList[currentQueueEntry].timerEveryFifteen &&
                currentSeconds %15 == 0 &&
                currentSeconds >= 60 &&
                currentSeconds <= 180 ) { speak = true; }
            if (playList[currentQueueEntry].timerEveryFifteen &&
                currentSeconds % 15 == 0 &&
                !playList[currentQueueEntry].timerEveryTenInLastMinute &&
                currentSeconds <= 60 ) { speak = true; }
            if (playList[currentQueueEntry].timerEveryMinute && currentSeconds % 60 == 0) { speak = true; }
            if (playList[currentQueueEntry].timerEveryTenInLastMinute && currentSeconds % 10 == 0 && currentSeconds <= 60) { speak = true; }
            if (playList[currentQueueEntry].timerEveryFiveInLastMinute && currentSeconds % 5 == 0 && currentSeconds <= 60) { speak = true; }
            if (playList[currentQueueEntry].timerEveryThirty && currentSeconds % 30 == 0) { speak = true; }
            if (currentSeconds == Convert.ToInt32( clockTotalSeconds )) { speak = false; }
            if (currentSeconds == 0 ) { speak = false; }

            if (e.prefs["useNoFly"] == "1" && ( currentSeconds == 80 || currentSeconds == 70 || currentSeconds == 65) && playList[currentQueueEntry].entryType == "PrepTime") { speak = true; }
            if (e.prefs["announcePilotsNextRound"] == "1" && currentSeconds == 205 && playList[currentQueueEntry].entryType == "Window") { speak = true; }
            if (e.prefs["announcePilotsNextRound"] == "1" && currentSeconds == 170 && playList[currentQueueEntry].entryType == "Window" && playList[currentQueueEntry].isAllUp == true) { speak = true; }

            // Now speak if needed
            if ( speak == true)
            {
                string speakText = "";
                e.audioSource.volume = 1.0f;
                if (currentSeconds >= 60)
                {
                    int min = Convert.ToInt32(currentSeconds / 60);
                    int sec = Convert.ToInt32(currentSeconds % 60);
                    if (sec == 0)
                    {
                        speakText = min.ToString() + " minute";
                        if (min > 1)
                        {
                            speakText += "s";
                        }
                        if( min == 1)
                        {
                            speakText = min.ToString() + "minnit";
                        }
                    }
                    else
                    {
                        speakText = min.ToString() + " " + sec.ToString();
                    }
                    if (playList[currentQueueEntry].isAllUp && ( currentSeconds == 180 || currentSeconds %30 != 0 ) )
                    {
                        // We don't want to speak the minutes, because the window time is 183 seconds and we don't want them talking over eachother
                        speakText = "";
                    }
                }
                else
                {
                    speakText = currentSeconds.ToString();
                    Boolean addSeconds = false;
                    if( currentSeconds >= 30 && playList[currentQueueEntry].timerEveryTenInLastMinute && !playList[currentQueueEntry].timerLastThirty) { addSeconds = true; }
                    if( currentSeconds >= 21 && ! playList[currentQueueEntry].timerLastThirty ) { addSeconds = true; }
                    if( currentSeconds == 20 && ! playList[currentQueueEntry].timerLastTwenty && !playList[currentQueueEntry].timerLastThirty) { addSeconds = true; }
                    if( addSeconds )
                    {
                        speakText += " seconds";
                    }
                }
                if ( (playList[currentQueueEntry].spokenTextOnCountdown != null || playList[currentQueueEntry].spokenTextOnCountdown != null ) && currentSeconds >= 15)
                {
                    if (playList[currentQueueEntry].entryType != "PrepTime")
                    {
                        speakText += " " + playList[currentQueueEntry].spokenTextOnCountdown;
                    }
                    else if(currentSeconds % 30 == 0)
                    {
                        speakText += " " + playList[currentQueueEntry].spokenTextOnCountdown;
                    }
                }
                if (playList[currentQueueEntry].entryType == "PrepTime")
                {
                    if (currentSeconds == 60 && e.prefs["useNoFly"] == "1")
                    {
                        speakText += ". - All pilots must not be flying during this final minnit.";
                    }
                    if (e.prefs["useNoFly"] == "1" && currentSeconds == 80)
                    {
                        speakText = "All pilots must start landing from test flights.";
                    }
                    if (e.prefs["useNoFly"] == "1" && currentSeconds == 70)
                    {
                        speakText = "Ten seconds till no fly time.";
                    }
                    if (e.prefs["useNoFly"] == "1" && currentSeconds == 65)
                    {
                        speakText = "Five seconds till no fly time.";
                    }
                }
                if (e.prefs["announcePilotsNextRound"] == "1" && currentSeconds == 205 && playList[currentQueueEntry].entryType == "Window") {
                    List<string> nextGroup = e.getNextGroup(playList[currentQueueEntry].round_number, playList[currentQueueEntry].group);
                    if (Convert.ToInt32(nextGroup[0]) <= e.eventNumRounds.Count)
                    {
                        speakText = "Will the following pilots get ready for the next group. Round " + nextGroup[0] + " Group " + nextGroup[1] + ", " + String.Join(", ", e.getPilotList(Convert.ToInt32(nextGroup[0]), nextGroup[1]));
                    }
                }
                // Let us figure out if this is an all up round and speak the next pilots in the last flight
                if ( playList[currentQueueEntry].isAllUp == true )
                {
                    // Determine what the particular all up task is so we can figure out the last flight number
                    EventTask taskInfo = e.getTask(playList[currentQueueEntry].round_number);
                    int flights = 3;
                    if (taskInfo.flight_type_code == "f3k_c2") { flights = 4; }
                    if (taskInfo.flight_type_code == "f3k_c3") { flights = 5; }
                    if (e.prefs["announcePilotsNextRound"] == "1" && currentSeconds == 170 && playList[currentQueueEntry].entryType == "Window" && playList[currentQueueEntry].allUpFlight == flights)
                    {
                        List<string> nextGroup = e.getNextGroup(playList[currentQueueEntry].round_number, playList[currentQueueEntry].group);
                        if (Convert.ToInt32(nextGroup[0]) <= e.eventNumRounds.Count)
                        {
                            speakText = "Will the following pilots get ready for the next group. Round " + nextGroup[0] + " Group " + nextGroup[1] + ", " + String.Join(", ", e.getPilotList(Convert.ToInt32(nextGroup[0]), nextGroup[1]));
                        }
                    }
                }

                // Now play the actual speech ( if there is any )
                if (speakText != "" && speakText != " " && speakText != null)
                {
                    Speaker.Instance.Speak(speakText, e.audioSource, voice, true);
                }
            }
            queueTimeRemaining -= 1;
            UpdateQueueTimeRemaining();
        }
        clockOldSeconds = currentSeconds;
        return;
    }

    public void VoicePreLoad()
    {
        // Turn the audio volume down
        e.audioSource.volume = 1.0f;

        // Subscribe to new voice callback function just for loading voices
        Speaker.Instance.OnSpeakAudioGenerationComplete += onAudioGenerationComplete;

        // Bring up popup for loading voices
        loadingVoicePopup.Show();
        loadingVoicePopup.loadingMessage.text = "Loading Voice Phrases";
        loadingVoicePopup.loadedPhrases = 0;

        // Lets clear the cache first
        globalCache.ClearCache();

        // Lets clear the phrases
        preloadPhrases.Clear();

        // Load the base number timing phrases
        SetPhraseList();

        // Now lets add the other phrases from the playlist
        if (playList != null)
        {
            // Let's step through all of the playlist entries and add the speech to the cache
            foreach (PlayQueueEntry queue in playList)
            {
                if (queue.spokenText != null || queue.spokenText != "")
                {
                    if (!preloadPhrases.Contains(queue.spokenText))
                    {
                        preloadPhrases.Add(queue.spokenText);
                    }
                }
            }
        }

        // Set teh total phrases to be extpected to load
        phrasesToLoad = preloadPhrases.Count;

        // Now lets send the first ten phrases to the tts engine
        int x;
        for( x = 0; x <= 10; x++)
        {
            Speaker.Instance.Speak( preloadPhrases[x], e.audioSource, voice, false );
        }
        return;
    }
    private void onAudioGenerationComplete(Crosstales.RTVoice.Model.Wrapper wrapper)
    {
        // Routine to update the number of preloaded cache entries after each one
        loadingVoicePopup.loadedPhrases++;
        loadingVoicePopup.loadingMessage.text = "Loading " + loadingVoicePopup.loadedPhrases + " of " + phrasesToLoad.ToString() + " Phrases";

        if (loadingVoicePopup.loadedPhrases == phrasesToLoad)
        {
            loadingVoicePopup.Hide();
            Speaker.Instance.OnSpeakAudioGenerationComplete -= onAudioGenerationComplete;
            e.preloadButton.image.color = Color.green;
            e.preloadButtonText.text = "Voice Phrases Cached";
            e.audioSource.volume = 1.0f;
        }
        else
        {
            if ( loadingVoicePopup.loadedPhrases %10 == 0)
            {
                // trigger loading the next 10 phrases
                int from = loadingVoicePopup.loadedPhrases + 1;
                int to;
                if( from + 10 < phrasesToLoad )
                {
                    to = loadingVoicePopup.loadedPhrases + 10;
                }
                else
                {
                    to = phrasesToLoad - 1;
                }

                int x;
                for ( x = from; x <= to; x++ ){
                    Speaker.Instance.Speak( preloadPhrases[x], e.audioSource, voice, false );
                }
            }
        }
        return;
    }
    private void SetPhraseList()
    {
        preloadPhrases.Add("15 minutes");
        preloadPhrases.Add("14 30");
        preloadPhrases.Add("14 minutes");
        preloadPhrases.Add("13 30");
        preloadPhrases.Add("13 minutes");
        preloadPhrases.Add("12 30");
        preloadPhrases.Add("12 minutes");
        preloadPhrases.Add("11 30");
        preloadPhrases.Add("11 minutes");
        preloadPhrases.Add("10 30");
        preloadPhrases.Add("10 minutes");
        preloadPhrases.Add("9 30");
        preloadPhrases.Add("9 minutes");
        preloadPhrases.Add("8 30");
        preloadPhrases.Add("8 minutes");
        preloadPhrases.Add("7 30");
        preloadPhrases.Add("7 minutes");
        preloadPhrases.Add("6 30");
        preloadPhrases.Add("6 minutes");
        preloadPhrases.Add("5 30");
        preloadPhrases.Add("5 minutes");
        preloadPhrases.Add("4 30");
        preloadPhrases.Add("4 minutes");
        preloadPhrases.Add("3 30");
        preloadPhrases.Add("3 minutes");
        preloadPhrases.Add("2 45");
        preloadPhrases.Add("2 30");
        preloadPhrases.Add("2 15");
        preloadPhrases.Add("2 minutes");
        preloadPhrases.Add("1 45");
        preloadPhrases.Add("1 30");
        preloadPhrases.Add("1 15");
        preloadPhrases.Add("1 minute");
        preloadPhrases.Add("1 minnit");
        preloadPhrases.Add("55");
        preloadPhrases.Add("50");
        preloadPhrases.Add("50 seconds");
        preloadPhrases.Add("45");
        preloadPhrases.Add("40");
        preloadPhrases.Add("40 seconds");
        preloadPhrases.Add("35");
        preloadPhrases.Add("30");
        preloadPhrases.Add("30 seconds");
        preloadPhrases.Add("20 seconds");
        preloadPhrases.Add("15 seconds");
        preloadPhrases.Add("29");
        preloadPhrases.Add("28");
        preloadPhrases.Add("27");
        preloadPhrases.Add("26");
        preloadPhrases.Add("25");
        preloadPhrases.Add("24");
        preloadPhrases.Add("23");
        preloadPhrases.Add("22");
        preloadPhrases.Add("21");
        preloadPhrases.Add("20");
        preloadPhrases.Add("19");
        preloadPhrases.Add("18");
        preloadPhrases.Add("17");
        preloadPhrases.Add("16");
        preloadPhrases.Add("15");
        preloadPhrases.Add("14");
        preloadPhrases.Add("13");
        preloadPhrases.Add("12");
        preloadPhrases.Add("11");
        preloadPhrases.Add("10");
        preloadPhrases.Add("9");
        preloadPhrases.Add("8");
        preloadPhrases.Add("7");
        preloadPhrases.Add("6");
        preloadPhrases.Add("5");
        preloadPhrases.Add("4");
        preloadPhrases.Add("3");
        preloadPhrases.Add("2");
        preloadPhrases.Add("1");
        return;
    }
    // Routines to calculate the time remaining in event
    public void CalculateQueueTimeRemaining()
    {
        // calculate the time remaining in hours and minutes from the current queue entry to the selected round
        if (playList != null)
        {
            int roundTo = e.calcToDropDown.value + 1;
            queueTimeRemaining = 0;
            int entryIndex = 0;
            foreach (PlayQueueEntry entry in playList)
            {
                if (entryIndex >= currentQueueEntry && entry.round_number <= roundTo )
                {
                    queueTimeRemaining += entry.estimatedSeconds;
                }
                entryIndex++;
            }
            UpdateQueueTimeRemaining();
        }
        return;
    }
    public void UpdateQueueTimeRemaining()
    {
        // turn the total seconds into hours and minutes
        int hours = queueTimeRemaining / 3600;
        int minutes = (queueTimeRemaining % 3600) / 60;
        string numhrs = hours > 1 ? " hrs " : " hr ";
        timeRemaining.text = hours.ToString() + numhrs + minutes.ToString() + " min";

        // Now update what time that will be
        endTime.text = DateTime.Now.AddSeconds((double)queueTimeRemaining).ToString("hh:mm tt").ToLower();
    }
    // Method to get a timestamp from 2020-01-01 in milliseconds
    public double getTimeStamp()
    {
        double returnStamp = ((DateTime.Now - new DateTime(2020, 1, 1)).TotalSeconds);
        return returnStamp;
    }
    private void OnDestroy()
    {
        // Save the queue state when they go away from the screen
        SaveQueueState();
        return;
    }
    private void onDisable()
    {
        // Save the queue state when they go away from the screen
        SaveQueueState();
        return;
    }

    // Update is called once per frame
    private void Update()
    {
        // If there isn't a playlist built yet, then don't do anything
        if( playList == null) { return; }

        // If the player is at the end of the playlist
        if (playList.Count > 0 && currentQueueEntry > playList.Count - 1)
        {
            currentQueueEntry = playList.Count - 1;
            lastQueueEntry = currentQueueEntry;
        }
        //Set the play icon to active in the current entry by turning off the old one and then activating the new one
        if (playList.Count > 0) {
            if (currentQueueEntry != lastQueueEntry && lastQueueEntry != -1 )
            {
                // Update the play symbols on the entries
                // Lets update the queueEntry image if we are in that queueEntry
                Transform playListObject = e.queueListObject.GetChild(currentQueueEntry).GetChild(2);
                Image playImage = playListObject.GetComponent<Image>();
                playImage.enabled = true;
                if (lastQueueEntry >= 0)
                {
                    // Turn off the image of the queue list entry before the current one
                    Transform playListObjectOld = e.queueListObject.GetChild(lastQueueEntry).GetChild(2);
                    Image playImageOld = playListObjectOld.GetComponent<Image>();
                    playImageOld.enabled = false;
                }
            }
        }

        // Check to see if there is a playList, and enable or disable the buttons
        if (playList.Count == 0)
        {
            // Make all the buttons not interactable
            playButton.interactable = false;
            forwardButton.interactable = false;
            forward10Button.interactable = false;
            backwardButton.interactable = false;
            backward10Button.interactable = false;
        }
        else
        {
            playButton.interactable = true;
            forwardButton.interactable = true;
            backwardButton.interactable = true;
            forward10Button.interactable = false;
            backward10Button.interactable = false;
        }

        // This is the main timing loop, so execute things if we need to
        if ( queueTimerRunning == true)
        {
            if ( clockTimerRunning == true)
            {
                // Enable the 10 forward and backward buttons
                forward10Button.interactable = true;
                backward10Button.interactable = true;

                // Update the clock countdown
                if ( clockCurrentSeconds > 0)
                {
                    clockCurrentSeconds = clockToStamp - getTimeStamp();
                    UpdateClockText();
                }
                if( clockCurrentSeconds <= 0)
                {
                    clockCurrentSeconds = 0;
                    clockText.text = "0:00";
                    if( playList[currentQueueEntry].hasEndHorn == true)
                    {
                        // Play an end horn
                        PlayHorn(playList[currentQueueEntry].endHornLength);
                    }
                    clockTimerRunning = false;
                    lastQueueEntry = currentQueueEntry;
                    currentQueueEntry += 1;
                    goToNextQueueEntry = true;
                }
            }
            else
            {
                if( goToNextQueueEntry == true)
                {
                    goToNextQueueEntry = false;
                    ProcessQueueEntry(currentQueueEntry);
                    UpdateVerticalScroll();
                }
            }
        }
        // autoscroll for queue scroll value if timer queue running
        if (queueTimerRunning)
        {
            playListScroll.verticalNormalizedPosition = Mathf.MoveTowards(playListScroll.verticalNormalizedPosition, scrollValue, 0.0004f);
        }
        return;
    }
}
