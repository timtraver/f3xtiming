using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.UI;

public class SerialClockBoard : MonoBehaviour
{
    public List<string> serialPorts = new List<string>();
    public List<string> serialPorts2 = new List<string>();
    public Dropdown prefsSerialPort;
    public Dropdown prefsSerialPort2;
    public Dropdown prefsProtocol;
    public Dropdown prefsProtocol2;
    public Dropdown prefsBaud;
    public Dropdown prefsBaud2;
    public Toggle prefsSendClockTime;
    public ClockUpdate topClock;
    public QueueControl queueControl;
    public Text clockText;
    public Text sentToClock;
    public Text sentToClock2;

    public int hour;
    public int minutes;
    public int minutesOld;
    public int seconds;
    public int secondsOld;

    private string currentSerialPort;
    private string currentSerialPort2;
    private Dictionary<String, int> groupConversion = new Dictionary<string, int>();
    public SerialPort serialPort;
    public SerialPort serialPort2;
    public Boolean serialPortIsOpening;
    public Boolean serialPort2IsOpening;

    // Start is called before the first frame update
    void Start()
    {
        // Load Prefences
        currentSerialPort = "None";
        currentSerialPort2 = "None";
        serialPortIsOpening = false;
        serialPort2IsOpening = false;
        LoadClockPrefs();
        secondsOld = 0;
        minutesOld = 0;
        groupConversion.Add("A", 1);
        groupConversion.Add("B", 2);
        groupConversion.Add("C", 3);
        groupConversion.Add("D", 4);
        groupConversion.Add("E", 5);
        groupConversion.Add("F", 6);
        groupConversion.Add("G", 7);
        groupConversion.Add("H", 8);
        groupConversion.Add("I", 9);
        groupConversion.Add("J", 10);
        groupConversion.Add("a", 1);
        groupConversion.Add("b", 2);
        groupConversion.Add("c", 3);
        groupConversion.Add("d", 4);
        groupConversion.Add("e", 5);
        groupConversion.Add("f", 6);
        groupConversion.Add("g", 7);
        groupConversion.Add("h", 8);
        groupConversion.Add("i", 9);
        groupConversion.Add("j", 10);
        return;
    }

    public List<string> GetSerialPorts()
    {
        string[] rawPorts;
        List<string> portNames = new List<string>();

        switch (Application.platform)
        {
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.LinuxPlayer:
                rawPorts = SerialPort.GetPortNames();
                if (rawPorts.Length == 0)
                {
                    rawPorts = System.IO.Directory.GetFiles("/dev/");
                }
                foreach (string portName in rawPorts)
                {
                    if (portName.StartsWith("/dev/tty.usb") || portName.StartsWith("/dev/ttyUSB") || portName.StartsWith("/dev/tty.") )
                    {
                        portNames.Add(portName);
                    }
                }
                break;
            default: // Windows
                rawPorts = SerialPort.GetPortNames();
                if (rawPorts.Length > 0)
                {
                    foreach (string portName in rawPorts)
                    {
                        portNames.Add(portName);
                    }
                }
                break;
        }
        return portNames;
    }
    public void PrefsSerialPortDropDown()
    {
        // Close the port when we do this
        if ( serialPort != null && serialPort.IsOpen) { serialPort.Close(); }

        // Get the list of ports
        serialPorts = GetSerialPorts();

        var savedSerialPort = PlayerPrefs.GetString("prefsSerialPort", "None");
        prefsSerialPort.options.Clear();
        prefsSerialPort.options.Add(new Dropdown.OptionData() { text = "None" });
        foreach (string port in serialPorts)
        {
            prefsSerialPort.options.Add(new Dropdown.OptionData() { text = port });
        }
        // Set the initial selection of the dropdown from the prefs
        int selectedValue = prefsSerialPort.options.FindIndex((i) => { return i.text.Equals(savedSerialPort); });
        prefsSerialPort.SetValueWithoutNotify(selectedValue);
        if( selectedValue == 0)
        {
            // set it to none and save the preferences
            savedSerialPort = "None";
            SaveClockPrefs();
        }
        prefsSerialPort.RefreshShownValue();
        currentSerialPort = savedSerialPort;
        // Now reopen the serialPort
        // if (currentSerialPort != "None") { OpenSerialPort(); }
        return;
    }
    public void PrefsSerialPortDropDown2()
    {
        // Close the port when we do this
        if (serialPort2 != null && serialPort2.IsOpen) { serialPort2.Close(); }

        // Get the list of ports
        serialPorts2 = GetSerialPorts();

        var savedSerialPort2 = PlayerPrefs.GetString("prefsSerialPort2", "None");
        prefsSerialPort2.options.Clear();
        prefsSerialPort2.options.Add(new Dropdown.OptionData() { text = "None" });
        foreach (string port in serialPorts2)
        {
            prefsSerialPort2.options.Add(new Dropdown.OptionData() { text = port });
        }
        // Set the initial selection of the dropdown from the prefs
        int selectedValue = prefsSerialPort2.options.FindIndex((i) => { return i.text.Equals(savedSerialPort2); });
        prefsSerialPort2.SetValueWithoutNotify(selectedValue);
        if (selectedValue == 0)
        {
            // set it to none and save the preferences
            savedSerialPort2 = "None";
            SaveClockPrefs();
        }
        prefsSerialPort2.RefreshShownValue();
        currentSerialPort2 = savedSerialPort2;
        // Now reopen the serialPort
        // if (currentSerialPort != "None") { OpenSerialPort(); }
        return;
    }
    public void ReloadSerialPorts()
    {
        // Set the serial port to none and reload
        PlayerPrefs.SetString("prefsSerialPort", "None");
        PrefsSerialPortDropDown();
        return;
    }
    public void ReloadSerialPorts2()
    {
        // Set the serial port 2 to none and reload
        PlayerPrefs.SetString("prefsSerialPort2", "None");
        PrefsSerialPortDropDown2();
        return;
    }
    // Load the clock preferences
    public void LoadClockPrefs()
    {
        prefsProtocol.SetValueWithoutNotify(prefsProtocol.options.FindIndex((i) => { return i.text.Equals(PlayerPrefs.GetString("prefsProtocol", "Simple")); }));
        prefsProtocol.RefreshShownValue();
        prefsProtocol2.SetValueWithoutNotify(prefsProtocol2.options.FindIndex((i) => { return i.text.Equals(PlayerPrefs.GetString("prefsProtocol2", "Simple")); }));
        prefsProtocol2.RefreshShownValue();
        prefsBaud.SetValueWithoutNotify(prefsBaud.options.FindIndex((i) => { return i.text.Equals(PlayerPrefs.GetString("prefsBaud", "9600 bps")); }));
        prefsBaud.RefreshShownValue();
        prefsBaud2.SetValueWithoutNotify(prefsBaud2.options.FindIndex((i) => { return i.text.Equals(PlayerPrefs.GetString("prefsBaud2", "9600 bps")); }));
        prefsBaud2.RefreshShownValue();
        prefsSendClockTime.SetIsOnWithoutNotify(PlayerPrefs.GetInt("prefsSendClockTime", 1 ) == 1 ? true : false);
        PrefsSerialPortDropDown();
        return;
    }
    // Save the clock preferences
    public void SaveClockPrefs()
    {
        // Save the preferences used for the clock
        PlayerPrefs.SetString("prefsSerialPort", prefsSerialPort.options[prefsSerialPort.value].text);
        PlayerPrefs.SetString("prefsSerialPort2", prefsSerialPort2.options[prefsSerialPort2.value].text);
        PlayerPrefs.SetString("prefsProtocol", prefsProtocol.options[prefsProtocol.value].text);
        PlayerPrefs.SetString("prefsProtocol2", prefsProtocol2.options[prefsProtocol2.value].text);
        PlayerPrefs.SetString("prefsBaud", prefsBaud.options[prefsBaud.value].text);
        PlayerPrefs.SetString("prefsBaud2", prefsBaud2.options[prefsBaud2.value].text);
        PlayerPrefs.SetInt("prefsSendClockTime", prefsSendClockTime.isOn ? 1 : 0);
        currentSerialPort = prefsSerialPort.options[prefsSerialPort.value].text;
        currentSerialPort2 = prefsSerialPort2.options[prefsSerialPort2.value].text;
        return;
    }
    public void onSendTimeToggleChange()
    {
        if(PlayerPrefs.GetInt("prefsSendClockTime") == 0)
        {
            // Send a 0:00 to the clock to clearthe time
            SendSerialData("0000");
        }
        return;
    }

    private void OpenSerialPort()
    {
        // Open the serial port with the port set
        serialPortIsOpening = true;
        int baudRate;
        switch (prefsBaud.options[prefsBaud.value].text)
        {
            case "9600 bps":
                baudRate = 9600;
                break;
            case "19200 bps":
                baudRate = 19200;
                break;
            case "38400 bps":
                baudRate = 38400;
                break;
            case "57600 bps":
                baudRate = 57600;
                break;
            default:
                baudRate = 9600;
                break;
        }
        serialPort = new SerialPort(currentSerialPort, baudRate);
        serialPort.Open();
        string protocol = PlayerPrefs.GetString("prefsProtocol");
        if ( protocol == "Extended + Pandora")
        {
            // Let us send an initialization string
            string sendString = "P|01|01|0|- - N/A\r\n";
            serialPort.Write(sendString);
            serialPort.BaseStream.Flush();
            sentToClock.text = sendString.Replace("\n", " ");
        }
        serialPortIsOpening = false;
        return;
    }
    private void OpenSerialPort2()
    {
        // Open the serial port with the port set
        serialPort2IsOpening = true;
        int baudRate;
        switch (prefsBaud2.options[prefsBaud2.value].text)
        {
            case "9600 bps":
                baudRate = 9600;
                break;
            case "19200 bps":
                baudRate = 19200;
                break;
            case "38400 bps":
                baudRate = 38400;
                break;
            case "57600 bps":
                baudRate = 57600;
                break;
            default:
                baudRate = 9600;
                break;
        }
        serialPort2 = new SerialPort(currentSerialPort2, baudRate);
        serialPort2.Open();
        string protocol = PlayerPrefs.GetString("prefsProtocol2");
        if (protocol == "Extended + Pandora")
        {
            // Let us send an initialization string
            string sendString = "P|01|01|0|- - N/A\r\n";
            serialPort2.Write(sendString);
            serialPort2.BaseStream.Flush();
            sentToClock2.text = sendString.Replace("\n", " ");
        }
        serialPort2IsOpening = false;
        return;
    }

    public void SendSerialData(string timeString, string windowType = "PT", int round_number = 1, string group = "1" )
    {
        if (currentSerialPort == "None" || serialPortIsOpening) { return; } // Don't do anything if the serial port chosen is none or is in the process of opening
        // Check to see if the serial port is open
        if ( serialPort == null || ! serialPort.IsOpen)
        {
            OpenSerialPort();
            return;
        }
        string sendString = "";

        // Send the data string in the format selected
        string protocol = PlayerPrefs.GetString("prefsProtocol", "Simple");
        sendString = getSendString(timeString, protocol, windowType, round_number, group);

        // Now send the string down the serial port
        serialPort.Write(sendString);
        serialPort.BaseStream.Flush();
        sentToClock.text = sendString.Replace("\n", " ");
        return;
    }
    public void SendSerialData2(string timeString, string windowType = "PT", int round_number = 1, string group = "1")
    {
        if (currentSerialPort2 == "None" || serialPort2IsOpening) { return; } // Don't do anything if the serial port chosen is none or is in the process of opening
        // Check to see if the serial port is open
        if (serialPort2 == null || !serialPort2.IsOpen)
        {
            OpenSerialPort2();
            return;
        }
        string sendString = "";

        // Send the data string in the format selected
        string protocol = PlayerPrefs.GetString("prefsProtocol2", "Simple");
        sendString = getSendString(timeString, protocol, windowType, round_number, group);

        // Now send the string down the serial port
        serialPort2.Write(sendString);
        serialPort2.BaseStream.Flush();
        sentToClock2.text = sendString.Replace("\n", " ");
        return;
    }

    public string getSendString(string timeString, string protocol = "Simple", string windowType = "PT", int round_number = 1, string group = "1")
    {
        // Routine to set the sendstring so we don't duplicate the code for the s3cond serial port
        int min;
        int sec;
        int totalSeconds;
        string sendString = "";

        switch (protocol)
        {
            case "Simple":
                sendString = "A" + timeString + "\r\n";
                break;
            case "Extended":
                sendString = string.Format("R{0:00}G{1}T{2}{3}\r", round_number, group.PadLeft(2, '0'), timeString, windowType);
                break;
            case "F3KMaster":
                // Let us figure out the number of seconds from the clock value sent
                min = Convert.ToInt32(timeString.Substring(0, 2));
                sec = Convert.ToInt32(timeString.Substring(2));
                totalSeconds = (60 * min) + sec;
                string colorString = "0 0 0";
                switch (windowType)
                {
                    case "LT":
                        colorString = "255 0 0";
                        break;
                    case "NF":
                        colorString = "254 0 0";
                        break;
                    case "TT":
                        colorString = "255 127 0";
                        break;
                    case "WT":
                        colorString = "0 255 0";
                        break;
                    case "PT":
                        colorString = "200 200 200";
                        break;
                    case "DT":
                    case "ST":
                    default:
                        colorString = "255 255 255";
                        break;
                }
                sendString = "b 100\nt " + totalSeconds.ToString() + " " + colorString + "\n";
                break;
            case "Extended + Pandora":
                min = Convert.ToInt32(timeString.Substring(0, 2));
                sec = Convert.ToInt32(timeString.Substring(2));
                totalSeconds = (60 * min) + sec;
                EventTask task = queueControl.e.getTask(round_number);
                if (queueControl.queueTimerRunning)
                {
                    sendString = string.Format("P|{0:00}|{1}|{2}|{3}\r\n", round_number, group.PadLeft(2, '0'), queueControl.playList[queueControl.currentQueueEntry].allUpFlight.ToString(), task.flight_type_name_short);
                    sendString += string.Format("R{0:00}G{1}T{2}{3}\r", round_number, group.PadLeft(2, '0'), timeString, windowType);
                }
                else
                {
                    sendString = string.Format("P|{0:00}|{1}|{2}|{3}\r\n", round_number, group.PadLeft(2, '0'), 0, "- - N/A");
                    sendString += string.Format("R{0:00}G{1}T{2}{3}\r", round_number, group.PadLeft(2, '0'), timeString, windowType);
                }
                break;
        }

        return sendString;
    }

    public string getGroup( string group)
    {
        // method to determine if the group is alpha and replace it when sending it to the enhanced clock
        int n;
        bool isNumeric = int.TryParse(group, out n);
        if (! isNumeric)
        {
            // Attempt tp Convert it to numeric from alpha
            int found = 0;
            if (groupConversion.ContainsKey(group))
            {
                found = groupConversion[group];
                group = found.ToString();
            }
        }
        return group;
    }
    public void OnDisable()
    {
        // close the serial port if there is one
        if (serialPort != null)
        {
            serialPort.Close();
        }
        if (serialPort2 != null)
        {
            serialPort2.Close();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (currentSerialPort != "None" || currentSerialPort2 != "None")
        {
            if( queueControl.queueTimerRunning)
            {
                if( queueControl.clockTimerRunning)
                {
                    int currentSeconds = Convert.ToInt32(Math.Ceiling(queueControl.clockCurrentSeconds));
                    int clockMin = currentSeconds / 60;
                    int clockSec = currentSeconds % 60;
                    // Let's figure out what the clock says and send the data
                    if (clockSec != secondsOld || clockMin != minutesOld)
                    {
                        // Time has changed, so lets update the serialClock with value if needed
                        // Determine the type
                        string type = "";
                        switch (queueControl.playList[queueControl.currentQueueEntry].entryType)
                        {
                            case "Announce":
                            case "Wait":
                                type = "ST";
                                break;
                            case "PrepTime":
                                type = "PT";
                                break;
                            case "Testing":
                                type = "TT";
                                break;
                            case "NoFly":
                                type = "NF";
                                break;
                            case "Window":
                                type = "WT";
                                break;
                            case "Landing":
                                type = "LT";
                                break;
                            default:
                                type = "DT";
                                break;
                        }
                        string group = getGroup(queueControl.playList[queueControl.currentQueueEntry].group);
                        if (currentSerialPort != "None")
                        {
                            SendSerialData(string.Format("{0:00}{1:00}", clockMin, clockSec), type, queueControl.playList[queueControl.currentQueueEntry].round_number, group);
                        }
                        if (currentSerialPort2 != "None")
                        {
                            SendSerialData2(string.Format("{0:00}{1:00}", clockMin, clockSec), type, queueControl.playList[queueControl.currentQueueEntry].round_number, group);
                        }
                        secondsOld = clockSec;
                        minutesOld = clockMin;
                    }
                }
                else
                {
                    if (secondsOld != 0)
                    {
                        // Let us update the board with a 0:00 time
                        if (currentSerialPort != "None")
                        {
                            SendSerialData(string.Format("{0:00}{1:00}", 0, 0), "ST", queueControl.playList[queueControl.currentQueueEntry].round_number, queueControl.playList[queueControl.currentQueueEntry].group);
                        }
                        if (currentSerialPort2 != "None")
                        {
                            SendSerialData2(string.Format("{0:00}{1:00}", 0, 0), "ST", queueControl.playList[queueControl.currentQueueEntry].round_number, queueControl.playList[queueControl.currentQueueEntry].group);
                        }
                        secondsOld = 0;
                    }
                }
            }
            else
            {
                // Send clock time when timer is idle if the pref is selected
                if (PlayerPrefs.GetInt("prefsSendClockTime", 1) == 1)
                {
                    hour = topClock.hour;
                    minutes = topClock.minutes;
                    seconds = topClock.seconds;
                    if( hour == 0) { hour = 12; }
                    if( hour > 12) { hour -= 12; }
                    if (seconds != secondsOld) // Send only when the seconds change
                    {
                        // Time has changed, so lets update the serialClock with value if needed
                        if (currentSerialPort != "None")
                        {
                            SendSerialData(string.Format("{0:00}{1:00}", hour, minutes), "PT", 1, "01");
                        }
                        if (currentSerialPort2 != "None")
                        {
                            SendSerialData2(string.Format("{0:00}{1:00}", hour, minutes), "PT", 1, "01");
                        }
                        secondsOld = seconds;
                    }
                }
            }

        }
        return;
    }
}
