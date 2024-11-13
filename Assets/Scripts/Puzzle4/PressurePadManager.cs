using System;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class PressurePadManager : MonoBehaviourPun
{
    public PressurePads[] pressurePads;
    private List<int> correctSequence = new List<int> { 13, 7, 19, 10, 16, 14, 1, 8 };
    private int currentStep = 0;
    public PressurePadInstructionScreenManager instructionScreenManager;
    private Dictionary<int, List<int>> playerPadAssignments = new Dictionary<int, List<int>>();

    public static PressurePadManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional, if you want it to persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            AssignPadsToPlayers();
            photonView.RPC("SendInstructionsToClients", RpcTarget.All);
            SetPadSequence();
        }
    }

    private void AssignPadsToPlayers()
    {
        // Define specific pad assignments for each player and store them in playerPadAssignments dictionary
    }

    [PunRPC]
    private void SendInstructionsToClients()
    {
        // Trigger instruction screen on all clients
        instructionScreenManager.SetInstructions(new string[] { /* player names */ }, new string[] { /* pad assignments */ });
    }

    private void SetPadSequence()
    {
        foreach (PressurePads pad in pressurePads)
        {
            if (correctSequence.Contains(pad.padNumber))
            {
                pad.isPartOfSequence = true;
                pad.sequenceIndex = correctSequence.IndexOf(pad.padNumber);
            }
            else
            {
                pad.isPartOfSequence = false;
            }
        }
    }

    public bool CheckPadSequence(int padNumber)
    {
        if (padNumber == correctSequence[currentStep])
        {
            currentStep++;
            if (currentStep == correctSequence.Count)
            {
                SceneTransitionManager.Instance.TransitionToScene("Puzzle5");
                return true;
            }
            return true;
        }
        return false;
    }

    [PunRPC]
    public void ResetPads()
    {
        currentStep = 0;
        foreach (PressurePads pad in pressurePads)
        {
            pad.ResetPad();
        }
    }

    public void OnPadActivated(int padNumber)
    {
        Debug.Log($"Pad {padNumber} was correctly activated.");
    }

    public static implicit operator PressurePadManager(PressurePads v)
    {
        throw new NotImplementedException();
    }
}
