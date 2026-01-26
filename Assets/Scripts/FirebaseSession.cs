using UnityEngine;
using Firebase.Database;

public static class FirebaseSession
{
    public static string Username { get; private set; } = "";

    public static bool IsLoggedIn => !string.IsNullOrEmpty(Username);

    public static DatabaseReference UserRef
    {
        get
        {
            if (!FirebaseInitializer.IsReady) return null;
            if (string.IsNullOrEmpty(Username)) return null;

            return FirebaseDatabase.DefaultInstance
                .RootReference
                .Child("users")
                .Child(Username);
        }
    }

    public static void SetUsername(string username)
    {
        Username = string.IsNullOrEmpty(username) ? "" : username.Trim();

        if (!string.IsNullOrEmpty(Username))
        {
            PlayerPrefs.SetString("last_username", Username);
            PlayerPrefs.Save();
        }
    }

    public static void Clear()
    {
        Username = "";
        PlayerPrefs.DeleteKey("last_username");
        PlayerPrefs.Save();
    }

    public static void LoadFromPrefs()
    {
        if (!string.IsNullOrEmpty(Username)) return;

        var u = PlayerPrefs.GetString("last_username", "");
        if (!string.IsNullOrEmpty(u))
            Username = u.Trim();
    }
}
