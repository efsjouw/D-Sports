using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Canvas))]
public class PanelNavigator : PanelNavigatorBase
{
    private static PanelNavigator instance = null;
    public static PanelNavigator Instance
    {
        get
        {
            if (instance == null) instance = (PanelNavigator)FindObjectOfType(typeof(PanelNavigator));
            return instance;
        }
    }    
}
