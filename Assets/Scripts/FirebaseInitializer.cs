using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseInitializer : MonoBehaviour
{
    public static bool IsReady { get; private set; }
    public static bool Ready => IsReady; // alias por compatibilidad con scripts viejos
    public static DependencyStatus LastStatus { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitFirebase();
    }

    private void InitFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                LastStatus = task.Result;

                if (LastStatus == DependencyStatus.Available)
                {
                    IsReady = true;
                    Debug.Log("Firebase listo.");
                }
                else
                {
                    IsReady = false;
                    Debug.LogError($"No se pudieron resolver dependencias de Firebase: {LastStatus}");
                }
            });
    }
}
