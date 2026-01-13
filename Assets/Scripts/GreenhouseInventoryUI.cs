using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// RQF57: Muestra el inventario visual del invernadero
/// Incluye plantas, semillas, sueros y monedas
/// </summary>
public class GreenhouseInventoryUI : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject plantSlotPrefab;
    public GameObject seedSlotPrefab;
    public GameObject serumSlotPrefab;

    [Header("Contenedores")]
    public Transform plantasContent;
    public Transform semillasContent;
    public Transform suerosContent;

    [Header("UI General")]
    public TextMeshProUGUI coinsText;
    public Button refreshButton;

    [Header("Panel de Acción")]
    public GameObject actionPanel;
    public TextMeshProUGUI actionTitleText;
    public TextMeshProUGUI actionDescriptionText;
    public Button plantButton;
    public Button sellButton;
    public Button closeActionButton;



    private PlantaTipo selectedPlantType;
    private PlantaCalidad selectedPlantQuality;
    private string selectedItemType;

    void Start()
    {
        SetupButtons();
        RefreshInventory();

        // Suscribirse a cambios del inventario
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += RefreshInventory;
        }
    }

    void OnDestroy()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= RefreshInventory;
        }
    }

    void SetupButtons()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshInventory);

        if (closeActionButton != null)
            closeActionButton.onClick.AddListener(CloseActionPanel);

        if (actionPanel != null)
            actionPanel.SetActive(false);
    }

    /// <summary>
    /// RQF57: Refresca todo el inventario visual
    /// </summary>
    public void RefreshInventory()
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogError("[GreenhouseInventoryUI] InventorySystem no encontrado!");
            return;
        }

        RefreshPlantas();
        RefreshSemillas();
        RefreshSueros();
        RefreshCoins();
    }

    #region PLANTAS

    void RefreshPlantas()
    {
        ClearContainer(plantasContent);

        foreach (var plant in InventorySystem.Instance.plantas)
        {
            GameObject slot = Instantiate(plantSlotPrefab, plantasContent);
            SetupPlantSlot(slot, plant);
        }
    }

    void SetupPlantSlot(GameObject slot, InventoryItem plant)
    {
        // Buscar componentes
        Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
        TextMeshProUGUI nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI quantityText = slot.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
        Button actionButton = slot.transform.Find("ActionButton")?.GetComponent<Button>();

        // Configurar visual
        if (InventorySystem.Instance.plantBD != null)
        {
            PlantasData info = InventorySystem.Instance.plantBD.GetPlantas(plant.plantaTipo);
            if (info != null && icon != null)
            {
                icon.sprite = info.plantaSprite;
            }
        }

        if (nameText != null)
            nameText.text = $"{plant.plantaTipo}\n<size=12>({plant.calidad})</size>";

        if (quantityText != null)
            quantityText.text = $"x{plant.cantidad}";

        // Configurar botón
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(() => ShowPlantActions(plant));
        }
    }

    void ShowPlantActions(InventoryItem plant)
    {
        selectedItemType = "plant";
        selectedPlantType = plant.plantaTipo;
        selectedPlantQuality = plant.calidad;

        if (actionPanel != null)
        {
            actionPanel.SetActive(true);

            if (actionTitleText != null)
                actionTitleText.text = $"{plant.plantaTipo} ({plant.calidad})";

            if (actionDescriptionText != null)
            {
                PlantasData info = InventorySystem.Instance.plantBD.GetPlantas(plant.plantaTipo);
                if (info != null)
                {
                    int precio = plant.calidad == PlantaCalidad.Estandar ? info.precioVentaEstandar :
                                plant.calidad == PlantaCalidad.Plata ? info.precioVentaPlata :
                                info.precioVentaOro;
                    actionDescriptionText.text = $"Cantidad: {plant.cantidad}\nPrecio venta: {precio} monedas";
                }
            }

            // Configurar botones
            if (plantButton != null)
            {
                plantButton.gameObject.SetActive(true);
                plantButton.onClick.RemoveAllListeners();
                plantButton.onClick.AddListener(PlantSelectedPlant);
            }

            if (sellButton != null)
            {
                sellButton.gameObject.SetActive(true);
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(SellSelectedPlant);
            }
        }
    }

    void PlantSelectedPlant()
    {
        // RQF60: Plantar desde el inventario
        Debug.Log($"🌱 Plantar {selectedPlantType} ({selectedPlantQuality})");

        // Aquí llamarías a tu sistema de plantación
        // Ejemplo: PlantingSystem.Instance.PlantFromInventory(selectedPlantType, selectedPlantQuality);

        CloseActionPanel();
    }

    void SellSelectedPlant()
    {
        // RQF59: Vender plantas
        InventorySystem.Instance.SellPlant(selectedPlantType, selectedPlantQuality, 1);
        CloseActionPanel();
    }

    #endregion

    #region SEMILLAS

    void RefreshSemillas()
    {
        ClearContainer(semillasContent);

        foreach (var seed in InventorySystem.Instance.semillas)
        {
            GameObject slot = Instantiate(seedSlotPrefab, semillasContent);
            SetupSeedSlot(slot, seed);
        }
    }

    void SetupSeedSlot(GameObject slot, SeedItem seed)
    {
        Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
        TextMeshProUGUI nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI quantityText = slot.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
        Button actionButton = slot.transform.Find("ActionButton")?.GetComponent<Button>();

        if (InventorySystem.Instance.plantBD != null)
        {
            PlantasData info = InventorySystem.Instance.plantBD.GetPlantas(seed.plantaTipo);
            if (info != null && icon != null)
            {
                // Puedes usar un icono diferente para semillas
                icon.sprite = info.plantaSprite;
                icon.color = new Color(1f, 1f, 1f, 0.6f); // Más transparente
            }
        }

        if (nameText != null)
            nameText.text = $"Semilla\n{seed.plantaTipo}";

        if (quantityText != null)
            quantityText.text = $"x{seed.cantidad}";

        if (actionButton != null)
        {
            actionButton.onClick.AddListener(() => ShowSeedActions(seed));
        }
    }

    void ShowSeedActions(SeedItem seed)
    {
        selectedItemType = "seed";
        selectedPlantType = seed.plantaTipo;

        if (actionPanel != null)
        {
            actionPanel.SetActive(true);

            if (actionTitleText != null)
                actionTitleText.text = $"Semilla de {seed.plantaTipo}";

            if (actionDescriptionText != null)
            {
                PlantasData info = InventorySystem.Instance.plantBD.GetPlantas(seed.plantaTipo);
                if (info != null)
                {
                    actionDescriptionText.text = $"Cantidad: {seed.cantidad}\nPrecio venta: {info.precioCompraEstandar} monedas";
                }
            }

            if (plantButton != null)
            {
                plantButton.gameObject.SetActive(true);
                plantButton.onClick.RemoveAllListeners();
                plantButton.onClick.AddListener(PlantSelectedSeed);
            }

            if (sellButton != null)
            {
                sellButton.gameObject.SetActive(true);
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(SellSelectedSeed);
            }
        }
    }

    void PlantSelectedSeed()
    {
        Debug.Log($"🌱 Plantar semilla de {selectedPlantType}");
        // Aquí llamarías a tu sistema de plantación con semillas
        CloseActionPanel();
    }

    void SellSelectedSeed()
    {
        InventorySystem.Instance.SellSeed(selectedPlantType, 1);
        CloseActionPanel();
    }

    #endregion

    #region SUEROS

    void RefreshSueros()
    {
        ClearContainer(suerosContent);

        foreach (var serum in InventorySystem.Instance.sueros)
        {
            GameObject slot = Instantiate(serumSlotPrefab, suerosContent);
            SetupSerumSlot(slot, serum);
        }
    }

    void SetupSerumSlot(GameObject slot, SerumItem serum)
    {
        Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
        TextMeshProUGUI nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI quantityText = slot.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();

        // Los sueros no tienen botones de acción por ahora
        // Solo se muestran como información

        if (nameText != null)
            nameText.text = serum.sueroNombre;

        if (quantityText != null)
            quantityText.text = $"x{serum.cantidad}";
    }

    #endregion

    #region MONEDAS

    void RefreshCoins()
    {
        if (coinsText != null)
        {
            coinsText.text = $"💰 {InventorySystem.Instance.coins}";
        }
    }

    #endregion

    #region UTILIDADES

    void ClearContainer(Transform container)
    {
        if (container == null) return;

        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    void CloseActionPanel()
    {
        if (actionPanel != null)
        {
            actionPanel.SetActive(false);
        }
    }

    #endregion
}