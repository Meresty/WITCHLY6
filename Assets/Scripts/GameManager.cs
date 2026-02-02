using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referencias de Sistemas (opcionales)")]
    public InventorySystem inventorySystem;
    public BarraEnergiaSistema energyBarSystem;
    public InvernaderoManager greenhouseManager;
    public InventoryUI inventoryUI;
    public CajaCalidadUI qualityBoxUI;

    [Header("UI Principal")]
    public TextMeshProUGUI debugText;
    public Button resetGameButton;

    [Header("Progreso")]
    [SerializeField] private int completedCards = 0;
    [SerializeField] private int qualityPotionsCompleted = 0;

    // Si otro sistema necesita reaccionar a cambios de progreso
    public event Action<int, int> OnProgressChanged; // (completedCards, qualityPotionsCompleted)

    private const string K_CARDS = "CompletedCards";
    private const string K_QPOTIONS = "QualityPotions";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSystems();
        LoadProgress();
    }

    private void Start()
    {
        if (resetGameButton != null)
            resetGameButton.onClick.AddListener(ResetGame);

        NotifyProgressChanged();
    }

    private void Update()
    {
        if (debugText != null)
            UpdateDebugInfo();
    }

    private void InitializeSystems()
    {
        if (inventorySystem == null)
            inventorySystem = FindObjectOfType<InventorySystem>();

        if (energyBarSystem == null)
            energyBarSystem = FindObjectOfType<BarraEnergiaSistema>();

        if (greenhouseManager == null)
            greenhouseManager = FindObjectOfType<InvernaderoManager>();

        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<InventoryUI>();

        if (qualityBoxUI == null)
            qualityBoxUI = FindObjectOfType<CajaCalidadUI>();
    }

    // =========================
    // SISTEMA DE CARTAS
    // =========================
    public void CompleteCard()
    {
        completedCards++;
        SaveProgress();
        NotifyProgressChanged();

        Debug.Log($"[GameManager] Carta completada. Total: {completedCards}");
    }

    public int GetCompletedCards() => completedCards;

    // =========================
    // SISTEMA DE POCIONES DE CALIDAD
    // =========================
    public void CompleteQualityPotion()
    {
        qualityPotionsCompleted++;
        SaveProgress();
        NotifyProgressChanged();

        Debug.Log($"[GameManager] Pocion de calidad completada. Total: {qualityPotionsCompleted}");
    }

    public int GetQualityPotionsCompleted() => qualityPotionsCompleted;

    private void NotifyProgressChanged()
    {
        OnProgressChanged?.Invoke(completedCards, qualityPotionsCompleted);
        // Si tu CajaCalidadUI ya maneja esto, aqui es donde podrias avisarle
        // Ej: qualityBoxUI?.Refresh(completedCards, qualityPotionsCompleted);
    }

    // =========================
    // GUARDADO / CARGA
    // =========================
    private void SaveProgress()
    {
        PlayerPrefs.SetInt(K_CARDS, completedCards);
        PlayerPrefs.SetInt(K_QPOTIONS, qualityPotionsCompleted);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        completedCards = PlayerPrefs.GetInt(K_CARDS, 0);
        qualityPotionsCompleted = PlayerPrefs.GetInt(K_QPOTIONS, 0);
    }

    // =========================
    // RESETEAR JUEGO
    // =========================
    public void ResetGame()
    {
        if (!Application.isEditor)
        {
            Debug.LogWarning("[GameManager] ResetGame solo funciona en el editor por seguridad.");
            return;
        }

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // =========================
    // DEBUG INFO
    // =========================
    private void UpdateDebugInfo()
    {
        string info = "=== DEBUG INFO ===\n";

        if (energyBarSystem != null)
        {
            info += $"Energia: {energyBarSystem.GetEnergyPercentage():F1}%\n";
            info += $"Penalizacion de tiempo: +{energyBarSystem.GetTimePenalty()}s\n";
        }
        else
        {
            info += "Energia: (sin referencia)\n";
        }

        if (inventorySystem != null)
            info += $"Monedas: {inventorySystem.coins}\n";
        else
            info += "Monedas: (sin referencia)\n";

        info += $"Cartas completadas: {completedCards}\n";
        info += $"Pociones de calidad: {qualityPotionsCompleted}\n";

        debugText.text = info;
    }

    // =========================
    // UTILIDAD PARA TESTING (ContextMenu)
    // =========================
    [ContextMenu("Add 5 Completed Cards")]
    public void AddCompletedCards()
    {
        for (int i = 0; i < 5; i++)
            CompleteCard();
    }

    [ContextMenu("Add 5 Quality Potions")]
    public void AddQualityPotions()
    {
        for (int i = 0; i < 5; i++)
            CompleteQualityPotion();
    }

    [ContextMenu("Add 1000 Coins")]
    public void AddMoney()
    {
        if (inventorySystem == null) inventorySystem = FindObjectOfType<InventorySystem>();
        inventorySystem?.AddCoins(1000);
    }

    [ContextMenu("Add Test Seeds")]
    public void AddTestSeeds()
    {
        if (inventorySystem == null) inventorySystem = FindObjectOfType<InventorySystem>();
        if (inventorySystem == null) return;

        inventorySystem.AddSemilla(PlantaTipo.Lumina, 10);
        inventorySystem.AddSemilla(PlantaTipo.Eldebria, 10);
        inventorySystem.AddSemilla(PlantaTipo.Jiveria, 10);
        inventorySystem.AddSemilla(PlantaTipo.Lirien, 10);
    }

    [ContextMenu("Add Test Plants")]
    public void AddTestPlants()
    {
        if (inventorySystem == null) inventorySystem = FindObjectOfType<InventorySystem>();
        if (inventorySystem == null) return;

        inventorySystem.AddPlant(PlantaTipo.Lumina, PlantaCalidad.Estandar, 5);
        inventorySystem.AddPlant(PlantaTipo.Eldebria, PlantaCalidad.Estandar, 5);

        inventorySystem.AddPlant(PlantaTipo.Lumina, PlantaCalidad.Plata, 3);
        inventorySystem.AddPlant(PlantaTipo.Eldebria, PlantaCalidad.Plata, 3);
    }

    [ContextMenu("Fill Energy Bar")]
    public void FillEnergy()
    {
        if (energyBarSystem == null) energyBarSystem = FindObjectOfType<BarraEnergiaSistema>();
        energyBarSystem?.ResetEnergy();
    }

    [ContextMenu("Set Energy to 50%")]
    public void SetEnergyHalf()
    {
        if (energyBarSystem == null) energyBarSystem = FindObjectOfType<BarraEnergiaSistema>();
        energyBarSystem?.SetEnergyTo50();
    }

    [ContextMenu("Print Inventory")]
    public void PrintInventory()
    {
        if (inventorySystem == null) inventorySystem = FindObjectOfType<InventorySystem>();
        if (inventorySystem == null)
        {
            Debug.LogWarning("[GameManager] InventorySystem no encontrado.");
            return;
        }

        Debug.Log("=== INVENTARIO ===");
        Debug.Log($"Monedas: {inventorySystem.coins}");

        Debug.Log("SEMILLAS:");
        foreach (var seed in inventorySystem.semillas)
            Debug.Log($"- {seed.plantaTipo}: {seed.cantidad}");

        Debug.Log("PLANTAS:");
        foreach (var plant in inventorySystem.plantas)
            Debug.Log($"- {plant.plantaTipo} ({plant.calidad}): {plant.cantidad}");

        Debug.Log("SUEROS:");
        foreach (var serum in inventorySystem.sueros)
            Debug.Log($"- {serum.sueroNombre}: {serum.cantidad}");
    }
}
