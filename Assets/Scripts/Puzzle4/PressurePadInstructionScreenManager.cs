using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PressurePadInstructionScreenManager : MonoBehaviour
{
    public GameObject instructionScreen;
    public TMP_Text instructionsText;
    public Button startButton;
    public GameObject videoPlayer;
    public GameObject rawImage;

    private void Start()
    {
        instructionScreen.SetActive(true);
        videoPlayer.SetActive(false);
        rawImage.SetActive(false);
        startButton.onClick.AddListener(StartPuzzle);
      
        startButton.onClick.AddListener(GlobaGameManager.Instance.ResumeTimer);


    }

   

    public void SetInstructions(string[] playerNames, string[] padAssignments)
    {
        instructionsText.text = "Pressure Pads Assigned:\n";
        for (int i = 0; i < playerNames.Length; i++)
        {
            instructionsText.text += $"{playerNames[i]}: Pads {padAssignments[i]}\n";
        }

        instructionScreen.SetActive(true);  // Show the instruction screen
    }


    void StartPuzzle()
    {
        instructionScreen.SetActive(false);
        rawImage.SetActive(true );
        videoPlayer.SetActive(true);
        StartCoroutine(HideVideoAfterPlaying());
    }

    IEnumerator HideVideoAfterPlaying()
    {
        yield return new WaitForSeconds((float)videoPlayer.GetComponent<VideoPlayer>().clip.length + 2);
        rawImage.SetActive(false);
        videoPlayer.SetActive(false);
    }

}
