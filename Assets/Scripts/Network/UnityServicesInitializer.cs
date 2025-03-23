using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using Unity.Services.Core.Environments;

public static class UnityServicesInitializer
{
    private static TaskCompletionSource<bool> _initTask = new TaskCompletionSource<bool>();
    private static bool _hasStarted = false;

    public static async Task InitializeUnityServices()
    {
        if (_hasStarted)
            return;

        _hasStarted = true;

        try
        {
            Debug.Log("⏳ Initializing Unity Services...");
            var options = new InitializationOptions();
            options.SetEnvironmentName("production"); // Optional, if using Environments

            await UnityServices.InitializeAsync(options);
            Debug.Log("✅ Unity Services initialized.");

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("✅ Signed in anonymously.");

            _initTask.TrySetResult(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Unity Services Init Failed: " + e.Message);
            _initTask.TrySetException(e);
        }
    }

    public static async Task WaitForInitialization()
    {
        // Trigger init if not already started
        if (!_hasStarted)
        {
            await InitializeUnityServices();
        }

        await _initTask.Task;
    }
}
