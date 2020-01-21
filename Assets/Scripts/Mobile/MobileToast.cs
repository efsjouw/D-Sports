using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MobileToast : MonoBehaviour
{
    //TODO: Toast stack?
    public List<Toast> toasts;
    public float showSeconds = 3;

    [Serializable]
    public class Toast
    {
        public RectTransform toastObject;
        public TMP_Text toastText;
    }

    void Start()
    {
        hide();
    }

    public void show(string message, int index = 0)
    {
        if(!toasts[index].toastObject.gameObject.activeSelf)
        {
            toasts[index].toastText.text = message;
            StartCoroutine(showRoutine(toasts[index].toastObject.gameObject));
        }        
    }

    public void hide(int index = 0)
    {
        toasts[index].toastObject.gameObject.SetActive(false);
    }

    IEnumerator showRoutine(GameObject toastObject)
    {
        toastObject.SetActive(true);
        yield return new WaitForSeconds(showSeconds);
        toastObject.SetActive(false);
    }
}
