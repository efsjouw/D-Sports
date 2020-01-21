using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Canvas))]
public class SubPanelNavigator : PanelNavigatorBase
{
    //Sub panel navigator, multiple in scene

    private void OnEnable()
    {
        PanelNavigator.Instance.currentSubPanelNavigator = this;  
    }

    private void OnDisable()
    {
        PanelNavigator.Instance.currentSubPanelNavigator = null;
    }
}
