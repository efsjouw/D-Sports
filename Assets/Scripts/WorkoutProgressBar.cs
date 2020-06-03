using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class WorkoutProgressBar : MonoBehaviour
{
    public Slider slider;
    public float lerpTime = 0.99f;

    private float progressPercentage;
    private Image fillImage;
    private void Awake()
    {
        fillImage = slider.fillRect.GetComponent<Image>();
    }

    private void Update()
    {
        slider.value = Mathf.Lerp(slider.value, progressPercentage, lerpTime * Time.deltaTime);
    }

    public void setProgress(float percentage)
    {
        progressPercentage = percentage;
    }

    public float getProgress()
    {
        return progressPercentage;
    }
    
    public void showFill(bool toggle)
    {
        Color fillColor = fillImage.color;
        fillImage.color = new Color(fillColor.r, fillColor.g, fillColor.b, toggle ? 100 : 0);
    }
}
