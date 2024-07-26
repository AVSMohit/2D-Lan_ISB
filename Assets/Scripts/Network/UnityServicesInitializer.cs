using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class UnityServicesInitializer : MonoBehaviour
{
    async void Start()
    {
        await InitializeUnityServices();
    }

    public static async Task InitializeUnityServices()
    {
        await UnityServices.InitializeAsync();
        await SignInAnonymously();
    }

    private static async Task SignInAnonymously()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in anonymously.");
        }
    }
}
