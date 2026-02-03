using UnityEngine;

// Ajusta los using si tu WalletFirebase vive en un namespace
using Witchly.Mercado;

public static class CoinsRewarder
{
    public static void AddCoins(int amount)
    {
        if (amount <= 0) return;

        // 1) Intenta WalletFirebase (lo que ya usas en Buzon)
        WalletFirebase wallet = null;

        if (WalletFirebase.Instance != null)
            wallet = WalletFirebase.Instance;
        else
            wallet = Object.FindObjectOfType<WalletFirebase>();

        if (wallet != null)
        {
            wallet.AddCoins(amount);
            Debug.Log("[COINS] Reward +" + amount + " | Total now: " + wallet.Coins);
            return;
        }

        Debug.LogWarning("[COINS] No encontre WalletFirebase en escena/DontDestroyOnLoad. No se pudo otorgar recompensa.");
    }
}
