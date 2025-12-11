using UnityEngine;
using System.Collections.Generic;


public class InvernaderoManager : MonoBehaviour
{
    public static InvernaderoManager Instance { get; private set; }

    [Header("Referencias")]
    public PlantaBD plantDatabase;

    [Header("Áreas de Cultivo - 4 slots por área")]
    [Tooltip("RQF36: 6 áreas de cultivo, una para cada tipo de planta")]
    public List<PlantaSlot> luminaSlots = new List<PlantaSlot>();
    public List<PlantaSlot> falsibayaSlots = new List<PlantaSlot>();
    public List<PlantaSlot> drakoniaSlots = new List<PlantaSlot>();
    public List<PlantaSlot> eldebriaSlots = new List<PlantaSlot>();
    public List<PlantaSlot> jiveriaSlots = new List<PlantaSlot>();
    public List<PlantaSlot> lirienSlots = new List<PlantaSlot>();

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
        InitializeSlots();
        ValidateSlots();

        Debug.Log("InvernaderoManager inicializado");
    }


    void InitializeSlots()
    {
        // RQNF36.2: Cada área solo acepta su tipo de planta específico
        AssignPlantTypeToSlots(luminaSlots, PlantaTipo.Lumina);
        AssignPlantTypeToSlots(falsibayaSlots, PlantaTipo.Falsibaya);
        AssignPlantTypeToSlots(drakoniaSlots, PlantaTipo.Drakonia);
        AssignPlantTypeToSlots(eldebriaSlots, PlantaTipo.Eldebria);
        AssignPlantTypeToSlots(jiveriaSlots, PlantaTipo.Jiveria);
        AssignPlantTypeToSlots(lirienSlots, PlantaTipo.Lirien);
    }


    void AssignPlantTypeToSlots(List<PlantaSlot> slots, PlantaTipo type)
    {
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                slot.slotPlantType = type;
            }
        }
    }


    void ValidateSlots()
    {
        ValidateSlotArea(luminaSlots, "Lumina");
        ValidateSlotArea(falsibayaSlots, "Falsibaya");
        ValidateSlotArea(drakoniaSlots, "Drakonia");
        ValidateSlotArea(eldebriaSlots, "Eldebria");
        ValidateSlotArea(jiveriaSlots, "Jiveria");
        ValidateSlotArea(lirienSlots, "Lirien");
    }

    void ValidateSlotArea(List<PlantaSlot> slots, string areaName)
    {
        if (slots.Count > 4)
        {
            Debug.LogError($"[VALIDACIÓN] El área {areaName} tiene {slots.Count} slots. MÁXIMO PERMITIDO: 4");
        }
        else if (slots.Count < 4)
        {
            Debug.LogWarning($"[VALIDACIÓN] El área {areaName} tiene {slots.Count} slots. Se recomienda 4.");
        }
        else
        {
            Debug.Log($"[VALIDACIÓN] Área {areaName}: {slots.Count} slots (correcto)");
        }

 
        int nullCount = 0;
        foreach (var slot in slots)
        {
            if (slot == null) nullCount++;
        }

        if (nullCount > 0)
        {
            Debug.LogError($"[VALIDACIÓN] El área {areaName} tiene {nullCount} slots NULL. Asígnalos en el Inspector.");
        }
    }


    public int GetActiveSlots(PlantaTipo type)
    {
        List<PlantaSlot> slots = GetSlotsForPlantType(type);
        int count = 0;

        foreach (var slot in slots)
        {
            if (slot != null && slot.isOccupied)
                count++;
        }

        return count;
    }

    /// <summary>
    /// RQF37: Obtiene el número de slots disponibles (vacíos) en un área
    /// </summary>
    public int GetAvailableSlots(PlantaTipo type)
    {
        List<PlantaSlot> slots = GetSlotsForPlantType(type);
        int count = 0;

        foreach (var slot in slots)
        {
            if (slot != null && slot.CanPlant())
                count++;
        }

        return count;
    }

    /// <summary>
    /// Obtiene el número de slots con plantas listas para cosechar
    /// </summary>
    public int GetReadySlots(PlantaTipo type)
    {
        List<PlantaSlot> slots = GetSlotsForPlantType(type);
        int count = 0;

        foreach (var slot in slots)
        {
            if (slot != null && slot.IsReady())
                count++;
        }

        return count;
    }

    /// <summary>
    /// RQF40: Planta una semilla en el siguiente slot disponible del área correspondiente
    /// </summary>
    public bool PlantInNextAvailableSlot(PlantaTipo type)
    {
        List<PlantaSlot> slots = GetSlotsForPlantType(type);

        // Buscar el primer slot disponible
        foreach (var slot in slots)
        {
            if (slot != null && slot.CanPlant())
            {
                bool success = slot.PlantSeed(type);

                if (success)
                {
                    Debug.Log($"Plantado {type} en slot disponible");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"No se pudo plantar {type} en el slot");
                }
            }
        }

        Debug.LogWarning($"No hay slots disponibles para {type}");
        return false;
    }

    /// <summary>
    /// Obtiene la lista de slots correspondiente a un tipo de planta
    /// </summary>
    List<PlantaSlot> GetSlotsForPlantType(PlantaTipo type)
    {
        switch (type)
        {
            case PlantaTipo.Lumina: return luminaSlots;
            case PlantaTipo.Falsibaya: return falsibayaSlots;
            case PlantaTipo.Drakonia: return drakoniaSlots;
            case PlantaTipo.Eldebria: return eldebriaSlots;
            case PlantaTipo.Jiveria: return jiveriaSlots;
            case PlantaTipo.Lirien: return lirienSlots;
            default:
                Debug.LogError($"Tipo de planta no reconocido: {type}");
                return new List<PlantaSlot>();
        }
    }

    /// <summary>
    /// Obtiene el total de plantas activas en todo el invernadero
    /// </summary>
    public int GetTotalActivePlants()
    {
        int total = 0;
        total += GetActiveSlots(PlantaTipo.Lumina);
        total += GetActiveSlots(PlantaTipo.Falsibaya);
        total += GetActiveSlots(PlantaTipo.Drakonia);
        total += GetActiveSlots(PlantaTipo.Eldebria);
        total += GetActiveSlots(PlantaTipo.Jiveria);
        total += GetActiveSlots(PlantaTipo.Lirien);
        return total;
    }

    /// <summary>
    /// RQF41: Cosecha todas las plantas listas en el invernadero
    /// </summary>
    public void HarvestAll()
    {
        int harvested = 0;

        harvested += HarvestAllFromArea(luminaSlots);
        harvested += HarvestAllFromArea(falsibayaSlots);
        harvested += HarvestAllFromArea(drakoniaSlots);
        harvested += HarvestAllFromArea(eldebriaSlots);
        harvested += HarvestAllFromArea(jiveriaSlots);
        harvested += HarvestAllFromArea(lirienSlots);

        Debug.Log($"Cosechadas {harvested} plantas en total");
    }

    /// <summary>
    /// Cosecha todas las plantas listas de un área específica
    /// </summary>
    int HarvestAllFromArea(List<PlantaSlot> slots)
    {
        int count = 0;

        foreach (var slot in slots)
        {
            if (slot != null && slot.isOccupied && slot.IsReady())
            {
                slot.Harvest();
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Cosecha todas las plantas de un tipo específico
    /// </summary>
    public void HarvestAllOfType(PlantaTipo type)
    {
        List<PlantaSlot> slots = GetSlotsForPlantType(type);
        int count = HarvestAllFromArea(slots);

        Debug.Log($"Cosechadas {count} plantas de {type}");
    }

    #region MÉTODOS DE DEBUGGING

    [ContextMenu("Validar Configuración de Slots")]
    public void ValidateConfiguration()
    {
        Debug.Log("=== VALIDACIÓN DE CONFIGURACIÓN ===");
        ValidateSlots();

        Debug.Log("\n--- ESTADO ACTUAL ---");
        Debug.Log($"Total de plantas activas: {GetTotalActivePlants()}");

        foreach (PlantaTipo type in System.Enum.GetValues(typeof(PlantaTipo)))
        {
            int active = GetActiveSlots(type);
            int available = GetAvailableSlots(type);
            int ready = GetReadySlots(type);
            Debug.Log($"{type}: {active} activos, {ready} listos, {available} disponibles");
        }
    }

    [ContextMenu("Plantar Una de Cada Tipo")]
    public void PlantOneOfEach()
    {
        Debug.Log("=== PLANTANDO UNA DE CADA TIPO ===");

        foreach (PlantaTipo type in System.Enum.GetValues(typeof(PlantaTipo)))
        {
            PlantInNextAvailableSlot(type);
        }
    }

    [ContextMenu("Llenar Todo el Invernadero")]
    public void FillGreenhouse()
    {
        Debug.Log("=== LLENANDO INVERNADERO ===");

        foreach (PlantaTipo type in System.Enum.GetValues(typeof(PlantaTipo)))
        {
            // Intentar llenar los 4 slots de cada área
            for (int i = 0; i < 4; i++)
            {
                if (!PlantInNextAvailableSlot(type))
                    break; // No hay más slots disponibles
            }
        }

        Debug.Log($"Invernadero llenado. Total plantas: {GetTotalActivePlants()}");
    }

    [ContextMenu("Cosechar Todo")]
    public void HarvestAllDebug()
    {
        HarvestAll();
    }

    [ContextMenu("Mostrar Estado del Invernadero")]
    public void ShowGreenhouseState()
    {
        Debug.Log("=== ESTADO DEL INVERNADERO ===");
        Debug.Log($"Total de plantas activas: {GetTotalActivePlants()}/24");

        foreach (PlantaTipo type in System.Enum.GetValues(typeof(PlantaTipo)))
        {
            int active = GetActiveSlots(type);
            int ready = GetReadySlots(type);
            int available = GetAvailableSlots(type);
            int total = active + available;

            Debug.Log($"\n{type}:");
            Debug.Log($"  Slots totales: {total}");
            Debug.Log($"  Activos: {active}");
            Debug.Log($"  Listos para cosechar: {ready}");
            Debug.Log($"  Disponibles: {available}");

            // Mostrar estado de cada slot
            List<PlantaSlot> slots = GetSlotsForPlantType(type);
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    string status = slots[i].isOccupied ?
                        (slots[i].IsReady() ? "LISTO " : "CRECIENDO...") :
                        "VACÍO";
                    Debug.Log($"    Slot {i + 1}: {status}");
                }
            }
        }
    }

    #endregion
}