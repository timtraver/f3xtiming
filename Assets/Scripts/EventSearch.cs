using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventSearch : MonoBehaviour
{
    public EventSearchList results;
    public Transform targetTransform;
    public EventListDisplay entryDisplayPrefab;
    public EventListDisplay entryDisplayPrefabYellow;
    public EventListDisplay entryDisplayPrefabGreen;
    public Dropdown disciplineDropDown;
    public List<EventType> types = new List<EventType>();
    public InputField searchStringField;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the search fields
        this.InitializeTypes();
        this.InitializeSearchString();

        // Add listener for when the value of the Dropdown changes, to take action
        disciplineDropDown.onValueChanged.AddListener(delegate { DropDownItemSelected(disciplineDropDown); });

        // Add listener for when the value of the Search String changes, to take action
        searchStringField.onEndEdit.AddListener(delegate { SearchFieldUpdated(searchStringField); });

        // Call the search event class to get a list of events
        this.SearchEvents();
        return;
    }

    // Actually perform the api search call
    void SearchEvents()
    {
        vaultAPI api = new vaultAPI();

        api.disc = this.types[disciplineDropDown.value].typeCode;
        api.searchString = this.searchStringField.text;
        this.results = api.EventSearch();
        this.ShowSearchResults();
        return;
    }

    // Show the results by creating all of the line objects
    void ShowSearchResults()
    {
        // Destroy any existing child lines before creating new ones
        foreach (Transform child in targetTransform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // Update the list to show the events
        foreach (EventSearchInfo e in this.results.events)
        {
            var now = System.DateTime.Now;
            var rowDate = System.DateTime.Parse(e.event_start_date);
            var daysDiff = (rowDate - now).Days;
            // Lets determine what background color the row should have from its date
            if (daysDiff > 7)
            {
                // This is farther than 7 days in the future
                // Set to yellow
                EventListDisplay display = (EventListDisplay)Instantiate(this.entryDisplayPrefabYellow);
                display.transform.SetParent(targetTransform, false);
                display.Prime(e);
            }
            else if (daysDiff < 0)
            {
                // This is older than today
                // leave the default color
                EventListDisplay display = (EventListDisplay)Instantiate(this.entryDisplayPrefab);
                display.transform.SetParent(targetTransform, false);
                display.Prime(e);
            }
            else
            {
                // This is between now and 7 days
                // Set to green
                EventListDisplay display = (EventListDisplay)Instantiate(this.entryDisplayPrefabGreen);
                display.transform.SetParent(targetTransform, false);
                display.Prime(e);
            }
        }
        return;
    }

    public void DropDownItemSelected(Dropdown dropdown)
    {
        this.SearchEvents();
        return;
    }
    public void SearchFieldUpdated(InputField SearchStringField)
    {
        this.SearchEvents();
        return;
    }

    public void InitializeTypes()
    {
        this.types.Add(new EventType(typeCode: "", typeDescription: "All Disiplines"));
        this.types.Add(new EventType(typeCode: "f3b", typeDescription: "F3B Multi Task"));
        this.types.Add(new EventType(typeCode: "f3f", typeDescription: "F3F Slope Racing"));
        this.types.Add(new EventType(typeCode: "f3j", typeDescription: "F3J Thermal Duration"));
        this.types.Add(new EventType(typeCode: "f3k", typeDescription: "F3K Hand Launch"));
        this.types.Add(new EventType(typeCode: "TD", typeDescription: "TD Thermal Duration"));
        this.types.Add(new EventType(typeCode: "mom", typeDescription: "MOM Slope Racing"));
        this.types.Add(new EventType(typeCode: "gps", typeDescription: "GPS Triangle Racing"));
        this.types.Add(new EventType(typeCode: "f5j", typeDescription: "F5J Electric Duration"));

        // Initialize the dropdown list
        disciplineDropDown.options.Clear();
        foreach ( var type in types)
        {
            disciplineDropDown.options.Add( new Dropdown.OptionData() { text = type.typeDescription } );
        }

        // Set the initial selection of the dropdown from the prefs
        disciplineDropDown.value = PlayerPrefs.GetInt("eventSearchTypeCode", 0);
        disciplineDropDown.RefreshShownValue();
    }

    // Initialize Search String field to a saved preference if there is one
    public void InitializeSearchString()
    {
        this.searchStringField.text = PlayerPrefs.GetString("eventSearchString", "");
        return;
    }
    void OnDisable()
    {
        // Save the prefernces used for this search
        PlayerPrefs.SetInt("eventSearchTypeCode", this.disciplineDropDown.value);
        PlayerPrefs.SetString("eventSearchString", this.searchStringField.text);
        PlayerPrefs.Save();
        return;
    }

}
