using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PressurePadManager : NetworkBehaviour
{
    public PressurePads[] pressurePads;
    private List<int> correctSequence = new List<int> { 13, 7, 19, 10, 16, 14, 1, 8 };
    private int currentStep = 0;
    public PressurePadInstructionScreenManager instructionScreenManager;

    private Dictionary<ulong, List<int>> playerPadAssignments = new Dictionary<ulong, List<int>>();
    private Dictionary<ulong, FixedString32Bytes> playerNames = new Dictionary<ulong, FixedString32Bytes>();

    public static PressurePadManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            AssignPadsToPlayers();
            SendInstructionsToClients();
            SetPadSequence();
        }
    }

    private void AssignPadsToPlayers()
    {
        List<ulong> connectedPlayers = new List<ulong>(NetworkManager.Singleton.ConnectedClients.Keys);

        // Define specific pad assignments for each player
        List<int> player0Pads = new List<int> { 1, 5, 9, 13, 17 };
        List<int> player1Pads = new List<int> { 2, 6, 10, 14, 18 };
        List<int> player2Pads = new List<int> { 3, 7, 11, 15, 19 };
        List<int> player3Pads = new List<int> { 4, 8, 12, 16, 20 };

        // Assign specific pads to players based on their index
        if (connectedPlayers.Count > 0)
        {
            playerPadAssignments[connectedPlayers[0]] = player0Pads;
        }
        if (connectedPlayers.Count > 1)
        {
            playerPadAssignments[connectedPlayers[1]] = player1Pads;
        }
        if (connectedPlayers.Count > 2)
        {
            playerPadAssignments[connectedPlayers[2]] = player2Pads;
        }
        if (connectedPlayers.Count > 3)
        {
            playerPadAssignments[connectedPlayers[3]] = player3Pads;
        }

        RetrievePlayerNames();
    }

    private void RetrievePlayerNames()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = client.Key;
            var playerName = client.Value.PlayerObject.GetComponent<PlayerName>();
            if (playerName != null)
            {
                playerNames[clientId] = new FixedString32Bytes(playerName.playerName.Value);
            }
        }
    }

    private void SendInstructionsToClients()
    {
        List<FixedString32Bytes> playerNamesList = new List<FixedString32Bytes>();
        List<FixedString32Bytes> padAssignmentsList = new List<FixedString32Bytes>();

        foreach (var assignment in playerPadAssignments)
        {
            ulong clientId = assignment.Key;
            if (playerNames.TryGetValue(clientId, out FixedString32Bytes fixedPlayerName))
            {
                playerNamesList.Add(fixedPlayerName);
                string assignedPads = string.Join(", ", assignment.Value);
                padAssignmentsList.Add(new FixedString32Bytes(assignedPads));
            }
        }

        ShowInstructionsClientRpc(playerNamesList.ToArray(), padAssignmentsList.ToArray());
    }

    [ClientRpc]
    private void ShowInstructionsClientRpc(FixedString32Bytes[] playerNames, FixedString32Bytes[] padAssignments)
    {
        string[] playerNamesArray = new string[playerNames.Length];
        string[] padAssignmentsArray = new string[padAssignments.Length];

        // Convert FixedString32Bytes back to strings
        for (int i = 0; i < playerNames.Length; i++)
        {
            playerNamesArray[i] = playerNames[i].ToString();
            padAssignmentsArray[i] = padAssignments[i].ToString();
        }

        instructionScreenManager.SetInstructions(playerNamesArray, padAssignmentsArray);
    }

    private void SetPadSequence()
    {
        foreach (PressurePads pad in pressurePads)
        {
            // Check if the pad number is part of the correct sequence
            if (correctSequence.Contains(pad.padNumber))
            {
                pad.isPartOfSequence = true;
                pad.sequenceIndex = correctSequence.IndexOf(pad.padNumber);  // Set the sequence index
            }
            else
            {
                pad.isPartOfSequence = false;
            }
        }
    }

    public bool CheckPadSequence(int padNumber)
    {
        // Check if the pad number matches the expected pad in the sequence
        if (padNumber == correctSequence[currentStep])
        {
            currentStep++;
            if (currentStep == correctSequence.Count)
            {
                Debug.Log("Sequence completed!");
                // You can trigger a win condition or transition to the next phase here
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    [ClientRpc]
    public void ResetPadsClientRpc()
    {
        currentStep = 0;
        foreach (PressurePads pad in pressurePads)
        {
            pad.ResetPad();  // Reset each pad
        }
        Debug.Log("All pads have been reset.");
    }

    public void OnPadActivated(int padNumber)
    {
        Debug.Log($"Pad {padNumber} was correctly activated.");
        // Additional logic for when a pad is correctly activated can be added here
    }
}
