using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using Unity.VisualScripting;

public class FirebaseCoinsManager : MonoBehaviour
{
    public static FirebaseCoinsManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private int defaultCoinsOnNewUser = 10;

    public int Coins { get; private set; }
    public event Action<int> OnCoinsChanged;

    private string _username;
    private DatabaseReference _coinsRef;
    private bool _listening;

    public bool IsBound => _coinsRef != null;

    private void Awake()
    {
        Debug.Log("FirebaseCoinsManager Awake");
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        TryAutoBindFromGameSession();
    }

    private void OnDestroy()
    {
        Debug.Log("FirebaseCoinsManager Destroyed");

        if (Instance == this)
        {
            Unbind();
            Instance = null;
        }
    }

    public void BindUser(string username)
    {
        if (string.IsNullOrEmpty(username)) return;
        if (!FirebaseInitializer.IsReady) return;

        username = username.Trim();

        if (_coinsRef != null && _username == username) return;

        Unbind();

        _username = username;

        _coinsRef = FirebaseDatabase.DefaultInstance
            .RootReference
            .Child("users")
            .Child(_username)
            .Child("coins");

        _coinsRef.ValueChanged += OnCoinsValueChanged;
        _listening = true;

        _coinsRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("[COINS] Read error: " + task.Exception);
                return;
            }

            var snap = task.Result;

            if (snap == null || !snap.Exists || snap.Value == null)
            {
                _coinsRef.SetValueAsync(defaultCoinsOnNewUser);
                SetLocalCoins(defaultCoinsOnNewUser);
            }
            else
            {
                SetLocalCoins(SafeToInt(snap.Value));
            }

            Debug.Log("[COINS] Bound user: " + _username);
        });
    }

    public void Unbind()
    {
        if (_coinsRef != null && _listening)
        {
            _coinsRef.ValueChanged -= OnCoinsValueChanged;
        }

        _listening = false;
        _coinsRef = null;
        _username = null;
    }

    private void OnCoinsValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.LogWarning("[COINS] ValueChanged error: " + e.DatabaseError.Message);
            return;
        }

        if (e.Snapshot == null || !e.Snapshot.Exists || e.Snapshot.Value == null)
        {
            SetLocalCoins(0);
            return;
        }

        SetLocalCoins(SafeToInt(e.Snapshot.Value));
    }

    private void SetLocalCoins(int value)
    {
        if (value < 0) value = 0;
        Coins = value;
        OnCoinsChanged?.Invoke(Coins);
    }

    private int SafeToInt(object val)
    {
        try
        {
            if (val is long l) return (int)l;
            if (val is int i) return i;
            if (val is double d) return (int)d;
            return Convert.ToInt32(val);
        }
        catch
        {
            return 0;
        }
    }

    public Task<bool> AddCoinsAsync(int amount)
    {
        amount = Mathf.Abs(amount);
        if (amount == 0) return Task.FromResult(true);
        return RunCoinsDeltaAsync(+amount, requireNonNegative: false);
    }

    public Task<bool> TrySpendCoinsAsync(int amount)
    {
        amount = Mathf.Abs(amount);
        if (amount == 0) return Task.FromResult(true);
        return RunCoinsDeltaAsync(-amount, requireNonNegative: true);
    }

    public Task<bool> RunCoinsDeltaAsync(int delta, bool requireNonNegative = false)
    {
        var tcs = new TaskCompletionSource<bool>();

        if (!FirebaseInitializer.IsReady) { tcs.SetResult(false); return tcs.Task; }
        if (_coinsRef == null) { tcs.SetResult(false); return tcs.Task; }

        long desiredNext = long.MinValue;

        _coinsRef.RunTransaction(mutableData =>
        {
            long current = 0;

            if (mutableData.Value != null)
            {
                try { current = Convert.ToInt64(mutableData.Value); }
                catch { current = 0; }
            }

            long next = current + delta;

            if (requireNonNegative && next < 0)
            {
                desiredNext = long.MinValue;
                return TransactionResult.Abort();
            }

            if (next < 0) next = 0;

            desiredNext = next;
            mutableData.Value = next;
            return TransactionResult.Success(mutableData);

        }).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                tcs.SetResult(false);
                return;
            }

            var snap = task.Result;
            if (snap == null || !snap.Exists || snap.Value == null)
            {
                tcs.SetResult(false);
                return;
            }

            long finalVal = 0;
            try { finalVal = Convert.ToInt64(snap.Value); } catch { finalVal = 0; }

            bool committed = (desiredNext != long.MinValue) && (finalVal == desiredNext);

            if (committed) SetLocalCoins((int)finalVal);

            tcs.SetResult(committed);
        });

        return tcs.Task;
    }

    private void TryAutoBindFromGameSession()
    {
        if (GameSession.Instance != null && GameSession.Instance.CurrentUser != null)
        {
            BindUser(GameSession.Instance.CurrentUser.username);
        }
    }
}
