using System.Collections;
using TMPro;
using UnityEngine;

public class CoinsHUDText : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text coinsText;

    [Header("Formato")]
    [SerializeField] private bool padWithZeros = true; // 10 -> 010
    [SerializeField] private int padDigits = 3;        // 000
    [SerializeField] private string prefix = "$";      // pon "" si no quieres $

    private bool subscribed = false;

    private void Awake()
    {
        // Agarra automaticamente TextMeshPro o TextMeshProUGUI
        if (coinsText == null)
            coinsText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        StartCoroutine(Init());
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private IEnumerator Init()
    {
        // Si aun no encontro el TMP_Text, reintenta
        while (coinsText == null)
        {
            coinsText = GetComponent<TMP_Text>();
            yield return null;
        }

        // placeholder mientras carga
        SetText(0);

        // Espera a Firebase listo
        while (!FirebaseInitializer.IsReady)
            yield return null;

        // Espera a que exista el usuario logueado
        while (GameSession.Instance == null || GameSession.Instance.CurrentUser == null)
            yield return null;

        // Asegura que exista FirebaseCoinsManager
        if (FirebaseCoinsManager.Instance == null)
        {
            var go = new GameObject("FirebaseCoinsManager");
            go.AddComponent<FirebaseCoinsManager>();
            yield return null; // deja que Awake corra
        }

        // Bind al usuario actual
        FirebaseCoinsManager.Instance.BindUser(GameSession.Instance.CurrentUser.username);

        // Suscribete a cambios
        Subscribe();

        // Refresco inmediato (puede ser 0 al inicio, luego se actualiza cuando Firebase responda)
        SetText(FirebaseCoinsManager.Instance.Coins);
    }

    private void Subscribe()
    {
        if (subscribed) return;
        if (FirebaseCoinsManager.Instance == null) return;

        FirebaseCoinsManager.Instance.OnCoinsChanged += HandleCoinsChanged;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed) return;

        if (FirebaseCoinsManager.Instance != null)
            FirebaseCoinsManager.Instance.OnCoinsChanged -= HandleCoinsChanged;

        subscribed = false;
    }

    private void HandleCoinsChanged(int newCoins)
    {
        SetText(newCoins);
    }

    private void SetText(int value)
    {
        if (coinsText == null) return;

        if (value < 0) value = 0;

        string num = padWithZeros
            ? value.ToString().PadLeft(padDigits, '0')
            : value.ToString();

        coinsText.text = $"{prefix}{num}";
    }
}
