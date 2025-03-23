using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    private Dictionary<string, string> roomCodeToRelayCode = new Dictionary<string, string>();
    public static string ActiveRoomCode { get; set; } // Tracks the current session code


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddRoom(string roomCode, string relayCode)
    {
        if (!roomCodeToRelayCode.ContainsKey(roomCode))
        {
            roomCodeToRelayCode[roomCode] = relayCode;
        }
    }

    public string GetRelayJoinCode(string roomCode)
    {
        return roomCodeToRelayCode.ContainsKey(roomCode) ? roomCodeToRelayCode[roomCode] : null;
    }

    public bool DoesRoomExist(string roomCode)
    {
        return roomCodeToRelayCode.ContainsKey(roomCode);
    }
    public void RemoveRoom(string roomCode)
    {
        if (roomCodeToRelayCode.ContainsKey(roomCode))
        {
            Debug.Log($"Removing room: {roomCode}");
            roomCodeToRelayCode.Remove(roomCode);
        }
    }

}
