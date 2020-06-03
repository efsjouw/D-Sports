using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Set a TMP_Text to time format text
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class Timetext : MonoBehaviour
{
    [HideInInspector]
    public TMP_Text tmp;

    void Start()
    {
        tmp = GetComponent<TMP_Text>();
    }

    public void setTimeText(int seconds)
    {
        if(tmp == null) tmp = GetComponent<TMP_Text>();
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        string format = string.Format("{0:D2} : {1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        tmp.text = format;
    }

    public void setText(string text)
    {
        if (tmp == null) tmp = GetComponent<TMP_Text>();
        tmp.text = text;
    }

}
