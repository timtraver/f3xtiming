using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Crosstales.RTVoice;
using Crosstales.RTVoice.Model;

public class Practice : MonoBehaviour
{
    public Dictionary<String, String> prefs = new Dictionary<string, string>();
    public ChangeScene scene;
    public List<Voice> voices;
    public Dropdown prefsHorn;
    public Dropdown prefsCulture;
    public Dropdown prefsVoice;
    public Dropdown prefsPrepTime;
    public Dropdown prefsBetweenRounds;
    public Toggle prefsUseLanding;
    public Toggle prefsUseOneMinuteNoFly;
    public Toggle prefsAnnounceTasks;
    public List<PlayQueueEntry> playList = new List<PlayQueueEntry>();
    public Transform queueListObject;
    public PracticeQueueListDisplayRound queueListDisplayRoundPrefab;
    public PracticeQueueListDisplay queueListDisplayPrefab;
    public FlightDescriptions flightDescriptions = new FlightDescriptions();
    public PracticeQueueControl queueControl;
    public Button createPlayListButton;
    public Dropdown taskChoice;
    public Text taskDescription;
    public Toggle repeatToggle;
    public Toggle overrideToggle;
    public InputField windowMinutes;
    public Dropdown numGroups;
    public Transform roundListObject;
    public List<EventTask> roundList = new List<EventTask>();
    public PracticeRoundListDisplay roundDisplayPrefab;
    public Button preloadButton;
    public Text preloadButtonText;
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


    // Start is called before the first frame update
    void Start()
    {
        // Add listener for when the value of the Language Dropdown changes, to take action
        // prefsCulture.onValueChanged.AddListener(delegate { LanguageDropDownItemSelected(prefsCulture); });

        if (audioSource == null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
        }
        // Load the saved event from the prefs
        this.LoadPrefs();
        windowMinutes.text = null;
        overrideToggle.isOn = false;
    }

    // DropDown List Actions
    public void LanguageDropDownItemSelected(Dropdown dropdown)
    {
        this.PrefsVoiceDropDown();
        return;
    }
    public void TaskDropDownItemSelected()
    {
        playList.Clear();
        if ( taskChoice.value == 0)
        {
            taskDescription.text = "";
            createPlayListButton.enabled = false;
            createPlayListButton.interactable = false;
        }
        else
        {
            createPlayListButton.enabled = true;
            createPlayListButton.interactable = true;
            // Update the task description when the task is selected from the dropdown
            int selection = taskChoice.value - 1;
            taskDescription.text = flightDescriptions.flights[selection].description;
        }
        SavePrefs();
        return;
    }
    // Test a selected voice with a text string
    public void VoiceTest()
    {
        queueControl.noSpeakEndEvent = true;
        Speaker.Instance.Speak("30 seconds before a 10 minute working window.", audioSource,Speaker.Instance.VoiceForName(prefs["voice"]), true);
        return;
    }
    public void MikeSmithRuleAnnounce()
    {
        List<String> msRuleStrings = new List<String>();
        msRuleStrings.Add("Mike has a history of something going wrong at the last minnit.");
        msRuleStrings.Add("Rumor has it that Mike spotted the lead singer of Spandau Ballet and needed to get an autograph.");
        msRuleStrings.Add("Some of the parts of his plane must have fallen off.");
        msRuleStrings.Add("Does anyone have any C A?");
        msRuleStrings.Add("The guy that he paid 20 bucks to build his plane swore it went on that way.");
        msRuleStrings.Add("Once the body goes, the mind follows.");
        msRuleStrings.Add("He must have lost the batteries in his hearing aid.");
        msRuleStrings.Add("He's finishing up the last sleave of girl scout cookies.");
        msRuleStrings.Add("Meditation takes a while.");
        msRuleStrings.Add("You just can't fix stupid.");
        msRuleStrings.Add("I think he just got vaccinated in the bushes.");
        msRuleStrings.Add("Drinking 3 red bulls has made him a little shakey.");
        msRuleStrings.Add("He must have had the finish his Justin Beeber song to get in the groove.");
        msRuleStrings.Add("Twitter was calling him to stop harassing Madonna.");
        msRuleStrings.Add("Remember kiddos, always bring a back up.");
        msRuleStrings.Add("The peanut butter sandwich is sticking to the roof of his mouth.");

        if (queueControl.clockTimerRunning)
        {
            // Pause the clock
            queueControl.TogglePlayPause();
        }
        queueControl.noSpeakEndEvent = true;
        string insultString = msRuleStrings[insult];
        Speaker.Instance.Speak("Attention, the mike smith rule has been called. " + insultString + " Please hold while we remedy the situation.", audioSource, Speaker.Instance.VoiceForName(prefs["voice"]), true);
        insult++;
        if (insult >= msRuleStrings.Count) { insult = 0; }
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
        ReloadPrefs();
        queueControl.voice = Speaker.Instance.VoiceForName(prefs["voice"]);
        //queueControl.VoicePreLoad();
        preloadButton.image.color = new Color(0.9622642f, 0.3767355f, 0.3767355f, 1f);
        preloadButtonText.text = "Preload Voice Cache (Suggested)";
        return;
    }
    public void TaskChoiceDropDown()
    {
        // populate the task choices dropdown
        int savedTask = PlayerPrefs.GetInt("practiceTask", 0);
        taskChoice.options.Clear();
        // Add the default one
        taskChoice.options.Add(new Dropdown.OptionData() { text = "Select a task to practice..." });
        // Let's get the tasks
        foreach (FlightDescription flight in flightDescriptions.flights)
        {
            taskChoice.options.Add(new Dropdown.OptionData() { text = flight.name });
        }
        // Set the initial selection of the dropdown from the prefs
        taskChoice.value = savedTask;
        taskChoice.RefreshShownValue();
        if(taskChoice.value == 0)
        {
            // Turn off the create playlist button for now
            createPlayListButton.enabled = false;
            createPlayListButton.interactable = false;
        }
        return;
    }
    void LoadPrefs()
    {
        // Load preferences into Dictionary array for easier access
        prefs["horn"] = PlayerPrefs.GetString("prefsHorn", "Ship Horn");
        prefs["culture"] = PlayerPrefs.GetString("prefsCulture", "en-US");
        prefs["voice"] = PlayerPrefs.GetString("prefsVoice", "Samantha");
        prefs["prepTime"] = PlayerPrefs.GetString("prefsPrepTime", "5 Minutes");
        prefs["betweenRounds"] = PlayerPrefs.GetString("prefsBetweenRounds", "1 Minute");
        prefs["useLanding"] = PlayerPrefs.GetString("prefsUseLanding", "1");
        prefs["useNoFly"] = PlayerPrefs.GetString("prefsUseNoFly", "0");
        prefs["announceTasks"] = PlayerPrefs.GetString("prefsAnnounceTasks", "0");
        prefs["repeatPlayList"] = PlayerPrefs.GetString("repeatPlayList", "0");

        // Now set the objects to their preferences values
        prefsHorn.SetValueWithoutNotify( prefsHorn.options.FindIndex((i) => { return i.text.Equals(prefs["horn"]); }) );
        prefsHorn.RefreshShownValue();

        prefsPrepTime.SetValueWithoutNotify( prefsPrepTime.options.FindIndex((i) => { return i.text.Equals(prefs["prepTime"]); }) );
        prefsPrepTime.RefreshShownValue();

        prefsBetweenRounds.SetValueWithoutNotify( prefsBetweenRounds.options.FindIndex((i) => { return i.text.Equals(prefs["betweenRounds"]); }));
        prefsBetweenRounds.RefreshShownValue();

        prefsUseLanding.SetIsOnWithoutNotify(prefs["useLanding"] == "1" ? true : false);
        prefsUseOneMinuteNoFly.SetIsOnWithoutNotify(prefs["useNoFly"] == "1" ? true : false);
        prefsAnnounceTasks.SetIsOnWithoutNotify(prefs["announceTasks"] == "1" ? true : false);
        if (queueControl.queueTimerRunning)
        {
            queueControl.voice = Speaker.Instance.VoiceForName(prefs["voice"]);
        }
        repeatToggle.SetIsOnWithoutNotify(prefs["repeatPlayList"] == "1" ? true : false);
        TaskChoiceDropDown();
        return;
    }
    public void SavePrefs()
    {
        // Save the preferences used for the audio stuff
        PlayerPrefs.SetString("prefsHorn", prefsHorn.options[prefsHorn.value].text);
        if (prefsCulture.options.Count > 0)
        {
            PlayerPrefs.SetString("prefsCulture", prefsCulture.options[prefsCulture.value].text);
        }
        if (prefsVoice.options.Count > 0)
        {
            PlayerPrefs.SetString("prefsVoice", prefsVoice.options[prefsVoice.value].text);
        }
        PlayerPrefs.SetString("prefsPrepTime", prefsPrepTime.options[prefsPrepTime.value].text);
        PlayerPrefs.SetString("prefsBetweenRounds", prefsBetweenRounds.options[prefsBetweenRounds.value].text);
        PlayerPrefs.SetString("prefsUseLanding", prefsUseLanding.isOn ? "1" : "0");
        PlayerPrefs.SetString("prefsUseNoFly", prefsUseOneMinuteNoFly.isOn ? "1" : "0");
        PlayerPrefs.SetString("prefsAnnounceTasks", prefsAnnounceTasks.isOn ? "1" : "0");
        PlayerPrefs.SetString("repeatPlayList", repeatToggle.isOn ? "1" : "0");
        PlayerPrefs.SetInt("practiceTask", taskChoice.value);
        PlayerPrefs.Save();
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
        // Now lets clear the list
        playList.Clear();

        int sequence = 1;
        int prepTimeMinutes = getPrepTime();
        int betweenTimeSeconds = getBetweenTime();
        bool announceTasks = prefs["announceTasks"] == "1" ? true : false;
        bool useLanding = prefs["useLanding"] == "1" ? true : false;
        PlayQueueEntry tempEntry = new PlayQueueEntry();
        int groups = numGroups.value + 1;
        if(roundList.Count == 0)
        {
            EventTask task = getTask(taskChoice.options[taskChoice.value].text);
            // Let us add a single element to the list to use to build the playlist
            roundList.Add(task);
            DisplayTasks();
        }

        foreach (EventTask taskInfo in roundList)
        {
            int windowTime = taskInfo.event_task_time_choice;

            for(int group = 1; group <= groups; group++)
            {
                if (taskInfo.flight_type_code.StartsWith("f3k"))
                {
                    // Build the f3k audio play list
                    int loops = 1;
                    if (taskInfo.flight_type_code == "f3k_c") { loops = 3; }
                    if (taskInfo.flight_type_code == "f3k_c2") { loops = 4; }
                    if (taskInfo.flight_type_code == "f3k_c3") { loops = 5; }


                    // Now lets start adding queue entries
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add Round and Group header
                    tempEntry = new PlayQueueEntry();
                    tempEntry.sequenceID = sequence;
                    tempEntry.round_number = 1;
                    tempEntry.group = group.ToString();
                    tempEntry.entryType = "Announce";
                    tempEntry.textDescription = "Round " + taskInfo.round_number.ToString() + " Group " + group.ToString();
                    tempEntry.spokenText = "Round " + taskInfo.round_number.ToString() + " Group " + group.ToString();
                    tempEntry.spokenTextWait = true;
                    tempEntry.estimatedSeconds = 1;
                    playList.Add(tempEntry);
                    sequence += 1;

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add task description entry
                    if (announceTasks == true)
                    {
                        tempEntry = new PlayQueueEntry();
                        tempEntry.sequenceID = sequence;
                        tempEntry.round_number = 1;
                        tempEntry.group = group.ToString();
                        tempEntry.entryType = "Announce";
                        tempEntry.textDescription = taskInfo.flight_type_name;
                        tempEntry.spokenText = taskInfo.flight_type_description;
                        tempEntry.estimatedSeconds = 2;
                        // Add the prep time notice if they don't want to announce pilots
                        tempEntry.spokenText += "...Preparation Time of " + prepTimeMinutes.ToString() + " minutes starts in...5...4...3...2...1...";
                        tempEntry.estimatedSeconds += 7;
                        tempEntry.spokenTextWait = true;
                        playList.Add(tempEntry);
                        sequence += 1;
                    }

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Preparation time entry
                    tempEntry = new PlayQueueEntry();
                    tempEntry.sequenceID = sequence;
                    tempEntry.round_number = 1;
                    tempEntry.group = group.ToString();
                    tempEntry.entryType = "PrepTime";
                    tempEntry.textDescription = convertSecondsToClockString(prepTimeMinutes * 60) + " Preparation Time";
                    tempEntry.spokenText = prepTimeMinutes.ToString() + " minutes remaining in prep time.";
                    tempEntry.spokenTextWait = false;
                    tempEntry.spokenPreDelay = 2.0;
                    tempEntry.spokenTextOnCountdown = "before launch window";
                    if (loops > 1)
                    {
                        tempEntry.spokenTextOnCountdown += " of Flight 1.";
                    }
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

                    for (int loop = 1; loop <= loops; loop++)
                    {
                        if (loop > 1)
                        {
                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            // Add 1 minute in between flights windows (for f3k C tasks only
                            tempEntry = new PlayQueueEntry();
                            tempEntry.sequenceID = sequence;
                            tempEntry.round_number = 1;
                            tempEntry.group = group.ToString();
                            tempEntry.entryType = "PrepTime";
                            tempEntry.textDescription = "1:00 No Fly Time";
                            tempEntry.spokenText = "1 Minute no fly time before launch window of Flight " + loop.ToString() + ".";
                            tempEntry.spokenPreDelay = 1.5;
                            tempEntry.spokenTextWait = false;
                            tempEntry.spokenTextOnCountdown = "before launch window of Flight " + loop.ToString() + ".";
                            if (loop == 1)
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
                        tempEntry.round_number = 1;
                        tempEntry.entryType = "Window";
                        tempEntry.group = group.ToString();
                        if (loops > 1)
                        {
                            tempEntry.textDescription = convertSecondsToClockString(windowTime) + " Flight " + loop.ToString() + " Window";
                            tempEntry.spokenText = (windowTime / 60).ToString() + " minute window for flight " + loop.ToString() + ".";
                        }
                        else
                        {
                            tempEntry.textDescription = convertSecondsToClockString(windowTime) + " Flight Window";
                            tempEntry.spokenText = (windowTime / 60).ToString() + " minute flight window.";
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
                            tempEntry.round_number = 1;
                            tempEntry.group = group.ToString();
                            tempEntry.entryType = "Landing";
                            tempEntry.textDescription = "30 Second Landing Window";
                            tempEntry.spokenText = "30 second landing window.";
                            tempEntry.spokenPreDelay = 2.0;
                            tempEntry.spokenTextWait = false;
                            tempEntry.spokenTextOnCountdown = "in landing window";
                            tempEntry.hasTimer = true;
                            tempEntry.timerSeconds = 30;
                            tempEntry.timerEveryTenInLastMinute = true;
                            tempEntry.timerLastTen = true;
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
                    if (betweenTimeSeconds > 0)
                    {
                        String betweenString = "";
                        if (betweenTimeSeconds < 60)
                        {
                            betweenString = betweenTimeSeconds.ToString() + " Second";
                        }
                        else
                        {
                            betweenString = (betweenTimeSeconds / 60).ToString() + " Minute";
                        }
                        tempEntry = new PlayQueueEntry();
                        tempEntry.sequenceID = sequence;
                        tempEntry.round_number = 1;
                        tempEntry.group = group.ToString();
                        tempEntry.entryType = "Wait";
                        tempEntry.textDescription = betweenString + " Group Separation Time";
                        tempEntry.spokenText = betweenString + " Group Separation Time";
                        tempEntry.spokenPreDelay = 2.0;
                        tempEntry.spokenTextWait = false;
                        tempEntry.spokenTextOnCountdown = "Until next group";
                        tempEntry.hasTimer = true;
                        tempEntry.timerSeconds = betweenTimeSeconds;
                        tempEntry.timerEveryThirty = true;
                        tempEntry.timerLastTen = false;
                        tempEntry.hasEndHorn = false;
                        tempEntry.estimatedSeconds = betweenTimeSeconds;
                        playList.Add(tempEntry);
                        sequence += 1;
                    }
                }
                if (taskInfo.flight_type_code.StartsWith("f3j") || taskInfo.flight_type_code.StartsWith("f5j") || taskInfo.flight_type_code.StartsWith("td") || taskInfo.flight_type_code.StartsWith("gps"))
                {
                    // Build the f3j or f5j play list

                    // Now lets start adding queue entries
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add Round and Group header
                    tempEntry = new PlayQueueEntry();
                    tempEntry.sequenceID = sequence;
                    tempEntry.round_number = 1;
                    tempEntry.group = group.ToString();
                    tempEntry.entryType = "Announce";
                    tempEntry.textDescription = "Round " + taskInfo.round_number.ToString() + " Group " + group.ToString();
                    tempEntry.spokenText = "Round " + taskInfo.round_number.ToString() + " Group " + group.ToString();
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
                        tempEntry.round_number = 1;
                        tempEntry.group = group.ToString();
                        tempEntry.entryType = "Announce";
                        tempEntry.textDescription = taskInfo.flight_type_name;
                        tempEntry.spokenText = taskInfo.flight_type_description;
                        tempEntry.estimatedSeconds = 4;
                        // Add the prep time notice if they don't want to announce pilots
                        tempEntry.spokenText += "...Preparation Time of " + prepTimeMinutes.ToString() + " minutes starts in...5...4...3...2...1...";
                        tempEntry.estimatedSeconds += 6;
                        tempEntry.spokenTextWait = true;
                        playList.Add(tempEntry);
                        sequence += 1;
                    }

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Preparation time entry
                    tempEntry = new PlayQueueEntry();
                    tempEntry.sequenceID = sequence;
                    tempEntry.round_number = 1;
                    tempEntry.group = group.ToString();
                    tempEntry.entryType = "PrepTime";
                    tempEntry.textDescription = convertSecondsToClockString(prepTimeMinutes * 60) + " Preparation Time";
                    tempEntry.spokenText = prepTimeMinutes.ToString() + " minutes remaining in prep time";
                    tempEntry.spokenTextWait = false;
                    tempEntry.spokenPreDelay = 2.0;
                    tempEntry.spokenTextOnCountdown = "before launch window";
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
                    tempEntry.round_number = 1;
                    tempEntry.group = group.ToString();
                    tempEntry.entryType = "Window";
                    tempEntry.textDescription = convertSecondsToClockString(windowTime) + " Flight Window";
                    tempEntry.spokenText = (windowTime / 60).ToString() + " minute flight window";
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
                    if (useLanding == true)
                    {
                        tempEntry = new PlayQueueEntry();
                        tempEntry.sequenceID = sequence;
                        tempEntry.round_number = 1;
                        tempEntry.group = group.ToString();
                        tempEntry.entryType = "Landing";
                        tempEntry.textDescription = "1 Minute Landing Window";
                        tempEntry.spokenText = "1 Minute Landing Window";
                        tempEntry.spokenPreDelay = 2.0;
                        tempEntry.spokenTextWait = false;
                        tempEntry.spokenTextOnCountdown = "in landing window";
                        tempEntry.hasTimer = true;
                        tempEntry.timerSeconds = 60;
                        tempEntry.timerEveryTenInLastMinute = true;
                        tempEntry.timerLastTen = true;
                        tempEntry.hasEndHorn = true;
                        tempEntry.endHornLength = 1;
                        tempEntry.estimatedSeconds = 60;
                        playList.Add(tempEntry);
                        sequence += 1;
                    }

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Add group separation time if wanted
                    if (betweenTimeSeconds > 0)
                    {
                        String betweenString = "";
                        if (betweenTimeSeconds < 60)
                        {
                            betweenString = betweenTimeSeconds.ToString() + " Second";
                        }
                        else
                        {
                            betweenString = (betweenTimeSeconds / 60).ToString() + " Minute";
                        }
                        tempEntry = new PlayQueueEntry();
                        tempEntry.sequenceID = sequence;
                        tempEntry.round_number = 1;
                        tempEntry.group = group.ToString();
                        tempEntry.entryType = "Wait";
                        tempEntry.textDescription = betweenString + " Group Separation Time";
                        tempEntry.spokenText = betweenString + " Group Separation Time";
                        tempEntry.spokenPreDelay = 2.0;
                        tempEntry.spokenTextWait = false;
                        tempEntry.spokenTextOnCountdown = "Until next group";
                        tempEntry.hasTimer = true;
                        tempEntry.timerSeconds = betweenTimeSeconds;
                        tempEntry.timerEveryThirty = true;
                        tempEntry.timerLastTen = false;
                        tempEntry.hasEndHorn = false;
                        tempEntry.estimatedSeconds = betweenTimeSeconds;
                        playList.Add(tempEntry);
                        sequence += 1;
                    }
                }
            }
        }
        ShowPlayList();
        queueControl.clockCurrentSeconds = 0;
        queueControl.currentQueueEntry = 0;
        queueControl.UpdateClockText();
        queueControl.playList = playList;
        preloadButton.image.color = new Color(0.9622642f, 0.3767355f, 0.3767355f,1f);
        preloadButtonText.text = "Preload Voice Cache (Suggested)";
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
                PracticeQueueListDisplayRound displayQueueRound = (PracticeQueueListDisplayRound)Instantiate(this.queueListDisplayRoundPrefab);
                displayQueueRound.transform.SetParent(queueListObject, false);
                displayQueueRound.Prime(entry);
            }
            else
            {
                PracticeQueueListDisplay displayQueueEntry = (PracticeQueueListDisplay)Instantiate(this.queueListDisplayPrefab);
                displayQueueEntry.transform.SetParent(queueListObject, false);
                displayQueueEntry.Prime(entry);
            }
        }
        return;
    }
    public void AddTask()
    {
        if(taskChoice.value == 0) { return; }

        // Create the event task object to send to the round list
        EventTask task = new EventTask();
        task.round_number = roundList.Count + 1;
        task.flight_type_name = taskChoice.options[taskChoice.value].text;

        EventTask tempTask = getTask(taskChoice.options[taskChoice.value].text);
        task.event_task_time_choice = tempTask.event_task_time_choice;
        task.flight_type_code = tempTask.flight_type_code;
        task.flight_type_description = tempTask.flight_type_description;
        if (overrideToggle.isOn && windowMinutes.text != null && windowMinutes.text != "")
        {
            task.event_task_time_choice = Convert.ToInt32(windowMinutes.text) * 60;
        }

        roundList.Add(task);

        DisplayTasks();
        return;
    }
    public void DisplayTasks()
    {
        // routine to display the task list
        // Destroy any existing child lines before creating new ones
        foreach (Transform child in roundListObject)
        {
            GameObject.Destroy(child.gameObject);
        }
        int i = 1;
        foreach(EventTask entry in roundList)
        {
            entry.round_number = i;
            PracticeRoundListDisplay displayRound = (PracticeRoundListDisplay)Instantiate(this.roundDisplayPrefab);
            displayRound.transform.SetParent(roundListObject, false);
            displayRound.Prime(entry);
            i++;
        }
        return;
    }

    // Get task info for a given round
    public EventTask getTask(string description)
    {
        EventTask returnTask = new EventTask();

        // Let us search the task list to set the values
        foreach ( FlightDescription flight in flightDescriptions.flights )
        {
            if( flight.name == description)
            {
                returnTask.flight_type_name = flight.name;
                returnTask.flight_type_code = flight.code;
                returnTask.flight_type_description = flight.description;
                returnTask.event_task_time_choice = flight.windowTime;

                if (overrideToggle.isOn && windowMinutes.text != null && windowMinutes.text != "" )
                {
                    returnTask.event_task_time_choice = Convert.ToInt32(windowMinutes.text) * 60;
                }

                if (returnTask.flight_type_code == "f3j_duration")
                {
                    returnTask.flight_type_description = "F3j Duration with precision landing... " + (returnTask.event_task_time_choice / 60).ToString() + " minute working window.";
                }
                else if (returnTask.flight_type_code == "f5j_duration")
                {
                    returnTask.flight_type_description = "F5j Electric Duration with precision landing... " + (returnTask.event_task_time_choice / 60).ToString() + " minute working window.";
                }
            }
        }

        return returnTask;
    }
    public EventTask getRoundTask(int round_number)
    {
        // Method to get the task from teh round number
        EventTask returnTask = new EventTask();
        foreach (EventTask task in roundList)
        {
            if( task.round_number == round_number)
            {
                returnTask = task;
                break;
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
    public int getBetweenTime()
    {
        int seconds = 0;
        switch( prefs["betweenRounds"])
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
    // Time convert number of seconds to clock string
    public String convertSecondsToClockString( int seconds)
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
