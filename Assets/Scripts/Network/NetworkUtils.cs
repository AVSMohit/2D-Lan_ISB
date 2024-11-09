using System.Net;
using System.Net.Sockets;
using UnityEngine;

public static class NetworkUtils
{
    public static string GetLocalIPAddress()
    {
#if !UNITY_WEBGL
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new System.Exception("No network adapters with an IPv4 address in the system!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to get local IP address: {ex.Message}");
            return "IP not available";
        }
#else
        Debug.LogWarning("GetLocalIPAddress is not supported on WebGL.");
        return "WebGL does not support local IP retrieval";
#endif
    }
}
