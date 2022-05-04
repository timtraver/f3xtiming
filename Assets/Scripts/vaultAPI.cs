using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;

public class vaultAPI
{
    public string host;
    public string uri;
    public string queryString;
    public string method;
    public string outputFormat;
    public IDictionary<string,string> requestVariables;
    public string responseString;
    public Object responseObject;
    public string login;
    public string password;
    public int perpage;
    public string disc;
    public string searchString;

    // Instance init
    public vaultAPI()
    {
        this.host = "www.f3xvault.com";
        this.uri = "/api.php";
        this.queryString = "";
        this.method = "GET";
        this.responseString = "";
        this.outputFormat = "json";
        this.login = "guest";
        this.password = "Guest1234";
        this.requestVariables = new Dictionary<string, string>();
        this.perpage = 100;
        this.disc = "all";
        this.searchString = "";
    }

    // Clear all request variables
    public void clearRequestVariables()
    {
        this.requestVariables.Clear();
    }

    // Set a variable pair to be sent
    public void setRequestVariable(string name, string value)
    {
        this.requestVariables.Add(name, value);
    }

    // Build the request query string
    public void buildRequestVariables()
    {
        this.queryString = "";
        // Set mandatory variables to send
        this.setRequestVariable("login", this.login);
        this.setRequestVariable("password", this.password);
        this.setRequestVariable("output_format", this.outputFormat );

        foreach( KeyValuePair<string,string> entry in requestVariables)
        {
            this.queryString += entry.Key + '=' + entry.Value + '&';
        }
        return;
    }

    // Actually make the web request to the API
    public int makeAPICall()
    {
        // Build the queryString
        this.buildRequestVariables();
        // Build the URL
        string url = "https://" + this.host + "/" + this.uri + "?" + this.queryString;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        this.responseString = reader.ReadToEnd();
        return 1;
    }

    public EventSearchList EventSearch()
    {
        this.clearRequestVariables();
        this.setRequestVariable("function", "searchEvents");
        this.setRequestVariable("per_page", this.perpage.ToString());
        this.setRequestVariable("event_type_code", this.disc);
        this.setRequestVariable("string", this.searchString);
        var success = makeAPICall();
        return JsonUtility.FromJson<EventSearchList>(this.responseString);
    }

    public EventDetailResponse EventGetDetail( int event_id )
    {
        this.clearRequestVariables();
        this.setRequestVariable("function", "getEventInfoFull" );
        this.setRequestVariable("event_id", event_id.ToString() );
        var success = makeAPICall();
        Debug.Log("call results = " + this.responseString);
        // Because unity cannot have a serializable data structure named "event" we have to replace that string
        this.responseString = this.responseString.Replace("\"event\":", "\"eventInfo\":");
        return JsonUtility.FromJson<EventDetailResponse>(this.responseString);
    }
}
