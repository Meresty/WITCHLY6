using System;
using System.Threading.Tasks;
using UnityEngine;

public class WalletFirebase : MonoBehaviour
{
    public static WalletFirebase Instance { get; private set; }

    public int Coins { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("WalletFirebase.Awake");
        // compat: intenta recuperar username si el juego arranca directo a una escena
        FirebaseSession.LoadFromPrefs();
    }

    private void OnEnable()
    {
        EnsureBind();
        HookEvents();  
        SyncLocal();
    }

    private void OnDisable()
    {
        if (FirebaseCoinsManager.Instance != null)
            FirebaseCoinsManager.Instance.OnCoinsChanged -= OnCoinsChanged;
    }

    private void EnsureBind()
    {
        if (FirebaseCoinsManager.Instance == null) return;
        if (!FirebaseInitializer.IsReady) return;

        string username = "";

        if (GameSession.Instance != null && GameSession.Instance.CurrentUser != null)
            username = GameSession.Instance.Username;

        if (string.IsNullOrEmpty(username))
        {
            FirebaseSession.LoadFromPrefs();
            username = FirebaseSession.Username;
        }

        if (!string.IsNullOrEmpty(username))
            FirebaseCoinsManager.Instance.BindUser(username);
    }

    private void HookEvents()
    {
        if (FirebaseCoinsManager.Instance == null) return;

        FirebaseCoinsManager.Instance.OnCoinsChanged -= OnCoinsChanged;
        FirebaseCoinsManager.Instance.OnCoinsChanged += OnCoinsChanged;
    }

    private void SyncLocal()
    {
        if (FirebaseCoinsManager.Instance == null) return;
        Coins = FirebaseCoinsManager.Instance.Coins;
    }

    private void OnCoinsChanged(int value)
    {
        Coins = value;
    }

    // API "vieja" para que tus botones y UI sigan funcionando
    public void AddCoins(int amount, Action<bool> done = null)
    {
        if (amount <= 0) { done?.Invoke(false); return; }
        EnsureBind();

        if (FirebaseCoinsManager.Instance == null) { done?.Invoke(false); return; }

        var task = FirebaseCoinsManager.Instance.AddCoinsAsync(amount);
        task.ContinueWith(t =>
        {
            bool ok = (t.Status == TaskStatus.RanToCompletion) && t.Result;
            done?.Invoke(ok);
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public void TrySpend(int cost, Action<bool> done)
    {
        if (cost <= 0) { done?.Invoke(true); return; }
        EnsureBind();

        if (FirebaseCoinsManager.Instance == null) { done?.Invoke(false); return; }

        var task = FirebaseCoinsManager.Instance.TrySpendCoinsAsync(cost);
        task.ContinueWith(t =>
        {
            bool ok = (t.Status == TaskStatus.RanToCompletion) && t.Result;
            done?.Invoke(ok);
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }
}
