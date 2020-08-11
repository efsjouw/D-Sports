using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonPressAnimation : MonoBehaviour
{
    [SerializeField] float pressValue = 0.15f;
    [SerializeField] float animationTime = 0.15f;

    Button button;

    void Start()
    {
        if(button == null) button = GetComponent<Button>();
        button.onClick.AddListener(playPressAnimation);
    }

    private void OnEnable()
    {
        if (button == null) button = GetComponent<Button>();
        button.interactable = true;
    }

    private void playPressAnimation()
    {
        button.interactable = false;
        StartCoroutine(pressAnimationRoutine());
    }

    private IEnumerator pressAnimationRoutine()
    {
        button.gameObject.transform.DOPunchScale(new Vector3(-pressValue, -pressValue), animationTime);
        yield return new WaitForSeconds(animationTime);
        button.interactable = true;
    }
}
