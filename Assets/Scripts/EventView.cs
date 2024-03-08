using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Crosstales.RTVoice;
using Crosstales.RTVoice.Model;
using PetrushevskiApps.Utilities; // Internet connectivity utility
using static UnityEngine.EventSystems.EventTrigger;

public class EventView : MonoBehaviour
{
    public Text softwareVersion;
    public int eventID;
    public Dictionary<String, String> prefs = new Dictionary<string, string>();
    public List<int> eventNumRounds = new List<int>();
    public List<EventRound> rounds = new List<EventRound>();
    public List<String> groups = new List<String>();
    public EventDetailResponse e;
    public ChangeScene scene;
    public Transform pilotListObject;
    public PilotListDisplay entryDisplayPrefab;
    public Transform drawListObject;
    public DrawDisplay drawDisplayPrefab;
    public DrawRoundDisplay drawRoundDisplayPrefab;
    public DrawDisplaySpacer drawRoundDisplaySpacerPrefab;
    public int numberDrawRoundDisplayEntries;
    public Text eventTitle;
    public List<Voice> voices;
    public Dropdown prefsHorn;
    public Dropdown prefsCulture;
    public Dropdown prefsVoice;
    public Toggle prefsAnnouncePilots;
    public Dropdown prefsPrepTime;
    public Dropdown prefsBetweenGroups;
    public Dropdown prefsBetweenRounds;
    public Toggle prefsUseLanding;
    public Toggle prefsUse45Test;
    public Toggle prefsUseOneMinuteNoFly;
    public Toggle prefsAnnouncePilotsNextRound;
    public Toggle prefsAnnounceTasks;
    public Toggle prefsAnnounceScoreReminders;
    public List<PlayQueueEntry> playList = new List<PlayQueueEntry>();
    public Transform queueListObject;
    public QueueListDisplayRound queueListDisplayRoundPrefab;
    public QueueListDisplay queueListDisplayPrefab;
    public FlightDescriptions flightDescriptions = new FlightDescriptions();
    public InputField pilotMeetingTime;
    public InputField contestStartTime;
    public QueueControl queueControl;
    public Button createPlayListButton;
    public Dropdown calcToDropDown;
    public Button preloadButton;
    public Text preloadButtonText;
    public Button internetButton;
    public int internetConnected;
    public Button callOutReminderButton;
    private int insult;

    // Sound variables
    public AudioSource audioSource;
    public AudioClip airHorn0;
    public AudioClip airHorn1;
    public AudioClip airHorn2;
    public AudioClip airHorn3;
    public AudioClip shipHorn0;
    public AudioClip shipHorn1;
    public AudioClip shipHorn2;
    public AudioClip shipHorn3;
    public AudioClip sportArenaHorn0;
    public AudioClip sportArenaHorn1;
    public AudioClip sportArenaHorn2;
    public AudioClip sportArenaHorn3;
    public AudioClip inceptionHorn0;
    public AudioClip inceptionHorn1;
    public AudioClip inceptionHorn2;
    public AudioClip inceptionHorn3;
    public AudioClip epicHorn0;
    public AudioClip epicHorn1;
    public AudioClip epicHorn2;
    public AudioClip epicHorn3;
    public AudioClip moo0;
    public AudioClip moo1;
    public AudioClip moo2;
    public AudioClip moo3;

    private void Awake()
    {
        // Create connectivity manager listener
        ConnectivityManager.Instance.AddConnectivityListener(OnConnectivityChange);
        connectionCheck();
    }

    // Start is called before the first frame update
    void Start()
    {
        softwareVersion.text = new SoftwareVersion().versionString;

        // Add listener for when the value of the Language Dropdown changes, to take action
        prefsCulture.onValueChanged.AddListener(delegate { LanguageDropDownItemSelected(prefsCulture); });

        if (audioSource == null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
        }

        // Load the saved event from the prefs
        this.eventID = PlayerPrefs.GetInt("eventID", 0);
        this.LoadEvent();
        this.ListPilots();
        this.InitializePrefs();
        // If there is a saved value for this event, then lets go ahead and build the playlist
        if( eventID != 0) {
            CreatePlayList();
        }
        insult = 0;
    }

    public void LoadEvent()
    {
        if (eventID != 0)
        {
            vaultAPI api = new vaultAPI();
            e = api.EventGetDetail(eventID);
            eventTitle.text = e.eventInfo.event_name;
            getRounds();
            if( rounds.Count > 0)
            {
                ListDraw();
            }

        }
        return;

    }
    public void ListPilots()
    {
        // Destroy any existing child lines before creating new ones
        foreach (Transform child in pilotListObject)
        {
            GameObject.Destroy(child.gameObject);
        }
        // Now lets add the pilot lines
        foreach (EventPilot pilot in e.eventInfo.pilots)
        {
            PilotListDisplay display = (PilotListDisplay)Instantiate(this.entryDisplayPrefab);
            display.transform.SetParent(pilotListObject, false);
            display.Prime(pilot);
        }
        return;
    }
    public void SortPilotsByName()
    {
        Array.Sort(e.eventInfo.pilots, new PilotComparer());
        ListPilots();
        return;
    }
    public void SortPilotsByBib()
    {
        Array.Sort(e.eventInfo.pilots, new PilotBibComparer());
        ListPilots();
        return;
    }
    public void ListDraw()
    {
        numberDrawRoundDisplayEntries = 0;
        // Destroy any existing child lines before creating new ones
        foreach (Transform child in drawListObject)
        {
            GameObject.Destroy(child.gameObject);
        }
        // Now lets add the pilot lines
        foreach (EventRound round in rounds)
        {
            string oldgroup = "old";
            foreach(EventRoundFlight flight in round.flights)
            {
                if (oldgroup != "" && flight.group != oldgroup)
                {
                    // This is the start of a new group, so put a round and group header in place
                    DrawRoundDisplay displayRound = (DrawRoundDisplay)Instantiate(this.drawRoundDisplayPrefab);
                    displayRound.transform.SetParent(drawListObject, false);
                    displayRound.Prime(round, flight);
                    numberDrawRoundDisplayEntries++;
                }
                DrawDisplay displayFlight = (DrawDisplay)Instantiate(this.drawDisplayPrefab);
                displayFlight.transform.SetParent(drawListObject, false);
                displayFlight.Prime(round, flight);
                numberDrawRoundDisplayEntries++;
                oldgroup = flight.group;
            }
        }
        return;
    }

    // DropDown List Actions
    public void LanguageDropDownItemSelected(Dropdown dropdown)
    {
        this.PrefsVoiceDropDown();
        return;
    }

    // Test a selected voice with a text string
    public void VoiceTest()
    {
        queueControl.noSpeakEndEvent = true;
        Speaker.Instance.Speak("30 seconds before a 10 minute working window.", audioSource,Speaker.Instance.VoiceForName(prefs["voice"]), true);
        return;
    }
    public void SpeakAllPilotNames()
    {
        // Speak all of the pilot names so you can see if there are any that need to be changed
        String pilotString = "";
        foreach (EventPilot pilot in e.eventInfo.pilots)
        {
            String pilotName = pilot.pilot_first_name + " " + pilot.pilot_last_name;
            pilotString += PlayerPrefs.GetString(pilotName, pilotName) + ", ";
        }
        queueControl.noSpeakEndEvent = true;
        Speaker.Instance.Speak(pilotString, audioSource, Speaker.Instance.VoiceForName(prefs["voice"]), true);
        return;
    }
    public void MikeSmithRuleAnnounce()
    {
        List<String> msRuleStrings = new List<String>();
        msRuleStrings.Add("This pilot has a history of something going wrong at the last minnit.");
        msRuleStrings.Add("Rumor has it that this pilot spotted the lead singer of Spandau Ballet and needed to get an autograph.");
        msRuleStrings.Add("Some of the parts of this pilots plane must have fallen off.");
        msRuleStrings.Add("Does anyone have any C A?");
        msRuleStrings.Add("The guy that he paid 20 bucks to build his plane swore it went on that way.");
        msRuleStrings.Add("Once the body goes, the mind follows.");
        msRuleStrings.Add("Somebody got his fly stuck.");
        msRuleStrings.Add("He must have lost the batteries in his hearing aid.");
        msRuleStrings.Add("He's finishing up the last box of girl scout cookies.");
        msRuleStrings.Add("You just can't fix stupid.");
        msRuleStrings.Add("Drinking 3 red bulls has made him a little shakey.");
        msRuleStrings.Add("Remember kiddos, always bring a back up.");
        msRuleStrings.Add("The peanut butter sandwich is sticking to the roof of his mouth.");
        msRuleStrings.Add("He looks smarter in pictures.");
        msRuleStrings.Add("His family tree didn't have enough branches.");
        msRuleStrings.Add("Evolution is not always a good thing.");
        msRuleStrings.Add("There really is one born every minnit.");
        msRuleStrings.Add("The gene pool needs more chlorine.");
        msRuleStrings.Add("Even Bob Ross would call this pilot a mistake.");
        msRuleStrings.Shuffle();

        if (queueControl.clockTimerRunning)
        {
            // Pause the clock
            queueControl.TogglePlayPause();
        }
        queueControl.noSpeakEndEvent = true;
        string insultString = msRuleStrings[insult];
        Speaker.Instance.Speak("Attention, the mike smith rule has been called. " + insultString + " Please hold while we remedy the situation.", audioSource, Speaker.Instance.VoiceForName(prefs["voice"]), true);
        insult++;
        if (insult >= msRuleStrings.Count) { msRuleStrings.Shuffle(); insult = 0; }
        return;
    }
    public void CallOutPilotReminders()
    {
        // Routine to call out any of the reminders for the pilot self scoring right this moment
        string speakText = "";
        int maxRound = playList[queueControl.currentQueueEntry].round_number - 1;
        if (maxRound < 0) { maxRound = 0; }
        if (queueControl.lastQueueEntry > 0 && maxRound == 0)
        {
            // Means we have reached the end of the contest, so lets make the call on everyone on all rounds
            maxRound = rounds.Count;
        }
        // If it is the end of a round, and the reminder to self score is activated, let us do it here
        if (queueControl.lastQueueEntry != -1 && internetConnected != 0)
        {
            LoadEvent();
            // let us look at rounds in the draw more than one round old where pilots have not entered scores
            List<string> nagList = getPilotReminderList(maxRound);
            if (nagList.Count > 0)
            {
                string insultString = queueControl.reminderStrings[insult];
                insult++;
                if (insult >= queueControl.reminderStrings.Count) { queueControl.reminderStrings.Shuffle(); insult = 0; }
                speakText = insultString;
                speakText += String.Join(", ", nagList);
                speakText += ". Thank you.";
                queueControl.noSpeakEndEvent = true;
                Speaker.Instance.Speak(speakText, audioSource, Speaker.Instance.VoiceForName(prefs["voice"]), true);
            }
        }
        return;
    }

    // Internet connectivity routine
    public void OnConnectivityChange(bool isConnected, string errorMsg)
    {
        connectionCheck();
    }
    public void connectionCheck()
    {
        // Method to do whatever is needed when connection is up or down
        if (ConnectivityManager.Instance.IsConnected)
        {
            // Make search button active and turn it green
            Debug.Log("Internet connection is up!");
            internetConnected = 1;
            internetButton.image.color = new Color(0.1595837f, 0.6981132f, 0.06256673f, 1f);
            callOutReminderButton.image.color = new Color(0.1595837f, 0.6981132f, 0.06256673f, 1f);
        }
        else
        {
            // Make search button inactive and turn it red
            Debug.Log("Internet connection is down!");
            internetConnected = 0;
            internetButton.image.color = new Color(0.7215686f, 0.1820968f, 0.1764706f, 1f);
            callOutReminderButton.image.color = new Color(0.7215686f, 0.1820968f, 0.1764706f, 1f);
        }
        return;
    }

    // Function to initialize all of the preference values and lists
    public void InitializePrefs()
    {
        this.LoadPrefs();
        return;
    }
    public void PrefsCultureDropDown()
    {
        LoadPrefs();
        string savedCulture = PlayerPrefs.GetString("prefsCulture", "en-US");
        prefsCulture.options.Clear();
        foreach (string culture in Speaker.Instance.Cultures)
        {
            prefsCulture.options.Add(new Dropdown.OptionData() { text = culture });
        }
        // Set the initial selection of the dropdown from the prefs
        prefsCulture.SetValueWithoutNotify( prefsCulture.options.FindIndex((i) => { return i.text.Equals(savedCulture); }));
        prefsCulture.RefreshShownValue();
        return;
    }
    public void PrefsVoiceDropDown()
    {
        LoadPrefs();
        string savedVoice = PlayerPrefs.GetString("prefsVoice", "Samantha");
        prefsVoice.options.Clear();
        // Let's get the voices that are associated with the selected language
        if (prefsCulture.value >= 0 && prefsCulture.options.Count > 0 )
        {
            foreach (Voice v in Speaker.Instance.VoicesForCulture(prefs["culture"]))
            {
                prefsVoice.options.Add(new Dropdown.OptionData() { text = v.Name });
            }
            // Set the initial selection of the dropdown from the prefs
            prefsVoice.SetValueWithoutNotify( prefsVoice.options.FindIndex((i) => { return i.text.Equals(savedVoice); }));
            prefsVoice.RefreshShownValue();
            SavePrefs();
            queueControl.voice = Speaker.Instance.VoiceForName(prefs["voice"]);
            //queueControl.VoicePreLoad();
        }
        return;
    }
    public void OnVoiceChange()
    {
        // routine to run when someone changesthe voice selection
        SavePrefs();
        queueControl.voice = Speaker.Instance.VoiceForName(prefs["voice"]);
        PreLoadButtonStatus(false);
        return;
    }
    public void CalcToDropDown()
    {
        // Set the calcto round dropdown list
        if( playList.Count > 0)
        {
            calcToDropDown.options.Clear();
            foreach( int round in eventNumRounds)
            {
                calcToDropDown.options.Add(new Dropdown.OptionData() { text = round.ToString() });
            }
            calcToDropDown.value = eventNumRounds.Count - 1;
            calcToDropDown.RefreshShownValue();
        }
        return;
    }
    void LoadPrefs()
    {
        // Load preferences into Dictionary array for easier access
        prefs["horn"] = PlayerPrefs.GetString("prefsHorn", "Ship Horn");
        prefs["culture"] = PlayerPrefs.GetString("prefsCulture", "en-US");
        prefs["voice"] = PlayerPrefs.GetString("prefsVoice", "Samantha");
        prefs["announcePilots"] = PlayerPrefs.GetString("prefsAnnouncePilots", "1");
        prefs["prepTime"] = PlayerPrefs.GetString("prefsPrepTime", "5 Minutes");
        prefs["betweenGroups"] = PlayerPrefs.GetString("prefsBetweenGroups", "None");
        prefs["betweenRounds"] = PlayerPrefs.GetString("prefsBetweenRounds", "None");
        prefs["useLanding"] = PlayerPrefs.GetString("prefsUseLanding", "1");
        prefs["use45Test"] = PlayerPrefs.GetString("prefsUse45Test", "0");
        prefs["useNoFly"] = PlayerPrefs.GetString("prefsUseNoFly", "0");
        prefs["announcePilotsNextRound"] = PlayerPrefs.GetString("prefsAnnouncePilotsNextRound", "0");
        prefs["announceTasks"] = PlayerPrefs.GetString("prefsAnnounceTasks", "0");
        prefs["announceScoreReminders"] = PlayerPrefs.GetString("prefsAnnounceScoreReminders", "0");

        // Now set the objects to their preferences values
        prefsHorn.SetValueWithoutNotify( prefsHorn.options.FindIndex((i) => { return i.text.Equals(prefs["horn"]); }) );
        prefsHorn.RefreshShownValue();

        prefsAnnouncePilots.SetIsOnWithoutNotify(prefs["announcePilots"] == "1" ? true : false);

        prefsPrepTime.SetValueWithoutNotify( prefsPrepTime.options.FindIndex((i) => { return i.text.Equals(prefs["prepTime"]); }) );
        prefsPrepTime.RefreshShownValue();

        prefsBetweenGroups.SetValueWithoutNotify(prefsBetweenGroups.options.FindIndex((i) => { return i.text.Equals(prefs["betweenGroups"]); }));
        prefsBetweenGroups.RefreshShownValue();
        prefsBetweenRounds.SetValueWithoutNotify( prefsBetweenRounds.options.FindIndex((i) => { return i.text.Equals(prefs["betweenRounds"]); }));
        prefsBetweenRounds.RefreshShownValue();

        prefsUseLanding.SetIsOnWithoutNotify(prefs["useLanding"] == "1" ? true : false);
        prefsUse45Test.SetIsOnWithoutNotify(prefs["use45Test"] == "1" ? true : false);
        prefsUseOneMinuteNoFly.SetIsOnWithoutNotify(prefs["useNoFly"] == "1" ? true : false);
        prefsAnnouncePilotsNextRound.SetIsOnWithoutNotify(prefs["announcePilotsNextRound"] == "1" ? true : false);
        prefsAnnounceTasks.SetIsOnWithoutNotify(prefs["announceTasks"] == "1" ? true : false);
        prefsAnnounceScoreReminders.SetIsOnWithoutNotify(prefs["announceScoreReminders"] == "1" ? true : false);
        if (queueControl.queueTimerRunning)
        {
            queueControl.voice = Speaker.Instance.VoiceForName(prefs["voice"]);
        }
        return;
    }
    public void SavePrefs()
    {
        // Save the preferences used for the audio stuff
        PlayerPrefs.SetString("prefsHorn", prefsHorn.options[prefsHorn.value].text);
        PlayerPrefs.SetString("prefsCulture", prefsCulture.options[prefsCulture.value].text);
        PlayerPrefs.SetString("prefsVoice", prefsVoice.options[prefsVoice.value].text);
        PlayerPrefs.SetString("prefsAnnouncePilots", prefsAnnouncePilots.isOn?"1":"0");
        PlayerPrefs.SetString("prefsPrepTime", prefsPrepTime.options[prefsPrepTime.value].text);
        PlayerPrefs.SetString("prefsBetweenGroups", prefsBetweenGroups.options[prefsBetweenGroups.value].text);
        PlayerPrefs.SetString("prefsBetweenRounds", prefsBetweenRounds.options[prefsBetweenRounds.value].text);
        PlayerPrefs.SetString("prefsUseLanding", prefsUseLanding.isOn ? "1" : "0");
        PlayerPrefs.SetString("prefsUse45Test", prefsUse45Test.isOn ? "1" : "0");
        PlayerPrefs.SetString("prefsUseNoFly", prefsUseOneMinuteNoFly.isOn ? "1" : "0");
        PlayerPrefs.SetString("prefsAnnouncePilotsNextRound", prefsAnnouncePilotsNextRound.isOn ? "1" : "0");
        PlayerPrefs.SetString("prefsAnnounceTasks", prefsAnnounceTasks.isOn ? "1" : "0");
        PlayerPrefs.SetString("prefsAnnounceScoreReminders", prefsAnnounceScoreReminders.isOn ? "1" : "0");
        PlayerPrefs.Save();
        LoadPrefs();
        return;
    }
    public void ReloadPrefs()
    {
        // reload the prefs by saving them and loading them again to reset the prefs dictionary
        SavePrefs();
        LoadPrefs();
        return;
    }
    // Create the playlist
    public void CreatePlayList()
    {
        // First, lets save the prefs
        getRounds();
        getGroups();

        // Now lets clear the list
        playList.Clear();

        int sequence = 1;
        int prepTimeMinutes = getPrepTime();
        int betweenGroupTimeSeconds = getBetweenGroupTime();
        int betweenRoundTimeSeconds = getBetweenRoundTime();
        bool announcePilots = prefs["announcePilots"] == "1" ? true : false;
        bool announceTasks = prefs["announceTasks"] == "1" ? true : false;
        bool use45Test = prefs["use45Test"] == "1" ? true : false;
        bool useNoFly = prefs["useNoFly"] == "1" ? true : false;
        bool useLanding = prefs["useLanding"] == "1" ? true : false;
        PlayQueueEntry tempEntry = new PlayQueueEntry();

        if ( e.eventInfo.event_type_code == "f3k" )
        {

            // Build the f3k audio play list
            foreach (EventRound round in rounds)
            {
                int loops = 1;
                if ( round.flights.Length == 0) { continue; }
                if (round.flights[0].flight_type_code == "f3k_c") { loops = 3; }
                if (round.flights[0].flight_type_code == "f3k_c2") { loops = 4; }
                if (round.flights[0].flight_type_code == "f3k_c3") { loops = 5; }
                // Set lastRound variable
                bool lastRound = round.round_number == rounds.Count ? true : false;
                EventTask taskInfo = getTask(round.round_number);
                int windowTime = taskInfo.event_task_time_choice;
                foreach (string group in getGroupsInRound(round.round_number))
                {
                    // Set lastGroup variable
                    bool lastGroup = groups.IndexOf(group) == groups.Count - 1 ? true : false;
                    // Get pilot list for this group if needed
                    List<String> pilotList = getPilotList(round.round_number, group);

                    // Skip this group if there is no flight for it in the draw
                    bool groupInRound = false;
                    foreach (EventRoundFlight flight in round.flights)
                    {
                        if (group == flight.group)
                        {
                            groupInRound = true;
                        }
                    }
                    if (!groupInRound) { continue; }

                    // Now lets start adding queue entries
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add Round and Group header
                    tempEntry = new PlayQueueEntry();
                    tempEntry.sequenceID = sequence;
                    tempEntry.round_number = round.round_number;
                    tempEntry.group = group;
                    tempEntry.entryType = "RoundAnnounce";
                    if (IsReflightGroup(round.round_number, group))
                    {
                        tempEntry.textDescription = "Round " + round.round_number.ToString() + " Reflight Group " + group;
                        tempEntry.spokenText = "Round " + round.round_number.ToString() + ", Reflight group " + group;
                    }
                    else
                    {
                        tempEntry.textDescription = "Round " + round.round_number.ToString() + " Group " + group;
                        tempEntry.spokenText = "Round " + round.round_number.ToString() + ", group " + group;
                    }
                    tempEntry.estimatedSeconds = 2;
                    if (announceTasks == false && announcePilots == false)
                    {
                        tempEntry.spokenText += ". Preparation Time of " + prepTimeMinutes.ToString() + " minutes starts in 5, 4, 3, 2, 1, ";
                        tempEntry.estimatedSeconds += 7;
                    }
                    if (round.round_number != 1)
                    {
                        tempEntry.spokenPreDelay = 2.0;
                    }
                    else
                    {
                        tempEntry.spokenPreDelay = 0.0;
                    }
                    tempEntry.spokenTextWait = true;
                    playList.Add(tempEntry);
                    sequence += 1;

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add task description entry
                    if (announceTasks == true)
                    {
                        tempEntry = new PlayQueueEntry();
                        tempEntry.sequenceID = sequence;
                        tempEntry.round_number = round.round_number;
                        tempEntry.group = group;
                        tempEntry.entryType = "Announce";
                        tempEntry.textDescription = taskInfo.flight_type_name;
                        tempEntry.spokenText = taskInfo.flight_type_description;
                        tempEntry.estimatedSeconds = 2;
                        // Add the prep time notice if they don't want to announce pilots
                        if (announcePilots == false || pilotList.Count == 0)
                        {
                            tempEntry.spokenText += ". Preparation Time of " + prepTimeMinutes.ToString() + " minutes starts in 5, 4, 3, 2, 1, ";
                            tempEntry.estimatedSeconds += 7;
                        }
                        tempEntry.spokenTextWait = true;
                        playList.Add(tempEntry);
                        sequence += 1;
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add pilot list entry if wanted
                    if (announcePilots == true && pilotList.Count > 0)
                    {
                        tempEntry = new PlayQueueEntry();
                        tempEntry.sequenceID = sequence;
                        tempEntry.round_number = round.round_number;
                        tempEntry.entryType = "Announce";
                        tempEntry.group = group;
                        tempEntry.textDescription = "Group " + group + " Pilot List";
                        tempEntry.spokenText = "Group " + group + " pilot list: " + String.Join(", ", pilotList) + ".";
                        tempEntry.spokenText += ". Preparation Time of " + prepTimeMinutes.ToString() + " minutes starts in 5, 4, 3, 2, 1, ";
                        tempEntry.spokenTextWait = true;
                        tempEntry.estimatedSeconds = 2 * pilotList.Count + 9;
                        playList.Add(tempEntry);
                        sequence += 1;
                    }

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Preparation time entry
                    tempEntry = new PlayQueueEntry();
                    tempEntry.sequenceID = sequence;
                    tempEntry.round_number = round.round_number;
                    tempEntry.group = group;
                    tempEntry.entryType = "PrepTime";
                    tempEntry.textDescription = convertSecondsToClockString(prepTimeMinutes * 60) + " Preparation Window";
                    tempEntry.spokenText = prepTimeMinutes.ToString() + " minutes remaining in prep time for Round " + round.round_number.ToString() + " Group " + group;
                    tempEntry.spokenTextWait = false;
                    tempEntry.spokenPreDelay = 2.0;
                    tempEntry.spokenTextOnCountdown = "";
                    if (use45Test || useNoFly)
                    {
                        if( use45Test )
                        {
                            tempEntry.spokenText += " . There is no flying allowed during this time.";
                            tempEntry.spokenTextOnCountdown += " of prep time before flight test window.";
                        }
                        else if( useNoFly )
                        {
                            tempEntry.spokenTextOnCountdown += " of prep time before no fly window.";
                        }
                        tempEntry.timerLastFive = true;
                        tempEntry.endHornLength = 0;
                    }
                    else
                    {
                        tempEntry.spokenTextOnCountdown = "before working window of Round " + round.round_number.ToString() + " Group " + group;
                        tempEntry.endHornLength = 3;
                        tempEntry.timerEveryTenInLastMinute = true;
                        tempEntry.timerLastTwenty = true;
                        tempEntry.timerEveryFifteen = true;
                    }
                    if( loops > 1 && !use45Test && !useNoFly)
                    {
                        tempEntry.spokenTextOnCountdown += ", Flight 1.";
                    }
                    tempEntry.hasTimer = true;
                    tempEntry.hasBeginHorn = true;
                    tempEntry.beginHornLength = 1;
                    tempEntry.timerSeconds = (prepTimeMinutes * 60);
                    tempEntry.timerEveryThirty = true;
                    tempEntry.hasEndHorn = true;
                    tempEntry.estimatedSeconds = (prepTimeMinutes * 60);
                    playList.Add(tempEntry);
                    sequence += 1;

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add 45s Test Time Entry
                    if (use45Test == true)
                    {
                        tempEntry = new PlayQueueEntry();
                        tempEntry.sequenceID = sequence;
                        tempEntry.round_number = round.round_number;
                        tempEntry.group = group.ToString();
                        tempEntry.entryType = "Testing";
                        tempEntry.textDescription = "45 Second Flight Test Window";
                        tempEntry.spokenText = "45 Second Flight Test Window. Pilots may now make test flights.";
                        tempEntry.spokenTextOnCountdown = " before no fly window";
                        tempEntry.estimatedSeconds = 45;
                        tempEntry.spokenTextWait = true;
                        tempEntry.spokenPreDelay = 1.0;
                        tempEntry.hasEndHorn = true;
                        tempEntry.hasTimer = true;
                        tempEntry.timerSeconds = 45;
                        tempEntry.timerEveryTenInLastMinute = true;
                        tempEntry.timerEveryFifteen = true;
                        tempEntry.timerLastFive = true;
                        tempEntry.hasEndHorn = true;
                        tempEntry.endHornLength = 0;
                        playList.Add(tempEntry);
                        sequence += 1;
                    }

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add One Minute NoFly Entry
                    if (useNoFly == true)
                    {
                        tempEntry = new PlayQueueEntry();
                        tempEntry.sequenceID = sequence;
                        tempEntry.round_number = round.round_number;
                        tempEntry.group = group.ToString();
                        tempEntry.entryType = "NoFly";
                        tempEntry.textDescription = "1:00 No Fly Window";
                        tempEntry.spokenText = "1 Minute no fly time before working window of Round " + round.round_number.ToString() + " Group " + group;
                        tempEntry.spokenTextOnCountdown = "before working window";
                        if (loops > 1)
                        {
                            tempEntry.spokenTextOnCountdown += ", Flight 1.";
                        }
                        tempEntry.spokenText += " . Pilots must not be flying during this final minnit.";

                        tempEntry.estimatedSeconds = 60;
                        tempEntry.spokenTextWait = true;
                        tempEntry.spokenPreDelay = 1.5;
                        tempEntry.hasEndHorn = true;
                        tempEntry.hasTimer = true;
                        tempEntry.timerSeconds = 60;
                        tempEntry.timerEveryFifteen = true;
                        tempEntry.timerEveryThirty = true;
                        tempEntry.timerEveryTenInLastMinute = true;
                        tempEntry.timerLastTwenty = true;
                        tempEntry.hasEndHorn = true;
                        tempEntry.endHornLength = 3;
                        playList.Add(tempEntry);
                        sequence += 1;
                    }

                    for (int loop = 1; loop <= loops; loop++)
                    {
                        if( loop > 1)
                        {
                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            // Add 1 minute in between flights windows (for f3k C tasks only
                            tempEntry = new PlayQueueEntry();
                            tempEntry.sequenceID = sequence;
                            tempEntry.round_number = round.round_number;
                            tempEntry.group = group;
                            tempEntry.entryType = "NoFly";
                            tempEntry.textDescription = "1:00 No Fly Window";
                            tempEntry.spokenText = "1 Minute no fly time before working window of Round " + round.round_number.ToString() + " Group " + group + ", Flight " + loop.ToString() + ".";
                            tempEntry.spokenPreDelay = 1.5;
                            tempEntry.spokenTextWait = false;
                            tempEntry.spokenTextOnCountdown = "before working window of Round " + round.round_number.ToString() + " Group " + group + ", Flight " + loop.ToString() + ".";
                            if( loop == 1)
                            {
                                // Only have beginning horn on the first one
                                tempEntry.hasBeginHorn = true;
                            }
                            tempEntry.beginHornLength = 1;
                            tempEntry.hasTimer = true;
                            tempEntry.timerSeconds = 60;
                            tempEntry.timerEveryTenInLastMinute = true;
                            tempEntry.timerLastTwenty = true;
                            tempEntry.hasEndHorn = true;
                            if (loops > 1)
                            {
                                tempEntry.endHornLength = 3;
                                tempEntry.isAllUp = true;
                                tempEntry.allUpFlight = loop;
                            }
                            else
                            {
                                tempEntry.endHornLength = 2;
                            }
                            tempEntry.estimatedSeconds = 60;
                            playList.Add(tempEntry);
                            sequence += 1;
                        }

                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Window time entry
                        tempEntry = new PlayQueueEntry();
                        tempEntry.sequenceID = sequence;
                        tempEntry.round_number = round.round_number;
                        tempEntry.entryType = "Window";
                        tempEntry.group = group;
                        if ( loops > 1)
                        {
                            tempEntry.textDescription = convertSecondsToClockString(windowTime) + " Flight " + loop.ToString() + " Window";
                            tempEntry.spokenText = (windowTime / 60).ToString() + " minute window for flight " + loop.ToString() + ".";
                        }
                        else
                        {
                            tempEntry.textDescription = convertSecondsToClockString(windowTime) + " Flight Window";
                            tempEntry.spokenText = (windowTime / 60).ToString() + " minnit working window.";
                        }
                        tempEntry.spokenPreDelay = 3.5;
                        tempEntry.spokenTextWait = false;
                        tempEntry.hasBeginHorn = false;
                        tempEntry.hasTimer = true;
                        tempEntry.timerSeconds = windowTime;
                        tempEntry.timerEveryMinute = true;
                        tempEntry.timerEveryThirty = true;
                        tempEntry.timerEveryFifteen = true;
                        tempEntry.timerLastTen = true;
                        tempEntry.timerEveryTenInLastMinute = true;
                        tempEntry.hasEndHorn = true;
                        tempEntry.endHornLength = 2;
                        if (loops > 1)
                        {
                            tempEntry.isAllUp = true;
                            tempEntry.allUpFlight = loop;
                        }
                        tempEntry.estimatedSeconds = windowTime;
                        playList.Add(tempEntry);
                        sequence += 1;

                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Add Landing window time if wanted
                        if (useLanding == true)
                        {
                            tempEntry = new PlayQueueEntry();
                            tempEntry.sequenceID = sequence;
                            tempEntry.round_number = round.round_number;
                            tempEntry.group = group;
                            tempEntry.entryType = "Landing";
                            tempEntry.textDescription = "30 Second Landing Window";
                            tempEntry.spokenText = "30 second landing window.";
                            tempEntry.spokenPreDelay = 2.0;
                            tempEntry.spokenTextWait = false;
                            tempEntry.spokenTextOnCountdown = "in landing window";
                            tempEntry.hasTimer = true;
                            tempEntry.timerSeconds = 30;
                            tempEntry.timerEveryTenInLastMinute = true;
                            tempEntry.timerLastFive = true;
                            tempEntry.hasEndHorn = true;
                            tempEntry.endHornLength = 1;
                            if (loops > 1)
                            {
                                tempEntry.isAllUp = true;
                                tempEntry.allUpFlight = loop;
                            }
                            tempEntry.estimatedSeconds = 30;
                            playList.Add(tempEntry);
                            sequence += 1;
                        }
                    }

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add group separation time if wanted
                    if (betweenGroupTimeSeconds > 0 && !(lastGroup && lastRound) && !(lastGroup && !lastRound))
                    {
                        String betweenString = "";
                        String betweenStringSpoken = "";
                        if (betweenGroupTimeSeconds < 60)
                        {
                            betweenString = betweenGroupTimeSeconds.ToString() + " Second";
                            betweenStringSpoken = betweenGroupTimeSeconds.ToString() + " Second";
                        }
                        else
                        {
                            betweenString = (betweenGroupTimeSeconds / 60).ToString() + " Minute";
                            betweenStringSpoken = (betweenGroupTimeSeconds / 60).ToString() + " minnit";
                        }
                        tempEntry = new PlayQueueEntry();
                        tempEntry.sequenceID = sequence;
                        tempEntry.round_number = round.round_number;
                        tempEntry.group = group;
                        tempEntry.entryType = "Wait";
                        tempEntry.textDescription = betweenString + " Group Separation Time";
                        tempEntry.spokenText = betweenStringSpoken + " Group Separation Time";
                        tempEntry.spokenPreDelay = 2.0;
                        tempEntry.spokenTextWait = false;
                        tempEntry.spokenTextOnCountdown = "Left in group separation";
                        tempEntry.hasTimer = true;
                        tempEntry.timerSeconds = betweenGroupTimeSeconds;
                        tempEntry.timerEveryThirty = true;
                        tempEntry.timerLastTen = false;
                        tempEntry.hasEndHorn = false;
                        tempEntry.estimatedSeconds = betweenGroupTimeSeconds;
                        playList.Add(tempEntry);
                        sequence += 1;
                    }

                }
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Add round separation time if wanted
                if (betweenRoundTimeSeconds > 0 && ! lastRound )
                {
                    String betweenString = "";
                    String betweenStringSpoken = "";
                    if (betweenRoundTimeSeconds < 60)
                    {
                        betweenString = betweenRoundTimeSeconds.ToString() + " Second";
                        betweenStringSpoken = betweenRoundTimeSeconds.ToString() + " Second";
                    }
                    else
                    {
                        betweenString = (betweenRoundTimeSeconds / 60).ToString() + " Minute";
                        betweenStringSpoken = (betweenRoundTimeSeconds / 60).ToString() + " minnit";
                    }
                    tempEntry = new PlayQueueEntry();
                    tempEntry.sequenceID = sequence;
                    tempEntry.round_number = round.round_number;
                    tempEntry.entryType = "Wait";
                    tempEntry.textDescription = betweenString + " Round Separation Time";
                    tempEntry.spokenText = betweenStringSpoken + " Round Separation Time";
                    tempEntry.spokenPreDelay = 2.0;
                    tempEntry.spokenTextWait = false;
                    tempEntry.spokenTextOnCountdown = "Until start of next round";
                    tempEntry.hasTimer = true;
                    tempEntry.timerSeconds = betweenRoundTimeSeconds;
                    tempEntry.timerEveryThirty = true;
                    tempEntry.timerLastTen = false;
                    tempEntry.hasEndHorn = false;
                    tempEntry.estimatedSeconds = betweenRoundTimeSeconds;
                    playList.Add(tempEntry);
                    sequence += 1;
                }

            }
        }
        if ( e.eventInfo.event_type_code == "f3j" || e.eventInfo.event_type_code == "f3l" || e.eventInfo.event_type_code == "f5j" || e.eventInfo.event_type_code == "td" || e.eventInfo.event_type_code == "gps")
        {
            // Build the f3j or f5j play list
            foreach( EventRound round in rounds)
            {
                EventTask taskInfo = getTask(round.round_number);
                int windowTime = taskInfo.event_task_time_choice;
                List<string> groups = getGroupsInRound(round.round_number);
                // Set lastRound variable
                bool lastRound = round.round_number == rounds.Count ? true : false;

                foreach ( string group in groups)
                {
                    // Set lastgroup variable
                    bool lastGroup = groups.IndexOf(group) == groups.Count - 1 ? true : false;

                    // Get pilot list for this group if needed
                    List<String> pilotList = getPilotList( round.round_number, group );

                    // Skip this group if there is no flight for it in the draw
                    bool groupInRound = false;
                    foreach( EventRoundFlight flight in round.flights)
                    {
                        if( group == flight.group)
                        {
                            groupInRound = true;
                        }
                    }
                    if( ! groupInRound) { continue; }

                    // Let's determine if this group is a reflight group


                    // Now lets start adding queue entries
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add Round and Group header
                    tempEntry = new PlayQueueEntry();
                    tempEntry.sequenceID = sequence;
                    tempEntry.round_number = round.round_number;
                    tempEntry.group = group;
                    tempEntry.entryType = "RoundAnnounce";
                    if( IsReflightGroup(round.round_number, group))
                    {
                        tempEntry.textDescription = "Round " + round.round_number.ToString() + " Reflight Group " + group;
                        tempEntry.spokenText = "Round " + round.round_number.ToString() + ", Reflight group " + group;
                    }
                    else
                    {
                        tempEntry.textDescription = "Round " + round.round_number.ToString() + " Group " + group;
                        tempEntry.spokenText = "Round " + round.round_number.ToString() + ", group " + group;
                    }
                    if (round.round_number != 1)
                    {
                        tempEntry.spokenPreDelay = 2.0;
                    }
                    else
                    {
                        tempEntry.spokenPreDelay = 0.0;
                    }
                    tempEntry.spokenTextWait = true;
                    tempEntry.estimatedSeconds = 2;
                    playList.Add(tempEntry);
                    sequence += 1;

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add task description entry
                    if (announceTasks == true)
                    {
                        tempEntry = new PlayQueueEntry();
                        tempEntry.sequenceID = sequence;
                        tempEntry.round_number = round.round_number;
                        tempEntry.group = group;
                        tempEntry.entryType = "Announce";
                        tempEntry.textDescription = taskInfo.flight_type_name;
                        tempEntry.spokenText = taskInfo.flight_type_description;
                        tempEntry.estimatedSeconds = 4;
                        // Add the prep time notice if they don't want to announce pilots
                        if (announcePilots == false || pilotList.Count == 0)
                        {
                            tempEntry.spokenText += ". Preparation Time of " + prepTimeMinutes.ToString() + " minutes starts in 5, 4, 3, 2, 1, ";
                            tempEntry.estimatedSeconds += 6;
                        }
                        tempEntry.spokenTextWait = true;
                        playList.Add(tempEntry);
                        sequence += 1;
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add pilot list entry if wanted
                    if (announcePilots == true && pilotList.Count > 0)
                    {
                        tempEntry = new PlayQueueEntry();
                        tempEntry.sequenceID = sequence;
                        tempEntry.round_number = round.round_number;
                        tempEntry.entryType = "Announce";
                        tempEntry.group = group;
                        tempEntry.textDescription = "Group " + group + " Pilot List";
                        tempEntry.spokenText = "Group " + group + " pilot list: " + String.Join(", ", pilotList) + ", ";
                        // Add the prep time notice if they don't want to announce pilots
                        tempEntry.spokenText += ". Preparation Time of " + prepTimeMinutes.ToString() + " minutes starts in 5, 4, 3, 2, 1, ";
                        tempEntry.spokenTextWait = true;
                        tempEntry.estimatedSeconds = 2 * pilotList.Count + 9;
                        playList.Add(tempEntry);
                        sequence += 1;
                    }

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Preparation time entry
                    tempEntry = new PlayQueueEntry();
                    tempEntry.sequenceID = sequence;
                    tempEntry.round_number = round.round_number;
                    tempEntry.group = group;
                    tempEntry.entryType = "PrepTime";
                    tempEntry.textDescription = convertSecondsToClockString(prepTimeMinutes * 60) + " Preparation Time";
                    tempEntry.spokenText = prepTimeMinutes.ToString() + " minutes remaining in prep time for Round " + round.round_number.ToString() + " Group " + group;
                    tempEntry.spokenTextWait = false;
                    tempEntry.spokenPreDelay = 2.0;
                    tempEntry.spokenTextOnCountdown = "before working window of Round " + round.round_number.ToString() + " Group " + group;
                    tempEntry.hasTimer = true;
                    tempEntry.hasBeginHorn = true;
                    tempEntry.beginHornLength = 1;
                    tempEntry.timerSeconds = (prepTimeMinutes * 60);
                    tempEntry.timerEveryFifteen = true;
                    tempEntry.timerEveryThirty = true;
                    tempEntry.timerEveryTenInLastMinute = true;
                    tempEntry.timerLastTwenty = true;
                    tempEntry.hasEndHorn = true;
                    tempEntry.endHornLength = 3;
                    tempEntry.estimatedSeconds = (prepTimeMinutes * 60);
                    playList.Add(tempEntry);
                    sequence += 1;

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Window time entry
                    tempEntry = new PlayQueueEntry();
                    tempEntry.sequenceID = sequence;
                    tempEntry.round_number = round.round_number;
                    tempEntry.group = group;
                    tempEntry.entryType = "Window";
                    tempEntry.textDescription = convertSecondsToClockString(windowTime) + " Flight Working Window";
                    tempEntry.spokenText = (windowTime / 60).ToString() + " minnit flight working window";
                    tempEntry.spokenPreDelay = 3.0;
                    tempEntry.spokenTextWait = false;
                    tempEntry.hasBeginHorn = false;
                    tempEntry.hasTimer = true;
                    tempEntry.timerSeconds = windowTime;
                    tempEntry.timerEveryMinute = true;
                    tempEntry.timerEveryThirty = true;
                    tempEntry.timerEveryFifteen = true;
                    tempEntry.timerLastThirty = true;
                    tempEntry.timerLastTen = true;
                    tempEntry.timerEveryFiveInLastMinute = true;
                    tempEntry.hasEndHorn = true;
                    tempEntry.endHornLength = 2;
                    tempEntry.isMainWindow = true;
                    tempEntry.estimatedSeconds = windowTime;
                    playList.Add(tempEntry);
                    sequence += 1;

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add Landing window time if wanted
                    if( useLanding == true )
                    {
                        tempEntry = new PlayQueueEntry();
                        tempEntry.sequenceID = sequence;
                        tempEntry.round_number = round.round_number;
                        tempEntry.group = group;
                        tempEntry.entryType = "Landing";
                        tempEntry.textDescription = "1 Minute Landing Window";
                        tempEntry.spokenText = "1 minnit Landing Window";
                        tempEntry.spokenPreDelay = 2.0;
                        tempEntry.spokenTextWait = false;
                        tempEntry.spokenTextOnCountdown = "in landing window";
                        tempEntry.hasTimer = true;
                        tempEntry.timerSeconds = 60;
                        tempEntry.timerEveryFifteen = true;
                        tempEntry.timerLastFive = true;
                        tempEntry.hasEndHorn = true;
                        tempEntry.endHornLength = 1;
                        tempEntry.estimatedSeconds = 60;
                        playList.Add(tempEntry);
                        sequence += 1;
                    }

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add group separation time if wanted
                    
                    if (betweenGroupTimeSeconds > 0 && !( lastGroup && lastRound) && ! ( lastGroup && ! lastRound ) )
                    {
                        String betweenString = "";
                        String betweenStringSpoken = "";
                        if (betweenGroupTimeSeconds < 60)
                        {
                            betweenString = betweenGroupTimeSeconds.ToString() + " Second";
                            betweenStringSpoken = betweenGroupTimeSeconds.ToString() + " Second";
                        }
                        else
                        {
                            betweenString = (betweenGroupTimeSeconds / 60).ToString() + " Minute";
                            betweenStringSpoken = (betweenGroupTimeSeconds / 60).ToString() + " minnit";
                        }
                        tempEntry = new PlayQueueEntry();
                        tempEntry.sequenceID = sequence;
                        tempEntry.round_number = round.round_number;
                        tempEntry.group = group;
                        tempEntry.entryType = "Wait";
                        tempEntry.textDescription = betweenString + " Group Separation Time";
                        tempEntry.spokenText = betweenStringSpoken + " Group Separation Time";
                        tempEntry.spokenPreDelay = 2.0;
                        tempEntry.spokenTextWait = false;
                        tempEntry.spokenTextOnCountdown = "left in group separation";
                        tempEntry.hasTimer = true;
                        tempEntry.timerSeconds = betweenGroupTimeSeconds;
                        tempEntry.timerEveryFifteen = true;
                        tempEntry.timerLastTen = false;
                        tempEntry.hasEndHorn = false;
                        tempEntry.estimatedSeconds = betweenGroupTimeSeconds;
                        playList.Add(tempEntry);
                        sequence += 1;
                    }
                }
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Add round separation time if wanted
                if (betweenRoundTimeSeconds > 0 && ! lastRound)
                {
                    String betweenString = "";
                    String betweenStringSpoken = "";
                    if (betweenRoundTimeSeconds < 60)
                    {
                        betweenString = betweenRoundTimeSeconds.ToString() + " Second";
                        betweenStringSpoken = betweenRoundTimeSeconds.ToString() + " Second";
                    }
                    else
                    {
                        betweenString = (betweenRoundTimeSeconds / 60).ToString() + " Minute";
                        betweenStringSpoken = (betweenRoundTimeSeconds / 60).ToString() + " minnit";
                    }
                    tempEntry = new PlayQueueEntry();
                    tempEntry.sequenceID = sequence;
                    tempEntry.round_number = round.round_number;
                    tempEntry.entryType = "Wait";
                    tempEntry.textDescription = betweenString + " Round Separation Time";
                    tempEntry.spokenText = betweenStringSpoken + " Round Separation Time";
                    tempEntry.spokenPreDelay = 2.0;
                    tempEntry.spokenTextWait = false;
                    tempEntry.spokenTextOnCountdown = "Until start of next round";
                    tempEntry.hasTimer = true;
                    tempEntry.timerSeconds = betweenRoundTimeSeconds;
                    tempEntry.timerEveryThirty = true;
                    tempEntry.timerLastTen = false;
                    tempEntry.hasEndHorn = false;
                    tempEntry.estimatedSeconds = betweenRoundTimeSeconds;
                    playList.Add(tempEntry);
                    sequence += 1;
                }

            }

        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Add end of playlist entry
        tempEntry = new PlayQueueEntry();
        tempEntry.sequenceID = sequence;
        tempEntry.entryType = "Announce";
        tempEntry.textDescription = "End of contest.";
        if (prefs["announceScoreReminders"] == "1")
        {
            tempEntry.spokenText = "End of contest. Please enter your final scores in the self scoring system. Thank you for flying with F 3 x vault. Have a nice day.";
        }
        else
        {
            tempEntry.spokenText = "End of contest. Thank you for flying with F 3 x vault. Have a nice day.";
        }
        tempEntry.estimatedSeconds = 5;
        playList.Add(tempEntry);

        ShowPlayList();
        queueControl.clockCurrentSeconds = 0;
        queueControl.currentQueueEntry = 0;
        queueControl.UpdateClockText();
        queueControl.CalculateQueueTimeRemaining();
        PreLoadButtonStatus(false);
        return;
    }
    // routine to show the playlist entries
    public void ShowPlayList()
    {
        // Destroy any existing child lines before creating new ones
        foreach (Transform child in queueListObject)
        {
            GameObject.Destroy(child.gameObject);
        }
        // Now lets add the queue entries
        foreach (PlayQueueEntry entry in playList)
        {
            if( entry.textDescription.IndexOf("Round") >= 0)
            {
                // This is a round heading, so get that prefab instead of the normal one
                QueueListDisplayRound displayQueueRound = (QueueListDisplayRound)Instantiate(this.queueListDisplayRoundPrefab);
                displayQueueRound.transform.SetParent(queueListObject, false);
                displayQueueRound.Prime(entry);
            }
            else
            {
                QueueListDisplay displayQueueEntry = (QueueListDisplay)Instantiate(this.queueListDisplayPrefab);
                displayQueueEntry.transform.SetParent(queueListObject, false);
                displayQueueEntry.Prime(entry);
            }
        }
        CalcToDropDown();
        queueControl.CalculateQueueTimeRemaining();
        return;
    }

    // Routine to set the preload button to green or red
    public void PreLoadButtonStatus(bool status)
    {
        if (status)
        {
            preloadButton.image.color = Color.green;
            preloadButtonText.text = "Voice Phrases Cached";
        }
        else
        {
            preloadButton.image.color = new Color(0.9622642f, 0.3767355f, 0.3767355f, 1f);
            preloadButtonText.text = "Preload Voice Cache (Suggested)";
        }
        return;
    }
    // Let us create a list of rounds from the event data
    public void getRounds()
    {
        getNumRounds();
        rounds.Clear();
        if( eventNumRounds.Count > 0)
        {
            foreach (int roundNum in eventNumRounds)
            {
                EventRound tempRound = new EventRound();
                tempRound.round_number = roundNum;
                // Determine if there are rounds set up in the preliminary standings
                foreach(EventStandings pilot in e.eventInfo.prelim_standings.standings)
                {
                    foreach( EventPilotRounds round in pilot.rounds)
                    {
                        if( round.round_number == roundNum)
                        {
                            foreach (EventPilotFlight flight in round.flights)
                            {
                                EventRoundFlight tempflight = new EventRoundFlight();
                                tempRound.flight_type_name = flight.flight_type_name;
                                tempflight.flight_is_reflight = flight.flight_is_reflight;
                                tempflight.flight_type_code = flight.flight_type_code;
                                tempflight.group = flight.flight_group;
                                tempflight.lane = flight.flight_lane;
                                tempflight.pilot_first_name = pilot.pilot_first_name;
                                tempflight.pilot_last_name = pilot.pilot_last_name;
                                tempflight.entered = flight.flight_entered;
                                // Add a new value to array of tempRound flights
                                System.Array.Resize(ref tempRound.flights, tempRound.flights.Length + 1);
                                tempRound.flights[tempRound.flights.Length - 1] = tempflight;
                            }
                            foreach (EventPilotFlight reflight in round.reflights)
                            {
                                EventRoundFlight tempflight = new EventRoundFlight();
                                tempflight.flight_is_reflight = reflight.flight_is_reflight;
                                tempflight.flight_type_code = reflight.flight_type_code;
                                tempflight.group = reflight.flight_group;
                                tempflight.lane = reflight.flight_lane;
                                tempflight.pilot_first_name = pilot.pilot_first_name;
                                tempflight.pilot_last_name = pilot.pilot_last_name;
                                tempflight.entered = reflight.flight_entered;
                                // Add a new value to array of tempRound flights
                                System.Array.Resize(ref tempRound.flights, tempRound.flights.Length + 1);
                                tempRound.flights[tempRound.flights.Length - 1] = tempflight;
                            }
                        }
                    }
                }

                // Now check if there are rounds int the tasks
                if( tempRound.flight_type_name == "")
                {
                    foreach( EventTask task in e.eventInfo.tasks)
                    {
                        if( task.round_number == tempRound.round_number)
                        {
                            EventRoundFlight tempflight = new EventRoundFlight();
                            tempRound.flight_type_name = task.flight_type_code;
                            tempflight.flight_type_code = task.flight_type_code;
                            tempflight.group = "A";
                            tempflight.lane = "1";
                            // Add a new vallue to array of tempRound flights                                
                            System.Array.Resize(ref tempRound.flights, tempRound.flights.Length + 1);
                            tempRound.flights[tempRound.flights.Length - 1] = tempflight;
                        }
                    }
                }
                // Sort the flights by group
                Array.Sort(tempRound.flights, new FlightComparer());
                

                // append the rounds array with the tempround object
                rounds.Add(tempRound);
            }
        }
        return;
    }
    // Create the integer round number array to use for iterating throug the number of rounds
    public void getNumRounds()
    {
        eventNumRounds.Clear();

        // Step through the standings to get the rounds
        foreach( EventStandings s in e.eventInfo.prelim_standings.standings )
        {
            foreach(EventPilotRounds r in s.rounds)
            {
                if( ! eventNumRounds.Contains( r.round_number ))
                {
                    eventNumRounds.Add(r.round_number);
                }
            }
        }
        // Check if there are tasks that have rounds that aren't already in the list
        foreach( EventTask t in e.eventInfo.tasks)
        {
            if (!eventNumRounds.Contains(t.round_number))
            {
                eventNumRounds.Add(t.round_number);
            }
        }
        // Now we have the list of round numbers, so we have to sort them
        eventNumRounds.Sort();
        return;
    }
    // Get the string groups List
    public void getGroups()
    {
        foreach (EventStandings standing in e.eventInfo.prelim_standings.standings)
        {
            foreach (EventPilotRounds r in standing.rounds)
            {
                foreach (EventPilotFlight flight in r.flights)
                {
                    if (!groups.Contains(flight.flight_group))
                    {
                        groups.Add(flight.flight_group);
                    }
                }
            }
        }
        // If there are no groups, then just set one
        if (groups.Count == 0)
        {
            groups.Add("A");
        }
        groups.Sort();
        return;
    }
    // Get the string groups List
    public List<String> getGroupsInRound( int round )
    {
        List<String> groupsInRound = new List<String>();

        foreach (EventStandings standing in e.eventInfo.prelim_standings.standings)
        {
            foreach (EventPilotRounds r in standing.rounds)
            {
                if(r.round_number != round) { continue; }
                foreach (EventPilotFlight flight in r.flights)
                {
                    if (!groupsInRound.Contains(flight.flight_group))
                    {
                        groupsInRound.Add(flight.flight_group);
                    }
                }
                foreach (EventPilotFlight reflight in r.reflights)
                {
                    if (!groupsInRound.Contains(reflight.flight_group))
                    {
                        groupsInRound.Add(reflight.flight_group);
                    }
                }
            }
        }
        // If there are no groups, then just set one
        if (groupsInRound.Count == 0)
        {
            groupsInRound.Add("A");
        }
        groupsInRound.Sort();
        return groupsInRound;
    }
    // Step through the rounds list and get the next group
    public List<string> getNextGroup(int round_number, string group )
    {
        int originalRound = round_number;
        string originalGroup = group;
        int newRound;
        string newGroup = group;
        bool getNext = false;
        groups.Sort();
        foreach( string g in groups)
        {
            if( getNext == true)
            {
                newGroup = g;
                getNext = false;
                break;
            }
            if( g == originalGroup)
            {
                getNext = true;
            }
        }
        if( getNext == true)
        {
            // Increment the round
            newRound = originalRound + 1;
            newGroup = groups[0];
        }
        else
        {
            newRound = originalRound;
        }
        List<string> returnList = new List<string>();
        returnList.Add(newRound.ToString());
        returnList.Add(newGroup.ToString());
        return returnList;
    }

    // Get task info for a given round
    public EventTask getTask(int round)
    {
        EventTask returnTask = new EventTask();
        foreach( EventTask task in e.eventInfo.tasks)
        {
            if( task.round_number == round)
            {
                // Copy the default task to this one for a fresh task
                returnTask.round_number = task.round_number;
                returnTask.flight_type_code = task.flight_type_code;
                returnTask.flight_type_name = task.flight_type_name;
                returnTask.flight_type_name_short = task.flight_type_name_short;
                returnTask.flight_type_description= task.flight_type_description;
                returnTask.flight_type_landing = task.flight_type_minutes;
                returnTask.flight_type_minutes = task.flight_type_minutes;
                returnTask.flight_type_seconds = task.flight_type_seconds;
                returnTask.flight_type_laps = task.flight_type_laps;
                returnTask.flight_type_start_height = task.flight_type_start_height;
                returnTask.flight_type_start_penalty = task.flight_type_start_penalty;
                returnTask.flight_type_over_penalty = task.flight_type_over_penalty;
                returnTask.flight_type_sub_flights = task.flight_type_sub_flights;
                returnTask.event_task_time_choice = task.event_task_time_choice;
            }
        }

        // Let us set the more specific task entry values for the flight type
        foreach ( FlightDescription flight in flightDescriptions.flights )
        {
            if( returnTask.flight_type_code == flight.code )
            {
                returnTask.flight_type_name = flight.name;
                if (returnTask.event_task_time_choice * 60 != flight.windowTime && ( returnTask.flight_type_code == "f3j_duration" || returnTask.flight_type_code == "f5j_duration"))
                {
                    if( returnTask.flight_type_code == "f3j_duration")
                    {
                        returnTask.flight_type_description = "F3j Duration with precision landing... " + returnTask.event_task_time_choice + " minute working window.";
                    }
                    else if (returnTask.flight_type_code == "f5j_duration")
                    {
                        returnTask.flight_type_description = "F5j Electric Duration with precision landing... " + returnTask.event_task_time_choice + " minute working window.";
                    }
                    else
                    {
                        returnTask.flight_type_description = flight.description;
                    }
                    returnTask.event_task_time_choice = 60 * returnTask.event_task_time_choice;
                }
                else
                {
                    returnTask.flight_type_description = flight.description;
                    returnTask.event_task_time_choice = flight.windowTime;
                }
            }
        }

        return returnTask;
    }
    // Detemine prep time from prefs
    public int getPrepTime()
    {
        int minutes = 0;
        switch(prefs["prepTime"])
        {
            case "1 Minute":
                minutes = 1;
                break;
            case "2 Minutes":
                minutes = 2;
                break;
            case "3 Minutes":
                minutes = 3;
                break;
            case "4 Minutes":
                minutes = 4;
                break;
            case "5 Minutes":
                minutes = 5;
                break;
            case "6 Minutes":
                minutes = 6;
                break;
            case "7 Minutes":
                minutes = 7;
                break;
            case "8 Minutes":
                minutes = 8;
                break;
            case "9 Minutes":
                minutes = 9;
                break;
            case "10 Minutes":
                minutes = 10;
                break;
        }
        return minutes;
    }
    public int getBetweenGroupTime()
    {
        int seconds = 0;
        switch( prefs["betweenGroups"])
        {
            case "None":
                seconds = 0;
                break;
            case "15 Seconds":
                seconds = 15;
                break;
            case "30 Seconds":
                seconds = 30;
                break;
            case "1 Minute":
                seconds = 60;
                break;
            case "2 Minutes":
                seconds = 120;
                break;
            case "3 Minutes":
                seconds = 180;
                break;
            case "4 Minutes":
                seconds = 240;
                break;
            case "5 Minutes":
                seconds = 300;
                break;
        }
        return seconds;
    }
    public int getBetweenRoundTime()
    {
        int seconds = 0;
        switch (prefs["betweenRounds"])
        {
            case "None":
                seconds = 0;
                break;
            case "15 Seconds":
                seconds = 15;
                break;
            case "30 Seconds":
                seconds = 30;
                break;
            case "1 Minute":
                seconds = 60;
                break;
            case "2 Minutes":
                seconds = 120;
                break;
            case "3 Minutes":
                seconds = 180;
                break;
            case "4 Minutes":
                seconds = 240;
                break;
            case "5 Minutes":
                seconds = 300;
                break;
        }
        return seconds;
    }
    public List<String> getPilotList(int round, string group)
    {
        List<String> pilots = new List<String>();
        foreach (EventRound r in rounds)
        {
            if (r.round_number == round)
            {
                foreach (EventRoundFlight f in r.flights)
                {
                    if (f.group == group)
                    {
                        // Lets look to see if they have a phoneme of their name
                        if (PlayerPrefs.HasKey(f.pilot_first_name + " " + f.pilot_last_name))
                        {
                            pilots.Add(PlayerPrefs.GetString(f.pilot_first_name + " " + f.pilot_last_name));
                        }
                        else
                        {
                            pilots.Add(f.pilot_first_name + " " + f.pilot_last_name);
                        }
                    }
                }
            }
        }
        pilots.Sort();
        return pilots;
    }
    public List<String> getPilotReminderList(int round)
    {
        List<String> pilots = new List<String>();

        foreach (EventRound r in rounds)
        {
            if (r.round_number <= round)
            {
                foreach (EventRoundFlight f in r.flights)
                {
                    if (f.entered == 0 && ! ( pilots.Contains(f.pilot_first_name + " " + f.pilot_last_name ) || pilots.Contains(PlayerPrefs.GetString(f.pilot_first_name + " " + f.pilot_last_name))) )
                    {
                        // Add the pilot to the list
                        // Lets look to see if they have a phoneme of their name
                        if (PlayerPrefs.HasKey(f.pilot_first_name + " " + f.pilot_last_name))
                        {
                            pilots.Add(PlayerPrefs.GetString(f.pilot_first_name + " " + f.pilot_last_name));
                        }
                        else
                        {
                            pilots.Add(f.pilot_first_name + " " + f.pilot_last_name);
                        }
                    }
                }
            }
        }
        pilots.Sort();
        return pilots;
    }
    public Boolean IsReflightGroup(int round_number, string group)
    {
        Boolean isReflight = false;
        foreach (EventStandings pilot in e.eventInfo.prelim_standings.standings)
        {
            foreach (EventPilotRounds round in pilot.rounds)
            {
                if (round.round_number != round_number) { continue; }
                foreach (EventPilotFlight reflight in round.reflights)
                {
                    if( reflight.flight_is_reflight == 1 && reflight.flight_group == group)
                    {
                        isReflight = true;
                    }
                }
            }
        }
        return isReflight;
    }
    // Time convert number of seconds to clock string
    public String convertSecondsToClockString( int seconds )
    {
        String clockString = "0:00";
        int min = seconds / 60;
        int sec = seconds % 60;
        if( sec < 0)
        {
            if( Math.Abs(sec) < 10)
            {
                clockString = "-" + Math.Abs(min).ToString() + ":0" + Math.Abs(sec).ToString();
            }
            else
            {
                clockString = "-" + Math.Abs(min).ToString() + ":" + Math.Abs(sec).ToString();
            }
        }
        else
        {
            if( sec < 10)
            {
                clockString = min.ToString() + ":0" + sec.ToString();
            }
            else
            {
                clockString = min.ToString() + ":" + sec.ToString();
            }
        }
        return clockString;
    }
    private void OnDisable()
    {
        this.SavePrefs();
        return;
    }

}

class FlightComparer: IComparer
{
    public int Compare(object x, object y)
    {
        int result;
        result = ((EventRoundFlight)x).group.CompareTo(((EventRoundFlight)y).group);
        if (result == 0)
            result = ((EventRoundFlight)x).lane.CompareTo(((EventRoundFlight)y).lane);
        if (result == 0)
            result = ((EventRoundFlight)x).pilot_first_name.CompareTo(((EventRoundFlight)y).pilot_first_name);
        return result;
    }
}
class PilotComparer : IComparer
{
    public int Compare(object x, object y)
    {
        int result;
        result = ((EventPilot)x).pilot_first_name.CompareTo(((EventPilot)y).pilot_first_name);
        return result;
    }
}
class PilotBibComparer : IComparer
{
    public int Compare(object x, object y)
    {
        int result;
        result = ((EventPilot)x).pilot_bib.CompareTo(((EventPilot)y).pilot_bib);
        return result;
    }
}
public static class Extensions
{
    private static System.Random rand = new System.Random();

    public static void Shuffle<T>(this IList<T> values)
    {
        for (int i = values.Count - 1; i > 0; i--)
        {
            int k = rand.Next(i + 1);
            T value = values[k];
            values[k] = values[i];
            values[i] = value;
        }
    }
}
