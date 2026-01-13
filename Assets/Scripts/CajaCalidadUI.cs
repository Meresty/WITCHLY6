using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CajaCalidadUI : MonoBehaviour
{
    public static CajaCalidadUI Instance { get; private set; }

    [Header("Referencias UI")]
    public GameObject selectionPanel;
    public Transform plantListContainer;
    public GameObject plantSelectionItemPrefab;
    public Button confirmarBtn;
    public Button cancelarBtn;
    public TextMeshProUGUI instruccionTxt;

    private CajaCalidad cajaActual;
    private PlantaTipo plantaActualTipo;
    private PlantaCalidad calidadActual;
    private List<GameObject> spawnedItems = new List<GameObject>();

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
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }

        if (confirmarBtn != null)
        {
            confirmarBtn.onClick.AddListener(OnConfirmClicked);
        }

        if (cancelarBtn != null)
        {
            cancelarBtn.onClick.AddListener(OnCancelClicked);
        }
    }

    public void OpenBoxSelection(CajaCalidad box)
    {
        cajaActual = box;

        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
        }

        PopulatePlantList();
    }

    void PopulatePlantList()
    {
        // Limpiar lista anterior
        foreach (var item in spawnedItems)
        {
            Destroy(item);
        }
        spawnedItems.Clear();

        if (instruccionTxt != null)
        {
            instruccionTxt.text = "Selecciona 2 plantas de la misma calidad para mejorar:" + "Estándar a Plata (3 min)" + "Plata a Oro (5 min)";
        }


        var inventory = InventorySystem.Instance;

        foreach (var plantaItem in inventory.plantas)
        {
            // Solo mostrar plantas que no sean oro y que tengamos al menos 2
            if (plantaItem.calidad != PlantaCalidad.Oro && plantaItem.cantidad >= 2)
            {
                PlantasData data = InvernaderoManager.Instance.plantDatabase.GetPlantas(plantaItem.plantaTipo);
                if (data == null) continue;

                GameObject itemObj = Instantiate(plantSelectionItemPrefab, plantListContainer);
                PlantSelectionItem itemUI = itemObj.GetComponent<PlantSelectionItem>();

                if (itemUI != null)
                {
                    itemUI.Setup(plantaItem.plantaTipo, plantaItem.calidad, data, plantaItem.cantidad, this);
                    spawnedItems.Add(itemObj);
                }
            }
        }

        if (spawnedItems.Count == 0)
        {
            if (instruccionTxt != null)
            {
                instruccionTxt.text = "No tienes plantas disponibles para mejorar." + "Necesitas al menos 2 plantas de la misma calidad (Estándar o Plata).";
            }
        }
    }

    public void SelectPlant(PlantaTipo tipo, PlantaCalidad calidad)
    {
        plantaActualTipo = tipo;
        calidadActual = calidad;

        if (confirmarBtn != null)
        {
            confirmarBtn.interactable = true;
        }
    }

    void OnConfirmClicked()
    {
        if (cajaActual != null)
        {
            bool success = cajaActual.StartProcessing(plantaActualTipo, calidadActual);

            if (success)
            {
                ClosePanel();
            }
            else
            {
                Debug.Log("No se pudo iniciar el procesamiento");
            }
        }
    }

    void OnCancelClicked()
    {
        ClosePanel();
    }

    void ClosePanel()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }

        cajaActual = null;
        plantaActualTipo = PlantaTipo.Lumina;
        calidadActual = PlantaCalidad.Estandar;

        if (confirmarBtn != null)
        {
            confirmarBtn.interactable = false;
        }
    }
}

public class PlantSelectionItem : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image itemImg;
    public TextMeshProUGUI itemNombreTxt;
    public TextMeshProUGUI cantidadTxt;
    public TextMeshProUGUI upgradeInfoText;
    public Button elegirBtn;
    public Image calidadEstrella;

    [Header("Colores")]
    public Color estandarColor = Color.white;
    public Color plataColor = new Color(0.75f, 0.75f, 0.75f);
    public Color oroColor = new Color(1f, 0.84f, 0f);

    private PlantaTipo plantaTipo;
    private PlantaCalidad calidad;
    private CajaCalidadUI parentUI;

    void Start()
    {
        if (elegirBtn != null)
        {
            elegirBtn.onClick.AddListener(OnSelectClicked);
        }
    }

    public void Setup(PlantaTipo type, PlantaCalidad qual, PlantasData data, int count, CajaCalidadUI parent)
    {
        plantaTipo = type;
        calidad = qual;
        parentUI = parent;

        if (itemImg != null)
        {
            itemImg.sprite = data.plantaSprite;
        }

        if (itemNombreTxt != null)
        {
            string qualityName = qual == PlantaCalidad.Estandar ? "Estándar" : "Plata";
            itemNombreTxt.text = $"{data.nombre} ({qualityName})";
        }

        if (cantidadTxt != null)
        {
            cantidadTxt.text = $"Disponible: {count}";
        }

        if (upgradeInfoText != null)
        {
            PlantaCalidad nextQuality = qual == PlantaCalidad.Estandar ? PlantaCalidad.Plata : PlantaCalidad.Oro;
            int time = qual == PlantaCalidad.Estandar ? 3 : 5;
            upgradeInfoText.text = $"Mejora a {nextQuality} ({time} min)";
        }

        if (calidadEstrella != null)
        {
            calidadEstrella.color = qual == PlantaCalidad.Estandar ? estandarColor : plataColor;
        }
    }

    void OnSelectClicked()
    {
        if (parentUI != null)
        {
            parentUI.SelectPlant(plantaTipo, calidad);
        }
    }
}