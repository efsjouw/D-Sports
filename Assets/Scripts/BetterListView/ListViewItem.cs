using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ListViewItem : MonoBehaviour
{
    public Button button;
    [HideInInspector] public ListViewDataItem data;

    public virtual void init(ListViewDataItem data)
    {
        this.data = data;
    }

    public virtual void doStartAnimation()
    {
    }
}
