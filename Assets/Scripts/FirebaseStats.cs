using System;
using Firebase.Database;
using UnityEngine;

public static class FirebaseStats
{
    private static DatabaseReference GetStatsRoot()
    {
        if (!FirebaseInitializer.IsReady) return null;

        // intenta por GameSession
        string username = "";
        if (GameSession.Instance != null && GameSession.Instance.CurrentUser != null)
            username = GameSession.Instance.Username;

        // fallback por FirebaseSession (prefs)
        if (string.IsNullOrEmpty(username))
        {
            FirebaseSession.LoadFromPrefs();
            username = FirebaseSession.Username;
        }

        if (string.IsNullOrEmpty(username)) return null;

        return FirebaseDatabase.DefaultInstance
            .RootReference
            .Child("users")
            .Child(username)
            .Child("stats");
    }

    public static void SetStat(string key, int value)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (value < 0) value = 0;

        var statsRoot = GetStatsRoot();
        if (statsRoot == null) return;

        statsRoot.Child(key).SetValueAsync(value);
    }

    public static void IncStat(string key, int delta = 1)
    {
        if (string.IsNullOrEmpty(key)) return;

        var statsRoot = GetStatsRoot();
        if (statsRoot == null) return;

        var statRef = statsRoot.Child(key);

        statRef.RunTransaction(mutable =>
        {
            long current = 0;
            if (mutable.Value != null)
            {
                try { current = Convert.ToInt64(mutable.Value); }
                catch { current = 0; }
            }

            long next = current + delta;
            if (next < 0) next = 0;

            mutable.Value = next;
            return TransactionResult.Success(mutable);
        });
    }
}
