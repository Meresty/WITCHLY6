using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlantAreaDetailView : MonoBehaviour
{
    public static PlantAreaDetailView Instance { get; private set; }

    [Header("Panel General que Contiene Todo")]
    public GameObject detailViewPanel; // El panel padre que contiene toda la vista detallada

    [Header("UI Superior - Info de la Planta")]
    public TextMeshProUGUI areaTitleText;
    public Image areaPlantImage;
    public TextMeshProUGUI plantInfoText;

    [Header("Grid Container 2x2")]
    public Transform slotsGridContainer; // El Grid Layout Group donde están los 4 SlotUI

    [Header("Botones Principales")]
    public Button closeButton;
    public Button plantAllButton;
    public Button harvestAllButton;

    private PlantaTipo currentAreaType;
    private List<PlantaSlot> currentSlots = new List<PlantaSlot>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Configurar botones
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseAreaDetail);
        }

        if (plantAllButton != null)
        {
            plantAllButton.onClick.AddListener(PlantAll);
        }

        if (harvestAllButton != null)
        {
            harvestAllButton.onClick.AddListener(HarvestAll);
        }

        // Cerrar panel por defecto
        if (detailViewPanel != null)
        {
            detailViewPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Abre la vista detallada para un tipo de planta específico
    /// </summary>
    public void OpenAreaDetail(PlantaTipo plantType)
    {
        // Guardar el tipo actual
        currentAreaType = plantType;

        // Obtener los slots de esta área
        currentSlots = GetSlotsForArea(plantType);

        if (currentSlots.Count == 0)
        {
            Debug.LogError($"No hay slots configurados para {plantType}");
            return;
        }

        // Activar el panel principal
        if (detailViewPanel != null)
        {
            detailViewPanel.SetActive(true);
        }

        // Actualizar toda la UI
        UpdateDetailUI();

        Debug.Log($"Vista detallada abierta: {plantType} con {currentSlots.Count} slots");
    }

    /// <summary>
    /// Cierra la vista detallada
    /// </summary>
    public void CloseAreaDetail()
    {
        if (detailViewPanel != null)
        {
            detailViewPanel.SetActive(false);
        }

        currentSlots.Clear();

        // Actualizar botones de áreas en la vista principal
        UpdateMainViewButtons();

        Debug.Log("Vista detallada cerrada");
    }

    /// <summary>
    /// Actualiza toda la UI de la vista detallada
    /// </summary>
    void UpdateDetailUI()
    {
        PlantasInfo data = InvernaderoManager.Instance?.plantDatabase.GetPlantas(currentAreaType);

        if (data == null)
        {
            Debug.LogError($"PlantaData no encontrado para {currentAreaType}");
            return;
        }

        // ===== 1. TÍTULO =====
        if (areaTitleText != null)
        {
            areaTitleText.text = $"Área de {data.plantaNombre}";
        }

        // ===== 2. IMAGEN DE LA PLANTA =====
        if (areaPlantImage != null && data.plantaSprite != null)
        {
            areaPlantImage.sprite = data.plantaSprite;
        }

        // ===== 3. INFO DE LA PLANTA =====
        if (plantInfoText != null)
        {
            string cycleType = data.semillaCiclo == SemillaCiclo.Perenne ? "Perenne (∞)" : "Replantar";
            int minutes = data.tiempoCrecimientoMinutos;

            plantInfoText.text = $"<b>Tipo:</b> {cycleType}\n" +
                                $"<b>Tiempo base:</b> {minutes} min\n" +
                                $"<b>Energía:</b> {data.energiaConsumo}%\n" +
                                $"<b>Cosecha:</b> {data.cosechaCantidad}x\n" +
                                $"<b>Venta:</b> {data.precioVentaEstandar} monedas";
        }

        // ===== 4. ACTUALIZAR ESTADO DE BOTONES =====
        UpdateButtons();
    }

    /// <summary>
    /// Actualiza el estado de los botones según los slots
    /// </summary>
    void UpdateButtons()
    {
        int availableSlots = 0;
        int readySlots = 0;

        // Contar slots disponibles y listos
        foreach (var slot in currentSlots)
        {
            if (slot != null)
            {
                if (slot.CanPlant())
                    availableSlots++;

                if (slot.IsReady())
                    readySlots++;
            }
        }

        // ===== BOTÓN PLANTAR TODO =====
        if (plantAllButton != null)
        {
            bool canPlant = availableSlots > 0 && CanPlantInArea(currentAreaType);
            plantAllButton.interactable = canPlant;

            TextMeshProUGUI buttonText = plantAllButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (availableSlots > 0)
                {
                    buttonText.text = $"Plantar Todo ({availableSlots})";
                }
                else
                {
                    buttonText.text = "Sin Espacios";
                }
            }
        }

        // ===== BOTÓN COSECHAR TODO =====
        if (harvestAllButton != null)
        {
            harvestAllButton.interactable = readySlots > 0;

            TextMeshProUGUI buttonText = harvestAllButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (readySlots > 0)
                {
                    buttonText.text = $"Cosechar Todo ({readySlots})";
                }
                else
                {
                    buttonText.text = "Sin Cosechas";
                }
            }
        }
    }

    /// <summary>
    /// Verifica si se puede plantar en esta área
    /// </summary>
    bool CanPlantInArea(PlantaTipo type)
    {
        PlantasInfo data = InvernaderoManager.Instance?.plantDatabase.GetPlantas(type);
        if (data == null) return false;

        // Verificar energía (RQNF40.4 - no se puede plantar al 1%)
        if (!BarraEnergiaSistema.Instance.CanPlant(data.energiaConsumo))
            return false;

        // Si no es perenne, verificar semillas (RQNF40.3)
        if (data.semillaCiclo == SemillaCiclo.Replantar)
        {
            return InventorySystem.Instance.HasSeed(type);
        }

        return true;
    }

    /// <summary>
    /// RQF40: Planta en todos los slots disponibles
    /// </summary>
    void PlantAll()
    {
        int planted = 0;
        int failed = 0;

        foreach (var slot in currentSlots)
        {
            if (slot != null && slot.CanPlant())
            {
                if (slot.PlantSeed(currentAreaType))
                {
                    planted++;
                }
                else
                {
                    failed++;
                }
            }
        }

        Debug.Log($"[PlantAll] Plantadas: {planted} | Fallos: {failed} en {currentAreaType}");

        UpdateButtons();

        // Actualizar vista principal
        UpdateMainViewButtons();
    }

    /// <summary>
    /// RQF41: Cosecha todos los slots listos
    /// </summary>
    void HarvestAll()
    {
        int harvested = 0;

        foreach (var slot in currentSlots)
        {
            if (slot != null && slot.IsReady())
            {
                slot.Harvest();
                harvested++;
            }
        }

        Debug.Log($"[HarvestAll] Cosechadas {harvested} plantas de {currentAreaType}");

        UpdateButtons();

        // Actualizar vista principal
        UpdateMainViewButtons();
    }

    /// <summary>
    /// RQF36: Obtiene los slots correspondientes al área desde InvernaderoManager
    /// </summary>
    List<PlantaSlot> GetSlotsForArea(PlantaTipo type)
    {
        if (InvernaderoManager.Instance == null)
        {
            Debug.LogError("InvernaderoManager no encontrado!");
            return new List<PlantaSlot>();
        }

        switch (type)
        {
            case PlantaTipo.Lumina:
                return InvernaderoManager.Instance.luminaSlots;
            case PlantaTipo.Falsibaya:
                return InvernaderoManager.Instance.falsibayaSlots;
            case PlantaTipo.Drakonia:
                return InvernaderoManager.Instance.drakoniaSlots;
            case PlantaTipo.Eldebria:
                return InvernaderoManager.Instance.eldebriaSlots;
            case PlantaTipo.Jiveria:
                return InvernaderoManager.Instance.jiveriaSlots;
            case PlantaTipo.Lirien:
                return InvernaderoManager.Instance.lirienSlots;
            default:
                return new List<PlantaSlot>();
        }
    }

    /// <summary>
    /// Actualiza los botones de la vista principal
    /// </summary>
    void UpdateMainViewButtons()
    {
        PlantAreaButton[] areaButtons = FindObjectsOfType<PlantAreaButton>();
        foreach (var button in areaButtons)
        {
            button.ForceUpdate();
        }
    }

    void Update()
    {
        // RQF38: Actualizar constantemente mientras esté abierto
        if (detailViewPanel != null && detailViewPanel.activeSelf)
        {
            UpdateButtons();
        }
    }

    /// <summary>
    /// Método público para que los slots individuales actualicen los botones
    /// </summary>
    public void OnSlotStateChanged()
    {
        if (detailViewPanel != null && detailViewPanel.activeSelf)
        {
            UpdateButtons();
            UpdateMainViewButtons();
        }
    }

    #region DEBUGGING

    [ContextMenu("Test Open Lumina")]
    void TestOpenLumina()
    {
        OpenAreaDetail(PlantaTipo.Lumina);
    }

    [ContextMenu("Test Open Falsibaya")]
    void TestOpenFalsibaya()
    {
        OpenAreaDetail(PlantaTipo.Falsibaya);
    }

    [ContextMenu("Test Close")]
    void TestClose()
    {
        CloseAreaDetail();
    }

    #endregion
}