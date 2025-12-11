using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;



public class PlantAreaDetailView : MonoBehaviour
{
    public static PlantAreaDetailView Instance { get; private set; }

    [Header("Paneles Detallados (uno por planta)")]
    public GameObject luminaDetailPanel;
    public GameObject falsibayaDetailPanel;
    public GameObject drakoniaDetailPanel;
    public GameObject eldebriaDetailPanel;
    public GameObject jiveriaDetailPanel;
    public GameObject lirienDetailPanel;

    [Header("Referencias que se Reutilizan")]
    public TextMeshProUGUI areaTitleText;
    public Button closeButton;
    public Button plantAllButton;
    public Button harvestAllButton;
    public Image areaPlantImage;
    public TextMeshProUGUI plantInfoText;

    private PlantaTipo currentAreaType;
    private GameObject currentPanel;
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

        // Cerrar todos los paneles por defecto
        CloseAllPanels();
    }

    /// <summary>
    /// Cierra todos los paneles detallados
    /// </summary>
    void CloseAllPanels()
    {
        if (luminaDetailPanel != null) luminaDetailPanel.SetActive(false);
        if (falsibayaDetailPanel != null) falsibayaDetailPanel.SetActive(false);
        if (drakoniaDetailPanel != null) drakoniaDetailPanel.SetActive(false);
        if (eldebriaDetailPanel != null) eldebriaDetailPanel.SetActive(false);
        if (jiveriaDetailPanel != null) jiveriaDetailPanel.SetActive(false);
        if (lirienDetailPanel != null) lirienDetailPanel.SetActive(false);
    }

    /// <summary>
    /// Abre la vista detallada para un tipo de planta específico
    /// </summary>
    public void OpenAreaDetail(PlantaTipo plantType)
    {
        // Cerrar cualquier panel abierto
        CloseAllPanels();

        currentAreaType = plantType;

        // Abrir el panel correspondiente
        currentPanel = GetPanelForPlantType(plantType);
        if (currentPanel != null)
        {
            currentPanel.SetActive(true);
        }
        else
        {
            Debug.LogError($"Panel no encontrado para {plantType}");
            return;
        }

        // Obtener slots correspondientes
        currentSlots = GetSlotsForArea(plantType);

        if (currentSlots.Count != 4)
        {
            Debug.LogWarning($"El área {plantType} tiene {currentSlots.Count} slots (se esperan 4)");
        }

        // Actualizar UI
        UpdateDetailUI();
    }

    /// <summary>
    /// Obtiene el panel correspondiente al tipo de planta
    /// </summary>
    GameObject GetPanelForPlantType(PlantaTipo type)
    {
        switch (type)
        {
            case PlantaTipo.Lumina: return luminaDetailPanel;
            case PlantaTipo.Falsibaya: return falsibayaDetailPanel;
            case PlantaTipo.Drakonia: return drakoniaDetailPanel;
            case PlantaTipo.Eldebria: return eldebriaDetailPanel;
            case PlantaTipo.Jiveria: return jiveriaDetailPanel;
            case PlantaTipo.Lirien: return lirienDetailPanel;
            default: return null;
        }
    }

    /// <summary>
    /// Cierra la vista detallada
    /// </summary>
    public void CloseAreaDetail()
    {
        CloseAllPanels();
        currentPanel = null;

        // Actualizar botones de áreas en la vista principal
        UpdateMainViewButtons();
    }

    /// <summary>
    /// Actualiza la UI de la vista detallada
    /// </summary>
    void UpdateDetailUI()
    {
        PlantasInfo data = InvernaderoManager.Instance?.plantDatabase.GetPlantas(currentAreaType);

        if (data == null)
        {
            Debug.LogError($"PlantaData no encontrado para {currentAreaType}");
            return;
        }

        // Título
        if (areaTitleText != null)
        {
            areaTitleText.text = $"Área de {data.plantaNombre}";
        }

        // Imagen
        if (areaPlantImage != null && data.plantaSprite != null)
        {
            areaPlantImage.sprite = data.plantaSprite;
        }

        // Info de la planta
        if (plantInfoText != null)
        {
            string cycleType = data.semillaCiclo == SemillaCiclo.Perenne ? "Perenne" : "Replantar";
            int minutes = data.tiempoCrecimientoMinutos;

            plantInfoText.text = $"<b>Tipo:</b> {cycleType}\n" +
                                $"<b>Tiempo base:</b> {minutes} min\n" +
                                $"<b>Energía:</b> {data.energiaConsumo}\n" +
                                $"<b>Cosecha:</b> {data.cosechaCantidad}x\n" +
                                $"<b>Venta:</b> {data.precioVentaEstandar} monedas";
        }

        // Actualizar estado de botones
        UpdateButtons();
    }

    /// <summary>
    /// Actualiza el estado de los botones según los slots
    /// </summary>
    void UpdateButtons()
    {
        int availableSlots = 0;
        int readySlots = 0;

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

        // Botón plantar todo
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

        // Botón cosechar todo
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

        // Verificar energía
        if (!BarraEnergiaSistema.Instance.CanPlant(data.energiaConsumo))
            return false;

        // Si no es perenne, verificar semillas
        if (data.semillaCiclo == SemillaCiclo.Replantar)
        {
            return InventorySystem.Instance.HasSeed(type);
        }

        return true;
    }

    /// <summary>
    /// Planta en todos los slots disponibles
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
                    // Continuar intentando con los demás slots
                }
            }
        }

        Debug.Log($"Plantadas {planted} plantas en {currentAreaType}. Fallos: {failed}");

        UpdateButtons();

        // Notificación (si tienes sistema de notificaciones)
        if (planted > 0)
        {
            Debug.Log($" Plantadas {planted} {currentAreaType}");
        }
        else if (failed > 0)
        {
            Debug.LogWarning("No se pudo plantar (sin energía o semillas)");
        }
    }

    /// <summary>
    /// Cosecha todos los slots listos
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

        Debug.Log($"Cosechadas {harvested} plantas de {currentAreaType}");

        UpdateButtons();

        if (harvested > 0)
        {
            Debug.Log($" ¡Cosechadas {harvested} plantas!");
        }
    }

    /// <summary>
    /// Obtiene los slots correspondientes al área desde InvernaderoManager
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
        // Actualizar botones constantemente mientras esté abierto algún panel
        if (currentPanel != null && currentPanel.activeSelf)
        {
            UpdateButtons();
        }
    }

    #region DEBUGGING

    [ContextMenu("Test Open Lumina")]
    void TestOpenLumina()
    {
        OpenAreaDetail(PlantaTipo.Lumina);
    }

    [ContextMenu("Test Close")]
    void TestClose()
    {
        CloseAreaDetail();
    }

    #endregion
}