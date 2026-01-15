using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CultivoSlotInfo
{
    public CultivoSlotInfo(PlantaTipo plantaTipo) {
        this.plantaTipo = plantaTipo;
    }

    public PlantaTipo plantaTipo;
    public bool isOccupied = false;
    public Timer timer = new Timer();
}


public class InvernaderoManager : MonoBehaviour
{
    public static InvernaderoManager Instance { get; private set; }

    public PlantData currentParcelaPlantData = null;

    [Header("Referencias")]
    public PlantaBD plantDatabase;
    public SueroDB sueroDatabase;

    public List<CultivoSlotUI> cultivoSlotUIs = new List<CultivoSlotUI>();
    //RQF36
    [Header("Áreas de Cultivo")]
    [Tooltip("RQF36")]
    [SerializeField] private List<CultivoSlotInfo> luminaSlots = new List<CultivoSlotInfo>();
    [SerializeField] private List<CultivoSlotInfo> falsibayaSlots = new List<CultivoSlotInfo>();
    [SerializeField] private List<CultivoSlotInfo> drakoniaSlots = new List<CultivoSlotInfo>();
    [SerializeField] private List<CultivoSlotInfo> eldebriaSlots = new List<CultivoSlotInfo>();
    [SerializeField] private List<CultivoSlotInfo> jiveriaSlots = new List<CultivoSlotInfo>();
    [SerializeField] private List<CultivoSlotInfo> lirienSlots = new List<CultivoSlotInfo>();

    public Dictionary<PlantaTipo, List<CultivoSlotInfo>> allPlantSlots;


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
        }
    }

    void Start()
    {
        Debug.Log("InvernaderoManager inicializado");
        InitilizeSlots();
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        allPlantSlots = new Dictionary<PlantaTipo, List<CultivoSlotInfo>>()
        {
            { PlantaTipo.Lumina, luminaSlots },
            { PlantaTipo.Falsibaya, falsibayaSlots },
            { PlantaTipo.Drakonia, drakoniaSlots },
            { PlantaTipo.Eldebria, eldebriaSlots },
            { PlantaTipo.Jiveria, jiveriaSlots },
            { PlantaTipo.Lirien, lirienSlots }
        };
    }

    private void InitilizeSlots()
    {
        InitializeSlotWithPlantType(luminaSlots, PlantaTipo.Lumina);
        InitializeSlotWithPlantType(falsibayaSlots, PlantaTipo.Falsibaya);
        InitializeSlotWithPlantType(drakoniaSlots, PlantaTipo.Drakonia);
        InitializeSlotWithPlantType(eldebriaSlots, PlantaTipo.Eldebria);
        InitializeSlotWithPlantType(jiveriaSlots, PlantaTipo.Jiveria);
        InitializeSlotWithPlantType(lirienSlots, PlantaTipo.Lirien);
    }

    private void InitializeSlotWithPlantType(List<CultivoSlotInfo> cultivoSlots, PlantaTipo tipo, int slotsCount = 4)
    {
        for(int i = 0; i < slotsCount; i++)
        {
            cultivoSlots.Add(new CultivoSlotInfo(tipo));
        }
    }

    private void Update()
    {
        foreach (var plantaSlots in allPlantSlots.Values)
        {
            foreach (var slot in plantaSlots)
            {
                slot.timer.Tick(Time.deltaTime);
            }
        }
    }

    public bool CanPlantInSlot(PlantaTipo plantaTipo)
    {
        List<CultivoSlotInfo> slots = allPlantSlots[plantaTipo];
        Debug.Log($"[InvernaderoManager] Verificando slots disponibles para {plantaTipo} entre {slots.Count} slots");

        foreach (var slot in slots)
        {
            Debug.Log($"[InvernaderoManager] Slot ocupado: {slot.isOccupied}");
            if (!slot.isOccupied)
            {
                return true;
            }
        }
        return false;
    }

    public void PlantSeed(PlantaTipo plantaTipo)
    {
        if(!CanPlantInSlot(plantaTipo))
        {
            Debug.LogWarning($"No hay slots disponibles para plantar {plantaTipo}");
            return;
        }
        PlantData plantData = plantDatabase.GetPlantas(plantaTipo);
        CultivoSlotInfo firstAvailableSlot = null;
        var allPlantSlotsOfType = allPlantSlots[plantaTipo];
        Debug.Log($"[InvernaderoManager] Buscando slot disponible para {plantaTipo} entre {allPlantSlotsOfType.Count} slots");
        firstAvailableSlot = allPlantSlotsOfType.Find(slot => !slot.isOccupied);

        if(firstAvailableSlot == null) { throw new System.Exception($"No hay slots disponibles para plantar {plantaTipo}"); }

        firstAvailableSlot.isOccupied = true;
        firstAvailableSlot.timer.Start(plantData.tiempoCrecimientoMinutos * 60);
        InventorySystem.Instance.RemoveSeed(plantaTipo, 1);
    }

    public void CosecharPlanta(CultivoSlotInfo slotInfo)
    {
        if (!slotInfo.isOccupied || !slotInfo.timer.hasFinished)
        {
            Debug.LogWarning("No se puede cosechar: Slot no ocupado o planta no lista");
            return;
        }

        PlantData plantData = plantDatabase.GetPlantas(slotInfo.plantaTipo);
        InventorySystem.Instance.AddPlant(plantData.plantaTipo, PlantaCalidad.Estandar, 1);
        slotInfo.isOccupied = false;
        slotInfo.timer.Reset(0);
    }

}