using System;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseInventoryManager : MonoBehaviour
{
    public static FirebaseInventoryManager Instance { get; private set; }

    private string _username;
    private DatabaseReference _itemsRef;

    public bool IsBound => _itemsRef != null;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Auto-bind si ya existe un usuario en GameSession
        TryAutoBindFromGameSession();
    }

    public void BindUser(string username)
    {
        if (!FirebaseInitializer.IsReady)
        {
            Debug.LogWarning("[INV-FB] Firebase not ready. Cannot bind yet.");
            return;
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            Debug.LogWarning("[INV-FB] BindUser: empty username.");
            return;
        }

        _username = username.Trim();

        _itemsRef = FirebaseDatabase.DefaultInstance
            .RootReference
            .Child("users")
            .Child(_username)
            .Child("inventory")
            .Child("items");

        Debug.Log("[INV-FB] BindUser OK: " + _username);
    }

    public void Unbind()
    {
        _username = null;
        _itemsRef = null;
    }

    public Task<bool> AddItemAsync(string itemId, int delta)
    {
        var tcs = new TaskCompletionSource<bool>();

        if (!FirebaseInitializer.IsReady || _itemsRef == null)
        {
            Debug.LogWarning("[INV-FB] AddItemAsync: not ready or not bound.");
            tcs.SetResult(false);
            return tcs.Task;
        }

        if (string.IsNullOrEmpty(itemId) || delta == 0)
        {
            tcs.SetResult(false);
            return tcs.Task;
        }

        itemId = SanitizeKey(itemId);
        var itemRef = _itemsRef.Child(itemId);

        itemRef.RunTransaction(mutable =>
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

        }).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("[INV-FB] Transaction failed: " + task.Exception);
                tcs.SetResult(false);
                return;
            }

            tcs.SetResult(true);
        });

        return tcs.Task;
    }

    private void TryAutoBindFromGameSession()
    {
        if (!FirebaseInitializer.IsReady) return;

        if (GameSession.Instance != null && GameSession.Instance.CurrentUser != null)
        {
            BindUser(GameSession.Instance.CurrentUser.username);
        }
    }

    private string SanitizeKey(string s)
    {
        // Prohibidos: . # $ [ ] /
        return s.Replace(".", "_")
                .Replace("#", "_")
                .Replace("$", "_")
                .Replace("[", "_")
                .Replace("]", "_")
                .Replace("/", "_")
                .Trim();
    }
}
