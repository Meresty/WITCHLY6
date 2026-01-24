using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class InventoryItem
{
    public PlantaTipo plantaTipo;
    public PlantaCalidad calidad;
    public int cantidad;
}

[System.Serializable]
public class SerumItem
{
    public string sueroNombre;
    public int cantidad;
}

[System.Serializable]
public class SeedItem
{
    public PlantaTipo plantaTipo;
    public int cantidad; // -1 = infinito
}

[System.Serializable]
public class PlantaItemSOMapping
{
    public PlantaTipo tipo;
    public PlantaCalidad calidad;
    public ItemSO itemSO;
}

[System.Serializable]
public class SueroItemSOMapping
{
    public string sueroNombre;
    public ItemSO itemSO;
}

[System.Serializable] public class PlantsList { public List<InventoryItem> items; }
[System.Serializable] public class SeedsList { public List<SeedItem> items; }
[System.Serializable] public class SerumsList { public List<SerumItem> items; }

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Header("Inventario")]
    public List<InventoryItem> plantas = new List<InventoryItem>();
    public List<SeedItem> semillas = new List<SeedItem>();
    public List<SerumItem> sueros = new List<SerumItem>();
    public int coins = 0;

    [Header("Referencias")]
    public PlantaBD plantBD;
    public SueroDB sueroBD;

    [Header("Conexion con Caldero")]
    public PlantaItemSOMapping[] plantaToItemMapping;
    public SueroItemSOMapping[] sueroToItemMapping;

    public event Action OnInventoryChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeInventory()
    {
        if (!PlayerPrefs.HasKey("FirstTime"))
        {
            AddSerum("Suero de Fuerza", 1);
            AddSerum("Suero de Energía", 1);

            AddPlant(PlantaTipo.Drakonia, PlantaCalidad.Estandar, 5);
            AddPlant(PlantaTipo.Drakonia, PlantaCalidad.Plata, 5);
            AddPlant(PlantaTipo.Falsibaya, PlantaCalidad.Estandar, 5);
            AddPlant(PlantaTipo.Drakonia, PlantaCalidad.Oro, 5);

            AddSemilla(PlantaTipo.Falsibaya, -1);
            AddSemilla(PlantaTipo.Drakonia, -1);

            PlayerPrefs.SetInt("FirstTime", 1);
            PlayerPrefs.Save();
        }
        else
        {
            LoadInventory();
        }

        SyncAllToCaldero();
        OnInventoryChanged?.Invoke();
    }

    // -------------------------
    // COMPAT: lo que te falta
    // -------------------------
    public bool HasSeed(PlantaTipo type, int amount = 1)
    {
        var seed = semillas.Find(s => s.plantaTipo == type);
        if (seed == null) return false;
        if (seed.cantidad == -1) return true;
        return seed.cantidad >= amount;
    }

    public int GetSeedCount(PlantaTipo type)
    {
        var seed = semillas.Find(s => s.plantaTipo == type);
        return seed != null ? seed.cantidad : 0;
    }

    public bool HasPlant(PlantaTipo type, PlantaCalidad quality, int amount = 1)
    {
        var plant = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        return plant != null && plant.cantidad >= amount;
    }

    public bool HasPlant(PlantaTipo type)
    {
        return plantas.Exists(p => p.plantaTipo == type && p.cantidad > 0);
    }

    public int GetPlantCount(PlantaTipo type, PlantaCalidad quality)
    {
        var plant = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        return plant != null ? plant.cantidad : 0;
    }

    // Para scripts viejos (CalderoDangerZone, etc.)
    public ItemSO ResolveItemSO(ItemInfo info)
    {
        if (info == null) return null;

        // Planta (tiene tipo y calidad)
        if (info.plantaTipo != PlantaTipo.NONE && info.calidad != PlantaCalidad.NONE)
            return GetItemSOForPlant(info.plantaTipo, info.calidad);

        // Suero (por nombre)
        if (info.plantaTipo == PlantaTipo.NONE)
            return GetItemSOForSerum(info.itemNombre);

        // Semilla: normalmente no hay ItemSO (regresa null => incorrect drop)
        return null;
    }

    // -------------------------
    // MAPS
    // -------------------------
    public ItemSO GetItemSOForPlant(PlantaTipo tipo, PlantaCalidad calidad)
    {
        if (plantaToItemMapping == null) return null;
        foreach (var m in plantaToItemMapping)
            if (m.tipo == tipo && m.calidad == calidad)
                return m.itemSO;
        return null;
    }

    public ItemSO GetItemSOForSerum(string sueroNombre)
    {
        if (string.IsNullOrEmpty(sueroNombre)) return null;
        if (sueroToItemMapping == null) return null;

        foreach (var m in sueroToItemMapping)
            if (m != null && string.Equals(m.sueroNombre, sueroNombre, StringComparison.OrdinalIgnoreCase))
                return m.itemSO;

        return null;
    }

    public bool ConsumeMappedItem(ItemSO itemSO, int amount = 1)
    {
        if (itemSO == null) return false;

        if (plantaToItemMapping != null)
        {
            foreach (var m in plantaToItemMapping)
                if (m != null && m.itemSO == itemSO)
                    return RemovePlant(m.tipo, m.calidad, amount);
        }

        if (sueroToItemMapping != null)
        {
            foreach (var m in sueroToItemMapping)
                if (m != null && m.itemSO == itemSO)
                    return RemoveSerum(m.sueroNombre, amount);
        }

        if (InventoryManager.instancia != null)
        {
            InventoryManager.instancia.RemoveItem(itemSO, amount);
            return true;
        }

        return false;
    }

    // -------------------------
    // SEMILLAS
    // -------------------------
    public void AddSemilla(PlantaTipo type, int amount)
    {
        var existing = semillas.Find(s => s.plantaTipo == type);
        if (existing != null)
        {
            if (existing.cantidad == -1 || amount == -1)
                existing.cantidad = -1;
            else
                existing.cantidad += amount;
        }
        else
        {
            semillas.Add(new SeedItem { plantaTipo = type, cantidad = amount });
        }

        OnInventoryChanged?.Invoke();
        SaveInventory();
    }

    public bool RemoveSeed(PlantaTipo type, int amount = 1)
    {
        var seed = semillas.Find(s => s.plantaTipo == type);
        if (seed == null) return false;

        if (amount == -1)
        {
            seed.cantidad = -1;
            OnInventoryChanged?.Invoke();
            SaveInventory();
            return true;
        }

        if (seed.cantidad == -1) return true;
        if (seed.cantidad < amount) return false;

        seed.cantidad -= amount;
        if (seed.cantidad <= 0) semillas.Remove(seed);

        OnInventoryChanged?.Invoke();
        SaveInventory();
        return true;
    }

    // -------------------------
    // PLANTAS
    // -------------------------
    public void AddPlant(PlantaTipo type, PlantaCalidad quality, int amount)
    {
        var existing = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        if (existing != null) existing.cantidad += amount;
        else plantas.Add(new InventoryItem { plantaTipo = type, calidad = quality, cantidad = amount });

        ItemSO so = GetItemSOForPlant(type, quality);
        if (so != null && InventoryManager.instancia != null)
            InventoryManager.instancia.AddItem(so, amount);

        OnInventoryChanged?.Invoke();
        SaveInventory();
    }

    public bool RemovePlant(PlantaTipo type, PlantaCalidad quality, int amount = 1)
    {
        var plant = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        if (plant == null) return false;
        if (plant.cantidad < amount) return false;

        plant.cantidad -= amount;
        if (plant.cantidad <= 0) plantas.Remove(plant);

        ItemSO so = GetItemSOForPlant(type, quality);
        if (so != null && InventoryManager.instancia != null)
            InventoryManager.instancia.RemoveItem(so, amount);

        OnInventoryChanged?.Invoke();
        SaveInventory();
        return true;
    }

    // -------------------------
    // SUEROS
    // -------------------------
    public void AddSerum(string serumName, int amount)
    {
        var existing = sueros.Find(s => s.sueroNombre == serumName);
        if (existing != null) existing.cantidad += amount;
        else sueros.Add(new SerumItem { sueroNombre = serumName, cantidad = amount });

        ItemSO so = GetItemSOForSerum(serumName);
        if (so != null && InventoryManager.instancia != null)
            InventoryManager.instancia.AddItem(so, amount);

        OnInventoryChanged?.Invoke();
        SaveInventory();
    }

    public bool RemoveSerum(string serumName, int amount = 1)
    {
        var serum = sueros.Find(s => s.sueroNombre == serumName);
        if (serum == null) return false;
        if (serum.cantidad < amount) return false;

        serum.cantidad -= amount;
        if (serum.cantidad <= 0) sueros.Remove(serum);

        ItemSO so = GetItemSOForSerum(serumName);
        if (so != null && InventoryManager.instancia != null)
            InventoryManager.instancia.RemoveItem(so, amount);

        OnInventoryChanged?.Invoke();
        SaveInventory();
        return true;
    }

    public int GetSerumCount(string serumName)
    {
        var serum = sueros.Find(s => s.sueroNombre == serumName);
        return serum != null ? serum.cantidad : 0;
    }

    // -------------------------
    // MONEDAS
    // -------------------------
    public void AddCoins(int amount)
    {
        coins += amount;
        OnInventoryChanged?.Invoke();
        SaveInventory();
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount) return false;
        coins -= amount;
        OnInventoryChanged?.Invoke();
        SaveInventory();
        return true;
    }

    // -------------------------
    // VENTAS (para tus otros UIs)
    // -------------------------
    public void SellPlant(PlantaTipo type, PlantaCalidad quality, int amount)
    {
        if (plantBD == null) return;
        if (!RemovePlant(type, quality, amount)) return;

        PlantData data = plantBD.GetPlantas(type);
        if (data == null) return;

        int price = quality == PlantaCalidad.Estandar ? data.precioVentaEstandar :
                    quality == PlantaCalidad.Plata ? data.precioVentaPlata :
                    data.precioVentaOro;

        AddCoins(price * amount);
    }

    public void SellSeed(PlantaTipo type, int amount)
    {
        if (plantBD == null) return;
        if (!RemoveSeed(type, amount)) return;

        PlantData data = plantBD.GetPlantas(type);
        if (data == null) return;

        AddCoins(data.precioCompraEstandar * amount);
    }

    // -------------------------
    // SYNC
    // -------------------------
    void SyncAllToCaldero()
    {
        if (InventoryManager.instancia == null) return;

        foreach (var p in plantas)
        {
            ItemSO so = GetItemSOForPlant(p.plantaTipo, p.calidad);
            if (so != null) InventoryManager.instancia.AddItem(so, p.cantidad);
        }

        foreach (var s in sueros)
        {
            ItemSO so = GetItemSOForSerum(s.sueroNombre);
            if (so != null) InventoryManager.instancia.AddItem(so, s.cantidad);
        }
    }

    // -------------------------
    // SAVE/LOAD
    // -------------------------
    void SaveInventory()
    {
        PlayerPrefs.SetString("Plants", JsonUtility.ToJson(new PlantsList { items = plantas }));
        PlayerPrefs.SetString("Seeds", JsonUtility.ToJson(new SeedsList { items = semillas }));
        PlayerPrefs.SetString("Serums", JsonUtility.ToJson(new SerumsList { items = sueros }));
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.Save();
    }

    void LoadInventory()
    {
        if (PlayerPrefs.HasKey("Plants"))
        {
            var data = JsonUtility.FromJson<PlantsList>(PlayerPrefs.GetString("Plants"));
            plantas = data != null && data.items != null ? data.items : new List<InventoryItem>();
        }

        if (PlayerPrefs.HasKey("Seeds"))
        {
            var data = JsonUtility.FromJson<SeedsList>(PlayerPrefs.GetString("Seeds"));
            semillas = data != null && data.items != null ? data.items : new List<SeedItem>();
        }

        if (PlayerPrefs.HasKey("Serums"))
        {
            var data = JsonUtility.FromJson<SerumsList>(PlayerPrefs.GetString("Serums"));
            sueros = data != null && data.items != null ? data.items : new List<SerumItem>();
        }

        coins = PlayerPrefs.GetInt("Coins", 0);
    }
}
