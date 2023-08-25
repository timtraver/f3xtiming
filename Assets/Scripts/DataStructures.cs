using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// API Data structure classes
[System.Serializable]
public class EventSearchInfo
{
    public int event_id;
    public string event_name;
    public string event_start_date;
    public string location_name;
    public string country_code;
    public string event_type_name;
    public string event_type_code;
}
// Event Search List Structure
[System.Serializable]
public class EventSearchList
{
    public int response_code;
    public string error_string;
    public int total_records;
    public EventSearchInfo[] events;
}

// Event Details
[System.Serializable]
public class EventDetailResponse
{
    public int response_code;
    public string error_string;
    public EventDetail eventInfo;
}

[System.Serializable]
public class EventDetail
{
    public int event_id;
    public string event_name;
    public int location_id;
    public string location_name;
    public string country_code;
    public string start_date;
    public string end_date;
    public string event_type_code;
    public string event_type_name;
    public string event_calc_accuracy_string;
    public int total_rounds;
    public EventTask[] tasks;
    public EventPilot[] pilots;
    public EventPrelimStandings prelim_standings;
    public EventFlyoffStandings[] flyoff_standings;
}
[System.Serializable]
public class EventTask
{
    public int round_number;
    public string flight_type_code;
    public string flight_type_name;
    public string flight_type_name_short;
    public string flight_type_description;
    public int flight_type_landing;
    public int flight_type_minutes;
    public int flight_type_seconds;
    public int flight_type_laps;
    public int flight_type_start_height;
    public int flight_type_start_penalty;
    public int flight_type_over_penalty;
    public int flight_type_sub_flights;
    public int event_task_time_choice;
    public bool rowColor;
}
[System.Serializable]
public class EventPilot
{
    public int pilot_id;
    public int pilot_bib;
    public string pilot_first_name;
    public string pilot_last_name;
    public string country_code;
    public string pilot_class;
    public string pilot_ama;
    public string pilot_fai;
    public string pilot_fai_license;
    public string pilot_team;
}


// Standings 
[System.Serializable]
public class EventPrelimStandings
{
    public int total_rounds;
    public int total_drops;
    public EventStandings[] standings;
}
[System.Serializable]
public class EventFlyoffStandings
{
    public int flyoff_number;
    public int total_rounds;
    public int total_drops;
    public EventStandings[] standings;
}
[System.Serializable]
public class EventStandings
{
    public int pilot_position;
    public int pilot_id;
    public int pilot_bib;
    public string pilot_first_name;
    public string pilot_last_name;
    public string country_code;
    public double total_score;
    public double total_diff;
    public double total_drop;
    public int total_penalties;
    public double total_percent;
    public EventPilotRounds[] rounds;
}

[System.Serializable]
public class EventRound
{
    public string flight_type_name;
    public int round_number;
    public EventRoundFlight[] flights = new EventRoundFlight[0];
}
[System.Serializable]
public class EventRoundFlight
{
    public int flight_is_reflight;
    public string flight_type_code;
    public string flight_type_name;
    public bool flight_header;
    public bool group_header;
    public int pilot_id;
    public int pilot_bib;
    public string pilot_first_name;
    public string pilot_last_name;
    public string country_code;
    public int rank;
    public string group;
    public string lane;
    public int order;
    public int minutes;
    public string seconds;
    public int laps;
    public int landing;
    public int start_height;
    public int start_penalty;
    public double score;
    public int penalty;
    public int dropped;
    public int score_status;
    public EventPilotFlightSub[] subs;
}


[System.Serializable]
public class EventPilotRounds
{
    public int round_number;
    public double round_score;
    public bool round_dropped;
    public EventPilotFlight[] flights;
    public EventPilotFlight[] reflights;
}
[System.Serializable]
public class EventPilotFlight
{
    public int flight_is_reflight;
    public string flight_type_code;
    public string flight_type_name;
    public int flight_rank;
    public int flight_order;
    public string flight_group;
    public string flight_lane;
    public int flight_minutes;
    public string flight_seconds;
    public int flight_laps;
    public int flight_landing;
    public int flight_start_height;
    public int flight_start_penalty;
    public double flight_score;
    public int flight_penalty;
    public int flight_dropped;
    public int score_status;
    public EventPilotFlightSub[] flight_subs;

}
[System.Serializable]
public class EventPilotFlightSub
{
    public int sub_num;
    public string sub_val;
}


// Structure for the playlist creation
public class PlayQueueEntry
{
    public int sequenceID;
    public string textDescription;
    public string spokenText;
    public bool spokenTextWait;
    public double spokenPreDelay;
    public double spokenPostDelay;
    public string spokenTextOnCountdown;
    public bool hasBeginHorn;
    public int beginHornLength;
    public bool hasEndHorn;
    public int endHornLength;
    public bool hasTimer;
    public double timerSeconds;
    public string timerStart;
    public bool timerCountdownToStart;
    public bool timerLastFive;
    public bool timerLastTen;
    public bool timerLastTwenty;
    public bool timerLastThirty;
    public bool timerEveryFifteen;
    public bool timerEveryThirty;
    public bool timerEveryTenInLastMinute;
    public bool timerEveryFiveInLastMinute;
    public bool timerEveryMinute;
    public bool isAllUp;
    public int allUpFlight;
    public int hornLength;
    public int round_number;
    public string group;
    public bool isMainWindow;
    public string entryType;
    public int estimatedSeconds;

    void Start()
    {
        this.sequenceID = 0;
        this.textDescription = "";
        this.spokenText = "";
        this.spokenTextWait = true;
        this.spokenPreDelay = 0.0;
        this.spokenPostDelay = 0.0;
        this.spokenTextOnCountdown = "";
        this.hasBeginHorn = false;
        this.beginHornLength = 2;
        this.hasEndHorn = false;
        this.endHornLength = 2;
        this.hasTimer = false;
        this.timerSeconds = 0.0;
        this.timerStart = "";
        this.timerCountdownToStart = false;
        this.timerLastFive = false;
        this.timerLastTen = true;
        this.timerLastTwenty = false;
        this.timerLastThirty = false;
        this.timerEveryFifteen = false;
        this.timerEveryThirty = false;
        this.timerEveryTenInLastMinute = false;
        this.timerEveryFiveInLastMinute = false;
        this.timerEveryMinute = false;
        this.isAllUp = false;
        this.allUpFlight = 0;
        this.hornLength = 2;
        this.round_number = 0;
        this.group = "";
        this.isMainWindow = false;
        this.entryType = "";
        this.estimatedSeconds = 0;
    }
}

public class FlightDescriptions
{
    public List<FlightDescription> flights = new List<FlightDescription>();

    public FlightDescriptions()
    {
        // Initialize the flight list
        // F3B Duration Window
        flights.Add(new FlightDescription(
            code: "f3b_duration",
            name: "F3B Duration",
            description: "F3B duration with precision landing. Twelve minute working window.",
            windowTime: 720
        ));
        // F3B Distance Window
        flights.Add(new FlightDescription(
            code: "f3b_distance",
            name: "F3B Distance",
            description: "F3B distance. Seven minute working window.",
            windowTime: 420
        ));
        // F3B Speed Window
        flights.Add(new FlightDescription(
            code: "f3b_speed",
            name: "F3B Speed",
            description: "F3B speed. Four minute working window.",
            windowTime: 240
        ));
        flights.Add(new FlightDescription(
            code: "f3k_a",
            name: "F3K Task A - Last 1 x 5:00 in 7:00",
            description: "F3k Task A - Last flight. Max five minutes. Seven minute working window.",
            windowTime: 420
        ));
        flights.Add(new FlightDescription(
            code: "f3k_a2",
            name: "F3K Task A - Last 1 x 5:00 in 10:00",
            description: "F3k Task A - Last Flight. Max five minutes. Ten minute working window.",
            windowTime: 600
        ));
        flights.Add(new FlightDescription(
            code: "f3k_b",
            name: "F3K Task B - Last 2 x 4:00",
            description: "F3k Task B - Last two flights. Max four minutes. Ten minute working window.",
            windowTime: 600
        ));
        flights.Add(new FlightDescription(
            code: "f3k_b2",
            name: "F3K Task B - Last 2 x 3:00",
            description: "F3k Task B - Last two flights. Max three minutes. Seven minute working window.",
            windowTime: 420
        ));
        flights.Add(new FlightDescription(
            code: "f3k_c",
            name: "F3K Task C - All Up 3 x 3:00",
            description: "F3k Task C - All up last down. Single launch. Max three minutes. Three three minute working windows. Must launch before three second start signal ends.",
            windowTime: 183
        ));
        flights.Add(new FlightDescription(
            code: "f3k_c2",
            name: "F3K Task C - All Up 4 x 3:00",
            description: "F3k Task C - All up last down. Single launch. Max three minutes. Four three minute working windows. Must launch before three second start signal ends.",
            windowTime: 183
        ));
        flights.Add(new FlightDescription(
            code: "f3k_c3",
            name: "F3K Task C - All Up 5 x 3:00",
            description: "F3k Task C - All up last down. Single launch. Max three minutes. Five three minute working windows. Must launch before three second start signal ends.",
            windowTime: 183
        ));
        flights.Add(new FlightDescription(
            code: "f3k_d",
            name: "F3K Task D - Ladder",
            description: "F3k Task D - Ladder. Increasing time by fifteen seconds. Flights of 30 seconds, 45 seconds, 1 minute, 1 minute 15 seconds, 1 minute 30 seconds, 1 minute 45 seconds, and 2 minutes. Ten minute working window. Must achieve or exceed flight time before proceeding to the next target flight.",
            windowTime: 600
        ));
        flights.Add(new FlightDescription(
            code: "f3k_d2",
            name: "F3K Task D (2020) - 2 x 5:00",
            description: "F3k Task D - Two flights only. Max five minutes. Ten minute working window.",
            windowTime: 600
        ));
        flights.Add(new FlightDescription(
            code: "f3k_e",
            name: "F3K Task E - Poker",
            description: "F3k Task E - Poker. Pilot nominated times. Five max target times. Ten minute working window. Must achieve or exceed flight time before proceeding to the next target flight.",
            windowTime: 600
        ));
        flights.Add(new FlightDescription(
            code: "f3k_e2",
            name: "F3K Task E - Poker (2020) 10",
            description: "F3k Task E - Poker. Pilot nominated times. Three max target times. Ten minute working window. Must achieve or exceed flight time before proceeding to the next target flight. May call end of window.",
            windowTime: 600
        ));
        flights.Add(new FlightDescription(
            code: "f3k_e3",
            name: "F3K Task E - Poker (2020) 15",
            description: "F3k Task E - Poker. Pilot nominated times. Three max target times. Fifteen minute working window. Must achieve or exceed flight time before proceeding to the next target flight. May call end of window.",
            windowTime: 900
        ));
        flights.Add(new FlightDescription(
            code: "f3k_f",
            name: "F3K Task F - Best 3 x 3:00",
            description: "F3k Task F - Best three flights. Max three minutes. Maximum six launches. Ten minute working window.",
            windowTime: 600
        ));
        flights.Add(new FlightDescription(
            code: "f3k_g",
            name: "F3K Task G - Best 5 x 2:00",
            description: "F3k Task G - Best five flights. Max two minutes. Ten minute working window.",
            windowTime: 600
        ));
        flights.Add(new FlightDescription(
            code: "f3k_h",
            name: "F3K Task H - 1, 2, 3, 4",
            description: "F3k Task H - One minute, two minute, three minute and four minute max flights in any order. Ten minute working window.",
            windowTime: 600
        ));
        flights.Add(new FlightDescription(
            code: "f3k_i",
            name: "F3K Task I - 3 x 3:20",
            description: "F3k Task I - Three longest flights. Max three minutes twenty seconds. Ten minute working window.",
            windowTime: 600
        ));
        flights.Add(new FlightDescription(
            code: "f3k_j",
            name: "F3K Task J - Last 3 x 3:00",
            description: "F3k Task J - Last three flights. Max three minutes. Ten minute working window.",
            windowTime: 600
        ));
        flights.Add(new FlightDescription(
            code: "f3k_k",
            name: "F3K Task K - Big Ladder",
            description: "F3k Task K - Big Ladder. Five target flights increasing time by thirty seconds. One minute, one minute thirty seconds, two minutes, two minutes thirty seconds, and three minute flights in order. All time counts. Ten minute working window.",
            windowTime: 600
        ));
        flights.Add(new FlightDescription(
            code: "f3k_l",
            name: "F3K Task L - Single Flight",
            description: "F3k Task L - Single flight. Single launch. Max time nine minutes fifty nine seconds. Ten minute working window.",
            windowTime: 600
        ));
        flights.Add(new FlightDescription(
            code: "f3k_m",
            name: "F3K Task M - Huge Ladder 3, 5, 7",
            description: "F3k Task M - Huge ladder. Three flights only. Three minute, five minute, and seven minnit flights in order. All time achieved counts. Fifteen minute working window.",
            windowTime: 900
        ));
        // F3J Flight
        flights.Add(new FlightDescription(
            code: "f3j_duration",
            name: "F3J Duration",
            description: "F3j Duration with precision landing. Ten minute working window.",
            windowTime: 600
        ));
        // F3L Flight
        flights.Add(new FlightDescription(
            code: "f3l_duration",
            name: "F3L F3RES Duration",
            description: "F3L, F3RES. Six minnit duration with precision landing. Nine minnit working window.",
            windowTime: 540
        ));
        // F5J Flight
        flights.Add(new FlightDescription(
            code: "f5j_duration",
            name: "F5J Electric Duration",
            description: "F5j Electric duration with precision landing. Ten minute working window.",
            windowTime: 600
        ));
        // F5J Low Launch
        flights.Add(new FlightDescription(
            code: "f5j_low30",
            name: "F5J Low Launch 30 min",
            description: "F5j Electric duration low launch competition. Thirty minute working window.",
            windowTime: 1800
        ));
        flights.Add(new FlightDescription(
            code: "f5j_low60",
            name: "F5J Low Launch 1 hour",
            description: "F5j Electric duration low launch competition. One hour working window.",
            windowTime: 3600
        ));
        // GPS Flight
        flights.Add(new FlightDescription(
            code: "gps_distance",
            name: "GPS Triangle Distance",
            description: "GPS Triangle distance with landing. Maximum laps around GPS triangle. Thirty minute working window.",
            windowTime: 1800
        ));
        flights.Add(new FlightDescription(
            code: "gps_speed",
            name: "GPS Triangle Speed",
            description: "GPS Triangle speed. Fastest lap around GPS triangle. Five minute working window.",
            windowTime: 300
        ));

        return;
    }
}
public class FlightDescription
{
    public string code;
    public string name;
    public string description;
    public int windowTime;

    public FlightDescription(string code, string name, string description, int windowTime)
    {
        this.code = code;
        this.name = name;
        this.description = description;
        this.windowTime = windowTime;
        return;
    }
}

// Drop Down List structures
// Structure for the Event Search discipline drop down list
public class EventType
{
    public string typeCode;
    public string typeDescription;
    public EventType(string typeCode, string typeDescription)
    {
        this.typeCode = typeCode;
        this.typeDescription = typeDescription;
    }
}

// Structure for the Event Preferences voice drop down list
public class PreferenceVoice
{
    public string voiceCulture;
    public string voiceDescription;
    public void EventType(string voiceCulture, string voiceDescription)
    {
        this.voiceCulture = voiceCulture;
        this.voiceDescription = voiceDescription;
    }
}