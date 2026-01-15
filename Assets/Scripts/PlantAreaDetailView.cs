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

    [Header("Botones Principales")]
    public Button plantAllButton;
    public Button harvestAllButton;

    private PlantaTipo currentAreaType;
    private List<CultivoSlotInfo> currentSlots = new List<CultivoSlotInfo>();
    public List<CultivoSlotUI> cultivoSlotUIs = new List<CultivoSlotUI>();

    void Awake()
    {
        if (Instance == null)
        {
            Debug.Log("PlantAreaDetailView inicializado");
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("PlantAreaDetailView ya existe, destruyendo instancia duplicada");
            Destroy(gameObject);
        }
    }

    void Start()
    {
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
        UpdateCultivoSlotsUI();
        Debug.Log($"Vista detallada abierta: {plantType} con {currentSlots.Count} slots");
    }

    public void CloseAreaDetail()
    {
        if (detailViewPanel != null)
        {
            detailViewPanel.SetActive(false);
        }
        Debug.Log("Vista detallada cerrada");
    }

    void UpdateDetailUI()
    {
        PlantData data = InvernaderoManager.Instance?.plantDatabase.GetPlantas(currentAreaType);

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
            float minutes = data.tiempoCrecimientoMinutos;

            plantInfoText.text = $"<b>Tipo:</b> {cycleType}\n" +
                                $"<b>Tiempo base:</b> {minutes} min\n" +
                                $"<b>Energía:</b> {data.energiaConsumo}%\n" +
                                $"<b>Cosecha:</b> {data.cosechaCantidad}x\n" +
                                $"<b>Venta:</b> {data.precioVentaEstandar} monedas";
        }
    }

    bool CanPlantInArea(PlantaTipo type)
    {
        PlantData data = InvernaderoManager.Instance?.plantDatabase.GetPlantas(type);
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


    List<CultivoSlotInfo> GetSlotsForArea(PlantaTipo plantaTipo)
    {
        if (InvernaderoManager.Instance == null)
        {
            throw new System.Exception("InvernaderoManager no encontrado!");
        }

        return InvernaderoManager.Instance.allPlantSlots[plantaTipo];
    }

    private void UpdateCultivoSlotsUI()
    {
        Debug.Log($"Actualizando UI de Cultivo para {currentAreaType} con {currentSlots.Count} slots");
        int i = 0;
        foreach (CultivoSlotInfo cultivoSlotInfo in currentSlots)
        {
            PlantData plantData = InvernaderoManager.Instance.plantDatabase.GetPlantas(currentAreaType);
            PlantAreaDetailView.Instance.cultivoSlotUIs[i].SetUp(plantData.plantaSprite, cultivoSlotInfo.timer, cultivoSlotInfo);
            i++;
        }
    }
}
