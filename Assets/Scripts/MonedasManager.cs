using UnityEngine;
using System;

public class MonedasManager : MonoBehaviour
{
    public static MonedasManager Instance { get; private set; }

    private const string KEY_COINS = "PlayerCoins";
    [SerializeField] private int monedas = 0;

  
    public delegate void OnCoinsChanged(int newCoins);
    public static event OnCoinsChanged CambioMonedas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CargarMonedas();
            CambioMonedas?.Invoke(monedas);
        }
        else
        {
            Destroy(gameObject);
        }
    }

  
    public int GetMonedas() => monedas;

    public void AnadirMonedas(int amount)
    {
        if (amount <= 0) return;

        monedas += amount;
        GuardarMonedas();
        CambioMonedas?.Invoke(monedas);
    }

    public bool GastarMonedas(int amount)
    {
        if (amount <= 0) return false;

        if (monedas < amount)
            return false;

        monedas -= amount;
        GuardarMonedas();
        CambioMonedas?.Invoke(monedas);
        return true;
    }

    public void ResetMonedas()
    {
        monedas = 0;
        GuardarMonedas();
        CambioMonedas?.Invoke(monedas);
    }

    private void GuardarMonedas()
    {
        PlayerPrefs.SetInt(KEY_COINS, monedas);
        PlayerPrefs.Save();
    }

    private void CargarMonedas()
    {
        monedas = PlayerPrefs.GetInt(KEY_COINS, 0);
    }

  
    public void AddCoins(int amount) => AnadirMonedas(amount);
    public bool RemoveCoins(int amount) => GastarMonedas(amount);
    public int Coins => monedas;
}
