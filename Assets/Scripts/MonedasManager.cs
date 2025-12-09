using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonedasManager : MonoBehaviour
{
    public static MonedasManager Instance { get; private set; }

    private int monedas = 0;

    public delegate void OnCoinsChanged(int newCoins);
    public static event OnCoinsChanged CambioMonedas;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ActualizarMonedas();
        }
        else
        {
            Destroy(gameObject);
        }
    } 


    public int GetMonedas()
    {
        return monedas;
    }


    public void AñadirMonedas(int amount)
    {
        monedas += amount;
        GuardarMonedas();
        CambioMonedas?.Invoke(monedas);
    }

    public bool GastarMonedas(int amount)
    {
        if (monedas >= amount)
        {
            monedas -= amount;
            GuardarMonedas();
            CambioMonedas?.Invoke(monedas);
            return true;
        }
        return false;
    }


    private void GuardarMonedas()
    {
        PlayerPrefs.SetInt("PlayerCoins", monedas);
        PlayerPrefs.Save();
    }

    private void ActualizarMonedas()
    {
        monedas = PlayerPrefs.GetInt("PlayerCoins", 0);
    }


    public void ResetMonedas()
    {
        monedas = 0;
        ActualizarMonedas();
        CambioMonedas?.Invoke(monedas);
    }
}