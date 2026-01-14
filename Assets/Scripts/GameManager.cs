using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referencias de Sistemas")]
    public InventorySystem inventorySystem;
    public BarraEnergiaSistema energyBarSystem;
    public InvernaderoManager greenhouseManager;
    public InventoryUI inventoryUI;
    public CajaCalidadUI qualityBoxUI;

    [Header("Cajas de Calidad")]
    public CajaCalidad[] qualityBoxes = new CajaCalidad[4];

    [Header("UI Principal")]
    public TextMeshProUGUI debugText;
    public Button resetGameButton;

    [Header("Logros/Progreso")]
    private int completedCards = 0;
    private int qualityPotionsCompleted = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeSystems();
    }

    void Start()
    {
        if (resetGameButton != null)
        {
            resetGameButton.onClick.AddListener(ResetGame);
        }

        LoadProgress();
        CheckQualityBoxUnlocks();
    }

    void Update()
    {
        if (debugText != null)
        {
            UpdateDebugInfo();
        }
    }

    void InitializeSystems()
    {
        // Los sistemas ya se inicializan solos en sus Awake
        // Aquñ solo verificamos que existan
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

    // SISTEMA DE CARTAS
    public void CompleteCard()
    {
        completedCards++;
        SaveProgress();
        CheckQualityBoxUnlocks();

        Debug.Log($"Carta completada! Total: {completedCards}");
    }

    public int GetCompletedCards()
    {
        return completedCards;
    }

    // SISTEMA DE POCIONES DE CALIDAD
    public void CompleteQualityPotion()
    {
        qualityPotionsCompleted++;
        SaveProgress();
        CheckQualityBoxUnlocks();

        Debug.Log($"Pociñn de calidad completada! Total: {qualityPotionsCompleted}");
    }

    // DESBLOQUEO DE CAJAS
    void CheckQualityBoxUnlocks()
    {
        // Caja 0: Se desbloquea al completar 12 cartas
        if (completedCards >= 12 && qualityBoxes[0] != null)
        {
            qualityBoxes[0].DesbloquearCaja();
        }

        // Caja 1: Se desbloquea al completar 5 pociones de calidad
        if (qualityPotionsCompleted >= 5 && qualityBoxes[1] != null)
        {
            qualityBoxes[1].DesbloquearCaja();
        }

        // Caja 2: Se desbloquea al completar 15 pociones de calidad
        if (qualityPotionsCompleted >= 15 && qualityBoxes[2] != null)
        {
            qualityBoxes[2].DesbloquearCaja();
        }

        // Caja 3: Se desbloquea al completar 30 pociones de calidad
        if (qualityPotionsCompleted >= 30 && qualityBoxes[3] != null)
        {
            qualityBoxes[3].DesbloquearCaja();
        }
    }

    // GUARDADO/CARGA DE PROGRESO
    void SaveProgress()
    {
        PlayerPrefs.SetInt("CompletedCards", completedCards);
        PlayerPrefs.SetInt("QualityPotions", qualityPotionsCompleted);
        PlayerPrefs.Save();
    }

    void LoadProgress()
    {
        completedCards = PlayerPrefs.GetInt("CompletedCards", 0);
        qualityPotionsCompleted = PlayerPrefs.GetInt("QualityPotions", 0);
    }

    // RESETEAR JUEGO
    public void ResetGame()
    {
        if (Application.isEditor)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
        else
        {
            Debug.LogWarning("ResetGame solo funciona en el editor por seguridad");
        }
    }

    // DEBUG INFO
    void UpdateDebugInfo()
    {
        if (energyBarSystem == null) return;

        string info = $"=== DEBUG INFO ===";
        info += $"Energña: {energyBarSystem.GetEnergyPercentage():F1}%";
        info += $"Penalizaciñn de tiempo: +{energyBarSystem.GetTimePenalty()}s";
        info += $"Monedas: {inventorySystem.coins}";
        // info += $"Plantas activas: {greenhouseManager.GetTotalActivePlants()}";
        info += $"Cartas completadas: {completedCards}";
        info += $"Pociones de calidad: {qualityPotionsCompleted}";


        debugText.text = info;
    }

    // MñTODOS DE UTILIDAD PARA TESTING
    [ContextMenu("Add 5 Completed Cards")]
    public void AddCompletedCards()
    {
        for (int i = 0; i < 5; i++)
        {
            CompleteCard();
        }
    }

    [ContextMenu("Add 5 Quality Potions")]
    public void AddQualityPotions()
    {
        for (int i = 0; i < 5; i++)
        {
            CompleteQualityPotion();
        }
    }

    [ContextMenu("Unlock All Quality Boxes")]
    public void UnlockAllBoxes()
    {
        foreach (var box in qualityBoxes)
        {
            if (box != null)
            {
                box.DesbloquearCaja();
            }
        }
    }

    [ContextMenu("Add 1000 Coins")]
    public void AddMoney()
    {
        inventorySystem.AddCoins(1000);
    }

    [ContextMenu("Add Test Seeds")]
    public void AddTestSeeds()
    {
        inventorySystem.AddSemilla(PlantaTipo.Lumina, 10);
        inventorySystem.AddSemilla(PlantaTipo.Eldebria, 10);
        inventorySystem.AddSemilla(PlantaTipo.Jiveria, 10);
        inventorySystem.AddSemilla(PlantaTipo.Lirien, 10);
    }

    [ContextMenu("Add Test Plants")]
    public void AddTestPlants()
    {
        // Plantas estñndar
        inventorySystem.AddPlant(PlantaTipo.Lumina, PlantaCalidad.Estandar, 5);
        inventorySystem.AddPlant(PlantaTipo.Eldebria, PlantaCalidad.Estandar, 5);

        // Plantas plata para testing de cajas
        inventorySystem.AddPlant(PlantaTipo.Lumina, PlantaCalidad.Plata, 3);
        inventorySystem.AddPlant(PlantaTipo.Eldebria, PlantaCalidad.Plata, 3);
    }

    [ContextMenu("Fill Energy Bar")]
    public void FillEnergy()
    {
        energyBarSystem.ResetEnergy();
    }

    [ContextMenu("Set Energy to 50%")]
    public void SetEnergyHalf()
    {
        energyBarSystem.SetEnergyTo50();
    }

    [ContextMenu("Print Inventory")]
    public void PrintInventory()
    {
        Debug.Log("=== INVENTARIO ===");
        Debug.Log($"Monedas: {inventorySystem.coins}");

        Debug.Log("SEMILLAS: ");
        foreach (var seed in inventorySystem.semillas)
        {
            Debug.Log($"- {seed.plantaTipo}: {seed.cantidad}");
        }

        Debug.Log("PLANTAS: ");
        foreach (var plant in inventorySystem.plantas)
        {
            Debug.Log($"- {plant.plantaTipo} ({plant.calidad}): {plant.cantidad}");
        }

        Debug.Log("SUEROS: ");
        foreach (var serum in inventorySystem.sueros)
        {
            Debug.Log($"- {serum.sueroNombre}: {serum.cantidad}");
        }
    }
}

// // Script adicional para botones de UI rñpida
// public class QuickActionButtons : MonoBehaviour
// {
//     public void PlantAllLumina()
//     {
//         for (int i = 0; i < 4; i++)
//         {
//             InvernaderoManager.Instance.PlantInNextAvailableSlot(PlantaTipo.Lumina);
//         }
//     }

//     public void PlantAllFalsibaya()
//     {
//         for (int i = 0; i < 4; i++)
//         {
//             InvernaderoManager.Instance.PlantInNextAvailableSlot(PlantaTipo.Falsibaya);
//         }
//     }

//     public void HarvestAll()
//     {
//         InvernaderoManager.Instance.HarvestAll();
//     }

//     public void BuySeedsLumina()
//     {
//         if (InventorySystem.Instance.SpendCoins(7))
//         {
//             InventorySystem.Instance.AddSemilla(PlantaTipo.Lumina, 1);
//         }
//     }
// }