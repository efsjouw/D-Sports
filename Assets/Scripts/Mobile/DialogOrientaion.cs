using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogOrientaion : MonoBehaviour
{
    public MobileDialog mobileDialog;

    private void OnEnable()
    {
        mobileDialog.determineOrientation();
    }
}
