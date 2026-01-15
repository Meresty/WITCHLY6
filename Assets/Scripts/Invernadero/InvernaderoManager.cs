using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CultivoSlotInfo
{
    public CultivoSlotInfo(PlantaTipo plantaTipo)
    {
        this.plantaTipo = plantaTipo;
    }

    public PlantaTipo plantaTipo;
    public bool isOccupied = false;
    public Timer timer = new Timer();
    public bool isReady => timer.hasFinished;
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
        TryLoadAllSlotsState();
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
        for (int i = 0; i < slotsCount; i++)
        {
            cultivoSlots.Add(new CultivoSlotInfo(tipo));
        }
    }

    private void TryLoadAllSlotsState()
    {
        foreach (var plantaSlots in allPlantSlots.Values)
        {
            for (int i = 0; i < plantaSlots.Count; i++)
            {
                LoadSlotState(plantaSlots[i], i);
            }
        }
    }

    private void SaveAllSlotsState()
    {
        foreach (var plantaSlots in allPlantSlots.Values)
        {
            for (int i = 0; i < plantaSlots.Count; i++)
            {
                SaveSlotState(plantaSlots[i], i);
            }
        }
    }

    private void OnApplicationQuit()
    {
        SaveAllSlotsState();
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

    public void TryPlantSeed(PlantaTipo plantaTipo)
    {
        if (!CanPlantInSlot(plantaTipo))
        {
            Debug.LogWarning($"No hay slots disponibles para plantar {plantaTipo}");
            return;
        }

        PlantData plantData = plantDatabase.GetPlantas(plantaTipo);
        if (BarraEnergiaSistema.Instance.CanPlant(plantData.energiaConsumo) == false) { return; }

        PlantSeed(plantData);
    }

    public void PlantSeed(PlantData plantData)
    {
        PlantaTipo plantaTipo = plantData.plantaTipo;
        CultivoSlotInfo firstAvailableSlot = null;
        var allPlantSlotsOfType = allPlantSlots[plantaTipo];
        Debug.Log($"[InvernaderoManager] Buscando slot disponible para {plantaTipo} entre {allPlantSlotsOfType.Count} slots");
        firstAvailableSlot = allPlantSlotsOfType.Find(slot => !slot.isOccupied);

        if (firstAvailableSlot == null) { throw new System.Exception($"No hay slots disponibles para plantar {plantaTipo}"); }

        firstAvailableSlot.isOccupied = true;
        float modifiedGrowthTime = BarraEnergiaSistema.Instance.GetModifiedGrowthTime(plantData.tiempoCrecimientoMinutos * 60);
        firstAvailableSlot.timer.Start(modifiedGrowthTime);
        InventorySystem.Instance.RemoveSeed(plantaTipo, 1);

        BarraEnergiaSistema.Instance.ConsumeEnergy(plantData.energiaConsumo);
    }

    public void CosecharPlanta(CultivoSlotInfo slotInfo)
    {
        if (!slotInfo.isOccupied || !slotInfo.timer.hasFinished)
        {
            Debug.LogWarning("No se puede cosechar: Slot no ocupado o planta no lista");
            return;
        }

        PlantData plantData = plantDatabase.GetPlantas(slotInfo.plantaTipo);
        InventorySystem.Instance.AddPlant(plantData.plantaTipo, PlantaCalidad.Estandar, plantData.cosechaCantidad);
        slotInfo.isOccupied = false;
        slotInfo.timer.Reset(0);

        BarraEnergiaSistema.Instance.RestoreEnergy(plantData.energiaConsumo);
    }

    private string GetCultivoSlotKey(CultivoSlotInfo cultivoSlotInfo, int index)
    {
        PlantData plantData = plantDatabase.GetPlantas(cultivoSlotInfo.plantaTipo);
        return $"PlantSlot_{plantData.plantaTipo}_{index}";
    }

    void SaveSlotState(CultivoSlotInfo cultivoSlotInfo, int index)
    {
        PlantData plantData = plantDatabase.GetPlantas(cultivoSlotInfo.plantaTipo);
        string key = GetCultivoSlotKey(cultivoSlotInfo, index);
        PlayerPrefs.SetInt($"{key}_Occupied", cultivoSlotInfo.isOccupied ? 1 : 0);
        // PlayerPrefs.SetInt($"{key}_Ready", cultivoSlotInfo.isReady ? 1 : 0);
        PlayerPrefs.SetInt($"{key}_PlantType", (int)cultivoSlotInfo.plantaTipo);
        // PlayerPrefs.SetInt($"{key}_SeedCycle", (int)cultivoSlotInfo.???);
        PlayerPrefs.SetFloat($"{key}_TimeLeft", cultivoSlotInfo.timer.TimeLeft); // ISO 8601
        PlayerPrefs.Save();
    }

    void LoadSlotState(CultivoSlotInfo cultivoSlotInfo, int index)
    {
        string key = GetCultivoSlotKey(cultivoSlotInfo, index);
        PlantData plantData = plantDatabase.GetPlantas(cultivoSlotInfo.plantaTipo);

        if (PlayerPrefs.HasKey($"{key}_Occupied"))
        {
            cultivoSlotInfo.isOccupied = PlayerPrefs.GetInt($"{key}_Occupied") == 1;
            cultivoSlotInfo.plantaTipo = (PlantaTipo)PlayerPrefs.GetInt($"{key}_PlantType");
            // cultivoSlotInfo.seedCycle = (SemillaCiclo)PlayerPrefs.GetInt($"{key}_SeedCycle");

            float timeLeft = PlayerPrefs.GetFloat($"{key}_TimeLeft");
            if (timeLeft > 0)
            {
                cultivoSlotInfo.timer.Start(timeLeft);
            }
            else
            {
                cultivoSlotInfo.timer.Reset(0);
            }

            Debug.Log($"Slot cargado: {cultivoSlotInfo.plantaTipo} - Ocupado: {cultivoSlotInfo.isOccupied} - Listo: {cultivoSlotInfo.timer.hasFinished} - Tiempo restante: {cultivoSlotInfo.timer.TimeLeft}");
        }
    }
}