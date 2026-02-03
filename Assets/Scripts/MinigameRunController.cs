using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Witchly.Mercado; // WalletFirebase

public class MinigameRunController : MonoBehaviour
{
    public static MinigameRunController Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Texto UI")]
    [SerializeField] private string scorePrefix = "Meteoritos: ";
    [SerializeField] private string timerPrefix = "Tiempo: ";

    [Header("Reglas")]
    [SerializeField] private int scoreLimit = 20;
    [SerializeField] private float durationSeconds = 45f;
    [SerializeField] private int rewardCoinsIfHitLimit = 30;

    [Tooltip("Si llega al limite antes de que acabe el tiempo, termina el minijuego al instante.")]
    [SerializeField] private bool endEarlyWhenHitLimit = true;

    [Header("Salida")]
    [SerializeField] private string returnScene = "PantallaInicial";
    [SerializeField] private float endDelay = 0.75f;

    // Estado
    private int score;
    private float timeLeft;
    private bool finished;

    // Recompensa robusta
    private int pendingRewardCoins = 0;
    private bool rewardResolved = false;

    // Llave PlayerPrefs por si el objeto se destruye inesperadamente
    private const string PREF_PENDING_REWARD = "MINIGAME_PENDING_REWARD";

    private void Awake()
    {
        // Singleton simple
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Importante: mantener vivo para poder aplicar recompensa al volver a PantallaInicial
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ResetRun();
        RefreshUI();
    }

    private void Update()
    {
        if (finished) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            Finish(score >= scoreLimit);
            return;
        }

        RefreshUI();
    }

    private void ResetRun()
    {
        score = 0;
        timeLeft = durationSeconds;
        finished = false;

        pendingRewardCoins = 0;
        rewardResolved = false;

        // limpia recompensa pendiente vieja
        PlayerPrefs.DeleteKey(PREF_PENDING_REWARD);
    }

    // ---- PUBLICO: lo llamas cuando recolectas algo ----
    public void AddScore(int amount = 1)
    {
        if (finished) return;

        score += Mathf.Max(0, amount);

        if (score >= scoreLimit)
        {
            score = scoreLimit;
            RefreshUI();

            if (endEarlyWhenHitLimit)
                Finish(true);
        }
        else
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (scoreText != null)
            scoreText.text = scorePrefix + score;

        if (timerText != null)
        {
            int t = Mathf.CeilToInt(timeLeft);
            timerText.text = timerPrefix + t + "s";
        }
    }

    private void Finish(bool success)
    {
        if (finished) return;
        finished = true;

        // Si gano, genero recompensa (pendiente)
        if (success)
        {
            pendingRewardCoins += rewardCoinsIfHitLimit;

            // backup por si algo raro pasa
            PlayerPrefs.SetInt(PREF_PENDING_REWARD, pendingRewardCoins);
            PlayerPrefs.Save();
        }

        StartCoroutine(ReturnToHomeAfterDelay());
    }

    private IEnumerator ReturnToHomeAfterDelay()
    {
        yield return new WaitForSeconds(endDelay);
        SceneManager.LoadScene(returnScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Solo intento aplicar recompensa cuando ya estoy de regreso
        if (scene.name != returnScene) return;

        // Recupera por si se perdio el valor en memoria
        if (pendingRewardCoins <= 0 && PlayerPrefs.HasKey(PREF_PENDING_REWARD))
            pendingRewardCoins = PlayerPrefs.GetInt(PREF_PENDING_REWARD, 0);

        // Ya no hay nada que dar
        if (pendingRewardCoins <= 0)
        {
            CleanupAndDie();
            return;
        }

        // Intento otorgar monedas en PantallaInicial (donde SI deberia existir el wallet)
        if (!rewardResolved)
        {
            bool ok = TryGiveCoinsNow(pendingRewardCoins);
            if (ok)
            {
                rewardResolved = true;
                pendingRewardCoins = 0;
                PlayerPrefs.DeleteKey(PREF_PENDING_REWARD);
                PlayerPrefs.Save();

                CleanupAndDie();
            }
            else
            {
                // Si por alguna razon el wallet aun no existe, reintento unos frames
                StartCoroutine(RetryGiveCoins());
            }
        }
    }

    private IEnumerator RetryGiveCoins()
    {
        float timeout = 3f; // 3s max
        float t = 0f;

        while (t < timeout && !rewardResolved)
        {
            t += Time.deltaTime;

            if (pendingRewardCoins > 0 && TryGiveCoinsNow(pendingRewardCoins))
            {
                rewardResolved = true;
                pendingRewardCoins = 0;
                PlayerPrefs.DeleteKey(PREF_PENDING_REWARD);
                PlayerPrefs.Save();

                CleanupAndDie();
                yield break;
            }

            yield return null;
        }

        Debug.LogWarning("[Minigame] No se pudo otorgar recompensa (WalletFirebase no encontrado).");
        CleanupAndDie();
    }

    private bool TryGiveCoinsNow(int amount)
    {
        if (amount <= 0) return true;

        WalletFirebase wallet = null;

        // Prioridad: singleton si existe
        if (WalletFirebase.Instance != null)
            wallet = WalletFirebase.Instance;

        // Fallback
        if (wallet == null)
            wallet = FindObjectOfType<WalletFirebase>();

        if (wallet == null)
            return false;

        wallet.AddCoins(amount);
        Debug.Log("[Minigame] Reward coins: +" + amount + " | Total: " + wallet.Coins);
        return true;
    }

    private void CleanupAndDie()
    {
        // limpia singleton
        if (Instance == this) Instance = null;

        // evita que se quede vivo en DontDestroyOnLoad
        Destroy(gameObject);
    }
}
