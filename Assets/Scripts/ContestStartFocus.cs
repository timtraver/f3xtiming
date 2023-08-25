using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContestStartFocus : MonoBehaviour
{
    InputField contestStart;
    void Start()
    {
        //Fetch the Input Field component from the GameObject
        contestStart = GetComponent<InputField>();
    }

    void Update()
    {
        if (contestStart.isFocused)
        {
            contestStart.GetComponent<Image>().color = Color.green;
            contestStart.Select();
        }
        if (!contestStart.isFocused && contestStart.text == "")
        {
            contestStart.GetComponent<Image>().color = Color.white;
        }
    }
}
