using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlantAreaDetailView : MonoBehaviour
{
    public static PlantAreaDetailView Instance { get; private set; }

    [Header("Panel General que Contiene Todo")]
    public GameObject detailViewPanel;

    [Header("UI Superior - Info de la Planta")]
    public TextMeshProUGUI areaTitleText;
    public Image areaPlantImage;
    public TextMeshProUGUI plantInfoText;

    [Header("Grid Container 2x2")]
    public Transform slotsGridContainer; 

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


        if (detailViewPanel != null)
        {
            detailViewPanel.SetActive(false);
        }
    }



    public void OpenAreaDetail(PlantaTipo plantType)
    {

        currentAreaType = plantType;
        currentSlots = GetSlotsForArea(plantType);

        if (currentSlots.Count == 0)
        {
            Debug.LogError($"No hay slots configurados para {plantType}");
            return;
        }

        if (detailViewPanel != null)
        {
            detailViewPanel.SetActive(true);
        }

        UpdateDetailUI();

        Debug.Log($"Vista detallada abierta: {plantType} con {currentSlots.Count} slots");
    }

    public void CloseAreaDetail()
    {
        if (detailViewPanel != null)
        {
            detailViewPanel.SetActive(false);
        }

        currentSlots.Clear();

     
        UpdateMainViewButtons();

        Debug.Log("Vista detallada cerrada");
    }

    void UpdateDetailUI()
    {
        PlantasData data = InvernaderoManager.Instance?.plantDatabase.GetPlantas(currentAreaType);

        if (data == null)
        {
            Debug.LogError($"PlantaData no encontrado para {currentAreaType}");
            return;
        }

       
        if (areaTitleText != null)
        {
            areaTitleText.text = $"Área de {data.nombre}";
        }

   
        if (areaPlantImage != null && data.plantaSprite != null)
        {
            areaPlantImage.sprite = data.plantaSprite;
        }


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
        UpdateButtons();
    }






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


    bool CanPlantInArea(PlantaTipo type)
    {
        PlantasData data = InvernaderoManager.Instance?.plantDatabase.GetPlantas(type);
        if (data == null) return false;

        //40.4
        if (!BarraEnergiaSistema.Instance.CanPlant(data.energiaConsumo))
            return false;

        //40.3
        if (data.semillaCiclo == SemillaCiclo.Replantar)
        {
            return InventorySystem.Instance.HasSeed(type);
        }

        return true;
    }



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


        UpdateMainViewButtons();
    }


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


        UpdateMainViewButtons();
    }


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
        // RQF38
        if (detailViewPanel != null && detailViewPanel.activeSelf)
        {
            UpdateButtons();
        }
    }






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