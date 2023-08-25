using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HoverTipManager : MonoBehaviour
{
    public TextMeshProUGUI tipText;
    public RectTransform tipWindow;
    public enum Location { UpperLeft, UpperRight, LowerLeft, LowerRight }
    public Location tipLocation;

    public static Action<string, Vector2, Location> OnMouseHover;
    public static Action OnMouseLoseFocus;

    private void OnEnable()
    {
        OnMouseHover += ShowTip;
        OnMouseLoseFocus += HideTip;

    }
    private void OnDisable()
    {
        OnMouseHover -= ShowTip;
        OnMouseLoseFocus -= HideTip;
    }


    void Start()
    {
        HideTip();
    }


    private void ShowTip(string tip, Vector2 mousePos, Location tipLocation)
    {
        tipText.text = tip;
        tipWindow.sizeDelta = new Vector2(tipText.preferredWidth > 200 ? 200 : tipText.preferredWidth, tipText.preferredHeight);
        tipWindow.gameObject.SetActive(true);
        tipWindow.gameObject.transform.SetAsLastSibling();

        // Determine the location to show which side and top or bottom to show the tip
        switch (tipLocation)
        {
            case Location.UpperLeft:
                tipWindow.transform.position = new Vector2(mousePos.x - tipWindow.sizeDelta.x * 0.6f, mousePos.y + 40);
                break;
            case Location.UpperRight:
                tipWindow.transform.position = new Vector2(mousePos.x + tipWindow.sizeDelta.x * 0.6f, mousePos.y + 40);
                break;
            case Location.LowerLeft:
                tipWindow.transform.position = new Vector2(mousePos.x - tipWindow.sizeDelta.x * 0.6f, mousePos.y - 40);
                break;
            case Location.LowerRight:
                tipWindow.transform.position = new Vector2(mousePos.x + tipWindow.sizeDelta.x * 0.6f, mousePos.y - 40);
                break;
        }
    }

    private void HideTip()
    {
        tipText.text = default;
        tipWindow.gameObject.SetActive(false);
    }
}
