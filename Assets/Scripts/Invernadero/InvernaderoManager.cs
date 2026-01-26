using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CultivoSlotInfo
{
    public CultivoSlotInfo(PlantaTipo plantaTipo)
    {
        this.plantaTipo = plantaTipo;
        isOccupied = false;
        timer = new Timer();
    }

    public PlantaTipo plantaTipo;
    public bool isOccupied = false;
    public Timer timer;

    public bool isReady => timer != null && timer.hasFinished;

    public void EnsureTimer()
    {
        if (timer == null) timer = new Timer();
    }
}

public class InvernaderoManager : MonoBehaviour
{
    public static InvernaderoManager Instance { get; private set; }

    public PlantData currentParcelaPlantData = null;

    [Header("Referencias")]
    public PlantaBD plantDatabase;
    public SueroDB sueroDatabase;

    public List<CultivoSlotUI> cultivoSlotUIs = new List<CultivoSlotUI>();

    [Header("Areas de Cultivo")]
    [SerializeField] private List<CultivoSlotInfo> luminaSlots = new List<CultivoSlotInfo>();
    [SerializeField] private List<CultivoSlotInfo> falsibayaSlots = new List<CultivoSlotInfo>();
    [SerializeField] private List<CultivoSlotInfo> drakoniaSlots = new List<CultivoSlotInfo>();
    [SerializeField] private List<CultivoSlotInfo> eldebriaSlots = new List<CultivoSlotInfo>();
    [SerializeField] private List<CultivoSlotInfo> jiveriaSlots = new List<CultivoSlotInfo>();
    [SerializeField] private List<CultivoSlotInfo> lirienSlots = new List<CultivoSlotInfo>();

    public Dictionary<PlantaTipo, List<CultivoSlotInfo>> allPlantSlots;

    [Header("Config")]
    [SerializeField] private int slotsCountPorPlanta = 4;

    private bool _initialized = false;
    private bool _loadedPrefs = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureInitialized();
    }

    private void Start()
    {
        EnsureInitialized();
        Debug.Log("InvernaderoManager inicializado (safe)");
    }

    private void OnEnable()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            // aun asi, si por alguna razon el diccionario se perdio, lo rearmamos
            if (allPlantSlots == null) BuildDictionary();
            return;
        }

        BuildSlotsSafe();
        BuildDictionary();

        _initialized = true;

        if (!_loadedPrefs)
        {
            TryLoadAllSlotsState();
            _loadedPrefs = true;
        }
    }

    private void BuildSlotsSafe()
    {
        EnsureSlotList(luminaSlots, PlantaTipo.Lumina, slotsCountPorPlanta);
        EnsureSlotList(falsibayaSlots, PlantaTipo.Falsibaya, slotsCountPorPlanta);
        EnsureSlotList(drakoniaSlots, PlantaTipo.Drakonia, slotsCountPorPlanta);
        EnsureSlotList(eldebriaSlots, PlantaTipo.Eldebria, slotsCountPorPlanta);
        EnsureSlotList(jiveriaSlots, PlantaTipo.Jiveria, slotsCountPorPlanta);
        EnsureSlotList(lirienSlots, PlantaTipo.Lirien, slotsCountPorPlanta);
    }

    private void EnsureSlotList(List<CultivoSlotInfo> list, PlantaTipo tipo, int count)
    {
        if (list == null) list = new List<CultivoSlotInfo>(count);

        // si el tamaño no coincide, lo reconstruimos para evitar duplicados raros
        if (list.Count != count)
        {
            list.Clear();
            for (int i = 0; i < count; i++)
                list.Add(new CultivoSlotInfo(tipo));
            return;
        }

        // si coincide el size, aseguramos que no haya nulls y que cada slot tenga timer
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) list[i] = new CultivoSlotInfo(tipo);
            list[i].plantaTipo = tipo;
            list[i].EnsureTimer();
        }
    }

    private void BuildDictionary()
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

    private void Update()
    {
        // guard total para evitar el NullReference
        if (!_initialized) EnsureInitialized();
        if (allPlantSlots == null) return;

        foreach (var plantaSlots in allPlantSlots.Values)
        {
            if (plantaSlots == null) continue;

            for (int i = 0; i < plantaSlots.Count; i++)
            {
                var slot = plantaSlots[i];
                if (slot == null) continue;

                slot.EnsureTimer();
                slot.timer.Tick(Time.deltaTime);
            }
        }
    }

    private void OnApplicationQuit()
    {
        SaveAllSlotsState();
    }

    // -------------------------
    // Plantar / cosechar
    // -------------------------
    public bool CanPlantInSlot(PlantaTipo plantaTipo)
    {
        EnsureInitialized();

        if (allPlantSlots == null || !allPlantSlots.ContainsKey(plantaTipo))
            return false;

        var slots = allPlantSlots[plantaTipo];

        for (int i = 0; i < slots.Count; i++)
        {
            var s = slots[i];
            if (s != null && !s.isOccupied) return true;
        }
        return false;
    }

    public void TryPlantSeed(PlantaTipo plantaTipo)
    {
        EnsureInitialized();

        if (plantDatabase == null)
        {
            Debug.LogError("[Invernadero] plantDatabase NO asignado en Inspector.");
            return;
        }

        if (!CanPlantInSlot(plantaTipo))
        {
            Debug.LogWarning("[Invernadero] No hay slots disponibles para plantar: " + plantaTipo);
            return;
        }

        PlantData plantData = plantDatabase.GetPlantas(plantaTipo);
        if (plantData == null)
        {
            Debug.LogError("[Invernadero] PlantData null para: " + plantaTipo);
            return;
        }

        if (BarraEnergiaSistema.Instance == null)
        {
            Debug.LogError("[Invernadero] BarraEnergiaSistema.Instance es null.");
            return;
        }

        if (!BarraEnergiaSistema.Instance.CanPlant(plantData.energiaConsumo))
            return;

        PlantSeed(plantData);
    }

    public void PlantSeed(PlantData plantData)
    {
        EnsureInitialized();

        if (plantData == null) return;

        if (InventorySystem.Instance == null)
        {
            Debug.LogError("[Invernadero] InventorySystem.Instance es null.");
            return;
        }

        if (BarraEnergiaSistema.Instance == null)
        {
            Debug.LogError("[Invernadero] BarraEnergiaSistema.Instance es null.");
            return;
        }

        PlantaTipo plantaTipo = plantData.plantaTipo;

        if (!allPlantSlots.ContainsKey(plantaTipo))
        {
            Debug.LogError("[Invernadero] No existe lista de slots para: " + plantaTipo);
            return;
        }

        var list = allPlantSlots[plantaTipo];
        var slot = list.Find(s => s != null && !s.isOccupied);

        if (slot == null)
        {
            Debug.LogWarning("[Invernadero] No hay slot disponible para: " + plantaTipo);
            return;
        }

        slot.isOccupied = true;

        float baseSeconds = plantData.tiempoCrecimientoMinutos * 60f;
        float modifiedGrowthTime = BarraEnergiaSistema.Instance.GetModifiedGrowthTime(baseSeconds);

        slot.EnsureTimer();
        slot.timer.Start(modifiedGrowthTime);

        InventorySystem.Instance.RemoveSeed(plantaTipo, 1);
        BarraEnergiaSistema.Instance.ConsumeEnergy(plantData.energiaConsumo);
    }

    public void CosecharPlanta(CultivoSlotInfo slotInfo)
    {
        EnsureInitialized();

        if (slotInfo == null) return;

        slotInfo.EnsureTimer();
        if (!slotInfo.isOccupied || !slotInfo.timer.hasFinished)
        {
            Debug.LogWarning("[Invernadero] No se puede cosechar: slot no ocupado o no listo");
            return;
        }

        if (plantDatabase == null)
        {
            Debug.LogError("[Invernadero] plantDatabase NO asignado.");
            return;
        }

        if (InventorySystem.Instance == null)
        {
            Debug.LogError("[Invernadero] InventorySystem.Instance es null.");
            return;
        }

        if (BarraEnergiaSistema.Instance == null)
        {
            Debug.LogError("[Invernadero] BarraEnergiaSistema.Instance es null.");
            return;
        }

        PlantData plantData = plantDatabase.GetPlantas(slotInfo.plantaTipo);
        if (plantData == null)
        {
            Debug.LogError("[Invernadero] PlantData null en cosecha para: " + slotInfo.plantaTipo);
            return;
        }

        InventorySystem.Instance.AddPlant(plantData.plantaTipo, PlantaCalidad.Estandar, plantData.cosechaCantidad);

        slotInfo.isOccupied = false;
        slotInfo.timer.Reset(0);

        BarraEnergiaSistema.Instance.RestoreEnergy(plantData.energiaConsumo);
    }

    // -------------------------
    // Save/Load (PlayerPrefs)
    // -------------------------
    private string GetCultivoSlotKey(PlantaTipo tipo, int index)
    {
        return $"PlantSlot_{tipo}_{index}";
    }

    private void TryLoadAllSlotsState()
    {
        EnsureInitialized();

        foreach (var kv in allPlantSlots)
        {
            var tipo = kv.Key;
            var list = kv.Value;
            if (list == null) continue;

            for (int i = 0; i < list.Count; i++)
            {
                LoadSlotState(tipo, list[i], i);
            }
        }
    }

    private void SaveAllSlotsState()
    {
        if (allPlantSlots == null) return;

        foreach (var kv in allPlantSlots)
        {
            var tipo = kv.Key;
            var list = kv.Value;
            if (list == null) continue;

            for (int i = 0; i < list.Count; i++)
            {
                SaveSlotState(tipo, list[i], i);
            }
        }
    }

    private void SaveSlotState(PlantaTipo tipo, CultivoSlotInfo slot, int index)
    {
        if (slot == null) return;

        string key = GetCultivoSlotKey(tipo, index);

        PlayerPrefs.SetInt($"{key}_Occupied", slot.isOccupied ? 1 : 0);

        slot.EnsureTimer();
        PlayerPrefs.SetFloat($"{key}_TimeLeft", slot.timer.TimeLeft);

        PlayerPrefs.Save();
    }

    private void LoadSlotState(PlantaTipo tipo, CultivoSlotInfo slot, int index)
    {
        if (slot == null) return;

        string key = GetCultivoSlotKey(tipo, index);

        if (!PlayerPrefs.HasKey($"{key}_Occupied"))
            return;

        slot.plantaTipo = tipo;
        slot.isOccupied = PlayerPrefs.GetInt($"{key}_Occupied") == 1;

        float timeLeft = PlayerPrefs.GetFloat($"{key}_TimeLeft", 0f);

        slot.EnsureTimer();
        if (timeLeft > 0f) slot.timer.Start(timeLeft);
        else slot.timer.Reset(0);
    }
}
