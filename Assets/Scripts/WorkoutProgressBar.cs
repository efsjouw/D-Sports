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
    void Awake()
    {
        fillImage = slider.fillRect.GetComponent<Image>();
    }

    void Update()
    {        
        slider.value = Mathf.Lerp(slider.value, progressPercentage, lerpTime * Time.deltaTime);
    }

    public void setProgress(float percentage, bool showFilling = true)
    {
        showFill(showFilling);
        progressPercentage = percentage;
    }

    public float getProgress()
    {
        return progressPercentage;
    }
    
    private void showFill(bool toggle)
    {
        Color fillColor = fillImage.color;
        fillImage.color = new Color(fillColor.r, fillColor.g, fillColor.b, toggle ? 100 : 0);
    }
}
