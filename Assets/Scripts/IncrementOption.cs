using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class IncrementOption : MonoBehaviour
{
    public class OnClickEvent : UnityEvent<int> { }
    public OnClickEvent onValueChanged = new OnClickEvent();

    public TMP_Text titleTMP;
    public TMP_Text increment;

    private int increments;
    private int value;

    public void init(string title, int increments)
    {
        titleTMP.text = title;
        this.increments = increments;
        setValue(increments);
    }

    private void setValue(int value)
    {
        if (value > 0 && value >= increments)
        {
            this.value = value;
            increment.text = value.ToString();
            onValueChanged.Invoke(this.value);
        }
    }
    
    public void increase()
    {     
        setValue(value + increments);
    }

    public void decrease()
    {
        setValue(value - increments);        
    }
}
