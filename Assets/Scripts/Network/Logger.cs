using System;
using System.IO;
using UnityEngine;
using Photon.Pun;

public class Logger : MonoBehaviourPun
{
    private static string logFilePath = Path.Combine(Application.persistentDataPath, "game_log.txt");

    public static void Log(string message)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
            Debug.Log(message);

            // Send log to all clients for synchronized logging (only when called from a PhotonView)
            if (PhotonNetwork.InRoom)
            {
                PhotonView photonView = PhotonView.Find(PhotonNetwork.LocalPlayer.ActorNumber);
                if (photonView != null)
                {
                    photonView.RPC("BroadcastLogMessage", RpcTarget.Others, message);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write to log file: {ex.Message}");
        }
    }

    [PunRPC]
    private void BroadcastLogMessage(string message)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now} [Remote]: {message}");
            }
            Debug.Log($"[Remote Log] {message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write remote log to file: {ex.Message}");
        }
    }
}
