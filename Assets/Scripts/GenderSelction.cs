using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenderSelction : MonoBehaviour
{
    [SerializeField] Button joinButton;

    private void Start()
    {
        joinButton.interactable = false;
    }
    public void SelectedMale()
    {
        PlayerPrefs.SetString("Gender", "male");
        joinButton.interactable = true;
    }

    public void SelectedFemale()
    {
        PlayerPrefs.SetString("Gender", "female");
        joinButton.interactable = true;
    }
}
