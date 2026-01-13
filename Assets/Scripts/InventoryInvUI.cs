using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryInvUI : MonoBehaviour
{
    public static InventoryInvUI Instance { get; private set; }
    
    [Header("Referencias")]
    public Transform semillasContainer;
    public Transform plantasContainer;
    public Transform suerosContainer;
    public GameObject inventoryItemPrefab;
    public TextMeshProUGUI monedasTxt;
    
    [Header("Configuración")]
    public Sprite infinityIcon;
    
    private List<InventoryItemUI> semillaItems = new List<InventoryItemUI>();
    private List<InventoryItemUI> plantaItems = new List<InventoryItemUI>();
    private List<InventoryItemUI> sueroItems = new List<InventoryItemUI>();
    
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
    
    public void RefreshInventory()
    {
        RefreshSeeds();
        RefreshPlants();
        RefreshSerums();
        UpdateCoinsDisplay();
    }
    
    void RefreshSeeds()
    {
        // Limpiar items existentes
        foreach (var item in semillaItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        semillaItems.Clear();
        
        // Crear items para cada tipo de semilla
        foreach (PlantaTipo plantaTipo in System.Enum.GetValues(typeof(PlantaTipo)))
        {
            PlantasData data = InvernaderoManager.Instance.plantDatabase.GetPlantas(plantaTipo);
            if (data == null) continue;
            
            // Mostrar siempre Drakonia y Falsibaya (perennes)
            bool isPerennial = data.semillaCiclo == SemillaCiclo.Perenne;
            int count = InventorySystem.Instance.GetSeedCount(plantaTipo);
            
            if (isPerennial || count > 0)
            {
                GameObject itemObj = Instantiate(inventoryItemPrefab, semillasContainer);
                InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();
                
                if (itemUI != null)
                {
                    itemUI.SetupSeed(plantaTipo, data, count, isPerennial);
                    semillaItems.Add(itemUI);
                }
            }
        }
    }
    
    void RefreshPlants()
    {
        // Limpiar items existentes
        foreach (var item in plantaItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        plantaItems.Clear();
        
        // Mostrar plantas agrupadas por tipo y calidad
        var inventory = InventorySystem.Instance;
        foreach (var plantItem in inventory.plantas)
        {
            if (plantItem.cantidad > 0)
            {
                PlantasData data = InvernaderoManager.Instance.plantDatabase.GetPlantas(plantItem.plantaTipo);
                if (data == null) continue;
                
                GameObject itemObj = Instantiate(inventoryItemPrefab, plantasContainer);
                InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();
                
                if (itemUI != null)
                {
                    itemUI.SetupPlant(plantItem.plantaTipo, plantItem.calidad, data, plantItem.cantidad);
                    plantaItems.Add(itemUI);
                }
            }
        }
    }
    
    void RefreshSerums()
    {
        // Limpiar items existentes
        foreach (var item in sueroItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        sueroItems.Clear();
        
        // Mostrar sueros
        var inventory = InventorySystem.Instance;
        foreach (var serum in inventory.sueros)
        {
            if (serum.cantidad > 0)
            {
                GameObject itemObj = Instantiate(inventoryItemPrefab, suerosContainer);
                InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();
                
                if (itemUI != null)
                {
                    itemUI.SetupSerum(serum.sueroNombre, serum.cantidad);
                    sueroItems.Add(itemUI);
                }
            }
        }
    }
    
    void UpdateCoinsDisplay()
    {
        if (monedasTxt != null)
        {
            monedasTxt.text = $"Monedas: {InventorySystem.Instance.coins}";
        }
    }
}

public class InventoryItemUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image itemImg;
    public TextMeshProUGUI itemNombreTxt;
    public TextMeshProUGUI cantidadTxt;
    public Button plantarBtn;
    public Button venderBtn;
    public Image calidadEstrella;
    
    [Header("Colores de Calidad")]
    public Color standardColor = Color.white;
    public Color silverColor = Color.gray;
    public Color goldColor = Color.yellow;
    
    private PlantaTipo plantaTipo;
    private PlantaCalidad calidad;
    private bool isSemilla;
    private bool isPlanta;
    private bool isSuero;
    private bool isPerenne;
    
    void Start()
    {
        if (plantarBtn != null)
        {
            plantarBtn.onClick.AddListener(OnPlantClicked);
        }
        
        if (venderBtn != null)
        {
            venderBtn.onClick.AddListener(OnSellClicked);
        }
    }
    
    public void SetupSeed(PlantaTipo type, PlantasData data, int count, bool perennial)
    {
        plantaTipo = type;
        isSemilla = true;
        isPerenne = perennial;
        
        if (itemImg != null)
            itemImg.sprite = data.semillaSprite;
        
        if (itemNombreTxt != null)
            itemNombreTxt.text = $"{data.nombre} (Semilla)";
        
        if (cantidadTxt != null)
        {
            if (perennial)
            {
                cantidadTxt.text = "~~~";
            }
            else
            {
                cantidadTxt.text = count.ToString();
            }
        }
        
        if (plantarBtn != null)
            plantarBtn.gameObject.SetActive(true);
        
        if (venderBtn != null)
            venderBtn.gameObject.SetActive(!perennial && count > 0);
        
        if (calidadEstrella != null)
            calidadEstrella.gameObject.SetActive(false);
    }
    
    public void SetupPlant(PlantaTipo type, PlantaCalidad qual, PlantasData data, int count)
    {
        plantaTipo = type;
        calidad = qual;
        isPlanta = true;
        
        if (itemImg != null)
            itemImg.sprite = data.plantaSprite;
        
        if (itemNombreTxt != null)
        {
            string qualityName = qual == PlantaCalidad.Estandar ? "" : 
                                qual == PlantaCalidad.Plata ? " (Plata)" : " (Oro)";
            itemNombreTxt.text = $"{data.nombre}{qualityName}";
        }
        
        if (cantidadTxt != null)
            cantidadTxt.text = count.ToString();
        
        if (plantarBtn != null)
            plantarBtn.gameObject.SetActive(false);
        
        if (venderBtn != null)
            venderBtn.gameObject.SetActive(true);
        
        if (calidadEstrella != null)
        {
            calidadEstrella.gameObject.SetActive(true);
            calidadEstrella.color = qual == PlantaCalidad.Estandar ? standardColor :
                                     qual == PlantaCalidad.Plata ? silverColor : goldColor;
        }
    }
    
    public void SetupSerum(string serumName, int count)
    {
        isSuero = true;
        
        if (itemNombreTxt != null)
            itemNombreTxt.text = serumName;
        
        if (cantidadTxt != null)
            cantidadTxt.text = count.ToString();
        
        if (plantarBtn != null)
            plantarBtn.gameObject.SetActive(false);
        
        if (venderBtn != null)
            venderBtn.gameObject.SetActive(true);
        
        if (calidadEstrella != null)
            calidadEstrella.gameObject.SetActive(false);
    }
    
    void OnPlantClicked()
    {
        if (isSemilla)
        {
            // Intentar plantar en el siguiente slot disponible
            bool success = InvernaderoManager.Instance.PlantInNextAvailableSlot(plantaTipo);
            
            if (success)
            {
                Debug.Log($"Plantado {plantaTipo}");
            }
            else
            {
                Debug.Log($"No se pudo plantar {plantaTipo}");
            }
        }
    }
    
    void OnSellClicked()
    {
        if (isSemilla && !isPerenne)
        {
            InventorySystem.Instance.SellSeed(plantaTipo, 1);
        }
        else if (isPlanta)
        {
            InventorySystem.Instance.SellPlant(plantaTipo, calidad, 1);
        }
        else if (isSuero)
        {
            // Implementar venta de sueros si es necesario
        }
    }
}