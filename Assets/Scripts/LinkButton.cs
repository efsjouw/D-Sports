using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LinkButton : MonoBehaviour
{
    public string baseURL = "https://duckduckgo.com/?q=";

    [HideInInspector] public Button button;
    private string url;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void appendToURL(string append)
    {
        url = baseURL + append;
    }

    public void openUrl()
    {
        Application.OpenURL(url);
    }
}
