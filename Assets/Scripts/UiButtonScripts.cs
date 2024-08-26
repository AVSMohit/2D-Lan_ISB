using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiButtonScripts : MonoBehaviour 
{
    Vector3 initialScale;
    public Vector3 hoverScale;
    public float scaleTime;
    TMP_Text childText;


    private void Start()
    {
        initialScale = transform.localScale;
        childText = GetComponentInChildren<TMP_Text>();
    }

    public void OnHoverEnter()
    {
        transform.localScale = Vector3.Lerp(initialScale, hoverScale, scaleTime  * Time.deltaTime);
        childText.color = this.GetComponent<Button>().colors.normalColor;
        
    }
    
    public void OnHoverExit()
    {
        transform.localScale = Vector3.Lerp(hoverScale, initialScale, scaleTime  * Time.deltaTime);
        childText.color = this.GetComponent<Button>().colors.highlightedColor;
    }
}
