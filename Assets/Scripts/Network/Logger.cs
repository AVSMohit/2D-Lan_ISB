using System;
using System.IO;
using UnityEngine;

public static class Logger
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
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write to log file: {ex.Message}");
        }
    }
}
