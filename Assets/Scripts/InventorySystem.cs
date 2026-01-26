using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

#region Data Models

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

#endregion

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

    [Header("Conexion con Caldero (ItemSO)")]
    public PlantaItemSOMapping[] plantaToItemMapping;
    public SueroItemSOMapping[] sueroToItemMapping;

    public event Action OnInventoryChanged;

    private bool _isInitializing = false;

    private const string KEY_FIRST_TIME = "FirstTime";
    private const string KEY_PLANTS = "Plants";
    private const string KEY_SEEDS = "Seeds";
    private const string KEY_SERUMS = "Serums";
    private const string KEY_COINS = "Coins";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        InitializeInventory();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cada que cambia escena, si ya existe InventoryManager, re-sincroniza seguro
        SyncAllToCalderoSafe();
    }

    private void InitializeInventory()
    {
        _isInitializing = true;

        if (!PlayerPrefs.HasKey(KEY_FIRST_TIME))
        {
            // Inventario inicial
            AddSerum("Suero de Fuerza", 1, save: false, notify: false);
            AddSerum("Suero de Energía", 1, save: false, notify: false);

            AddPlant(PlantaTipo.Drakonia, PlantaCalidad.Estandar, 5, save: false, notify: false);
            AddPlant(PlantaTipo.Drakonia, PlantaCalidad.Plata, 5, save: false, notify: false);
            AddPlant(PlantaTipo.Falsibaya, PlantaCalidad.Estandar, 5, save: false, notify: false);
            AddPlant(PlantaTipo.Drakonia, PlantaCalidad.Oro, 5, save: false, notify: false);

            AddSemilla(PlantaTipo.Falsibaya, -1, save: false, notify: false);
            AddSemilla(PlantaTipo.Drakonia, -1, save: false, notify: false);

            PlayerPrefs.SetInt(KEY_FIRST_TIME, 1);
            PlayerPrefs.Save();
        }
        else
        {
            LoadInventory();
        }

        SaveInventory();
        _isInitializing = false;

        // Sincroniza una vez (sin duplicar)
        SyncAllToCalderoSafe();

        OnInventoryChanged?.Invoke();
    }

    #region Compatibility (methods otros scripts ya llaman)

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

    // Overload comun (por si algun script lo llama asi)
    public int GetPlantCount(PlantaTipo type)
    {
        int total = 0;
        foreach (var p in plantas)
            if (p.plantaTipo == type) total += p.cantidad;
        return total;
    }

    public int GetSerumCount(string serumName)
    {
        var serum = sueros.Find(s => string.Equals(s.sueroNombre, serumName, StringComparison.OrdinalIgnoreCase));
        return serum != null ? serum.cantidad : 0;
    }

    public bool HasSerum(string serumName, int amount = 1)
    {
        return GetSerumCount(serumName) >= amount;
    }

    #endregion

    #region Mapping UI(ItemInfo) -> ItemSO (recetas)

    public ItemSO ResolveItemSO(ItemInfo info)
    {
        if (info == null) return null;

        // Planta (tiene tipo y calidad)
        if (info.plantaTipo != PlantaTipo.NONE && info.calidad != PlantaCalidad.NONE)
            return GetItemSOForPlant(info.plantaTipo, info.calidad);

        // Suero (por nombre)
        if (info.plantaTipo == PlantaTipo.NONE)
            return GetItemSOForSerum(info.itemNombre);

        // Semilla: normalmente no hay ItemSO para recetas del caldero
        return null;
    }

    public ItemSO GetItemSOForPlant(PlantaTipo tipo, PlantaCalidad calidad)
    {
        if (plantaToItemMapping == null) return null;

        foreach (var m in plantaToItemMapping)
        {
            if (m == null) continue;
            if (m.tipo == tipo && m.calidad == calidad)
                return m.itemSO;
        }
        return null;
    }

    public ItemSO GetItemSOForSerum(string sueroNombre)
    {
        if (string.IsNullOrEmpty(sueroNombre)) return null;
        if (sueroToItemMapping == null) return null;

        foreach (var m in sueroToItemMapping)
        {
            if (m == null) continue;
            if (string.Equals(m.sueroNombre, sueroNombre, StringComparison.OrdinalIgnoreCase))
                return m.itemSO;
        }
        return null;
    }

    #endregion

    #region Consume helpers (para caldero)

    // Descuenta usando ItemInfo (lo correcto para tu inventario real)
    public bool ConsumeItemInfo(ItemInfo info, int amount = 1)
    {
        if (info == null) return false;

        // Planta
        if (info.plantaTipo != PlantaTipo.NONE && info.calidad != PlantaCalidad.NONE)
            return RemovePlant(info.plantaTipo, info.calidad, amount);

        // Semilla
        if (info.plantaTipo != PlantaTipo.NONE && info.calidad == PlantaCalidad.NONE)
            return RemoveSeed(info.plantaTipo, amount);

        // Suero
        if (info.plantaTipo == PlantaTipo.NONE)
            return RemoveSerum(info.itemNombre, amount);

        return false;
    }

    // Descuenta usando ItemSO (fallback / compat)
    public bool ConsumeMappedItem(ItemSO itemSO, int amount = 1)
    {
        if (itemSO == null) return false;

        // Planta
        if (plantaToItemMapping != null)
        {
            foreach (var m in plantaToItemMapping)
            {
                if (m == null || m.itemSO == null) continue;
                if (m.itemSO == itemSO)
                    return RemovePlant(m.tipo, m.calidad, amount);
            }
        }

        // Suero
        if (sueroToItemMapping != null)
        {
            foreach (var m in sueroToItemMapping)
            {
                if (m == null || m.itemSO == null) continue;
                if (m.itemSO == itemSO)
                    return RemoveSerum(m.sueroNombre, amount);
            }
        }

        // Si no esta mapeado, al menos intenta bajar del InventoryManager
        if (InventoryManager.instancia != null)
        {
            InventoryManager.instancia.RemoveItem(itemSO, amount);
            return true;
        }

        return false;
    }

    #endregion

    #region Semillas

    public void AddSemilla(PlantaTipo type, int amount, bool save = true, bool notify = true)
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

        if (save) SaveInventory();
        if (notify) OnInventoryChanged?.Invoke();
    }

    public bool RemoveSeed(PlantaTipo type, int amount = 1)
    {
        var seed = semillas.Find(s => s.plantaTipo == type);
        if (seed == null) return false;

        // infinito
        if (seed.cantidad == -1) return true;

        if (amount <= 0) return true;
        if (seed.cantidad < amount) return false;

        seed.cantidad -= amount;
        if (seed.cantidad <= 0) semillas.Remove(seed);

        SaveInventory();
        OnInventoryChanged?.Invoke();
        return true;
    }

    #endregion

    #region Plantas

    public void AddPlant(PlantaTipo type, PlantaCalidad quality, int amount, bool save = true, bool notify = true)
    {
        var existing = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        if (existing != null) existing.cantidad += amount;
        else plantas.Add(new InventoryItem { plantaTipo = type, calidad = quality, cantidad = amount });

        // Sync delta a caldero (solo si NO estamos inicializando para evitar duplicar)
        if (!_isInitializing) SyncDeltaToCaldero(GetItemSOForPlant(type, quality), amount);

        if (save) SaveInventory();
        if (notify) OnInventoryChanged?.Invoke();
    }

    public bool RemovePlant(PlantaTipo type, PlantaCalidad quality, int amount = 1)
    {
        var plant = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        if (plant == null) return false;

        if (amount <= 0) return true;
        if (plant.cantidad < amount) return false;

        plant.cantidad -= amount;
        if (plant.cantidad <= 0) plantas.Remove(plant);

        SyncDeltaToCaldero(GetItemSOForPlant(type, quality), -amount);

        SaveInventory();
        OnInventoryChanged?.Invoke();
        return true;
    }

    #endregion

    #region Sueros

    public void AddSerum(string serumName, int amount, bool save = true, bool notify = true)
    {
        if (string.IsNullOrEmpty(serumName)) return;

        var existing = sueros.Find(s => string.Equals(s.sueroNombre, serumName, StringComparison.OrdinalIgnoreCase));
        if (existing != null) existing.cantidad += amount;
        else sueros.Add(new SerumItem { sueroNombre = serumName, cantidad = amount });

        if (!_isInitializing) SyncDeltaToCaldero(GetItemSOForSerum(serumName), amount);

        if (save) SaveInventory();
        if (notify) OnInventoryChanged?.Invoke();
    }

    public bool RemoveSerum(string serumName, int amount = 1)
    {
        if (string.IsNullOrEmpty(serumName)) return false;

        var serum = sueros.Find(s => string.Equals(s.sueroNombre, serumName, StringComparison.OrdinalIgnoreCase));
        if (serum == null) return false;

        if (amount <= 0) return true;
        if (serum.cantidad < amount) return false;

        serum.cantidad -= amount;
        if (serum.cantidad <= 0) sueros.Remove(serum);

        SyncDeltaToCaldero(GetItemSOForSerum(serumName), -amount);

        SaveInventory();
        OnInventoryChanged?.Invoke();
        return true;
    }

    #endregion

    #region Monedas

    public void AddCoins(int amount)
    {
        coins += amount;
        SaveInventory();
        OnInventoryChanged?.Invoke();
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount) return false;
        coins -= amount;
        SaveInventory();
        OnInventoryChanged?.Invoke();
        return true;
    }

    #endregion

    #region Compra/Venta (para tus UIs de mercado/invernadero)

    public void SellPlant(PlantaTipo type, PlantaCalidad quality, int amount)
    {
        if (amount <= 0) return;
        if (plantBD == null) return;

        if (!RemovePlant(type, quality, amount)) return;

        PlantData data = plantBD.GetPlantas(type);
        if (data == null) return;

        int price = (quality == PlantaCalidad.Estandar) ? data.precioVentaEstandar :
                    (quality == PlantaCalidad.Plata) ? data.precioVentaPlata :
                    data.precioVentaOro;

        AddCoins(price * amount);
    }

    public void SellSeed(PlantaTipo type, int amount)
    {
        if (amount <= 0) return;
        if (plantBD == null) return;

        if (!RemoveSeed(type, amount)) return;

        PlantData data = plantBD.GetPlantas(type);
        if (data == null) return;

        AddCoins(data.precioCompraEstandar * amount);
    }

    public bool BuyPlant(PlantaTipo type, PlantaCalidad quality, int amount = 1)
    {
        if (amount <= 0) return false;
        if (plantBD == null) return false;

        PlantData data = plantBD.GetPlantas(type);
        if (data == null) return false;

        int price = (quality == PlantaCalidad.Estandar) ? data.precioCompraEstandar :
                    (quality == PlantaCalidad.Plata) ? data.precioCompraPlata :
                    data.precioCompraOro;

        int total = price * amount;
        if (!SpendCoins(total)) return false;

        AddPlant(type, quality, amount);
        return true;
    }

    public bool BuySeed(PlantaTipo type, int amount = 1)
    {
        if (amount <= 0) return false;
        if (plantBD == null) return false;

        PlantData data = plantBD.GetPlantas(type);
        if (data == null) return false;

        int total = data.precioCompraEstandar * amount;
        if (!SpendCoins(total)) return false;

        AddSemilla(type, amount);
        return true;
    }

    #endregion

    #region Caldero Sync (sin duplicar)

    private void SyncDeltaToCaldero(ItemSO itemSO, int delta)
    {
        if (itemSO == null) return;
        if (InventoryManager.instancia == null) return;

        // Ajusta cantidad actual + delta sin duplicar por syncs globales
        var inv = InventoryManager.instancia.items.Find(i => i.item == itemSO);
        if (inv == null)
        {
            if (delta > 0)
                InventoryManager.instancia.AddItem(itemSO, delta);
            return;
        }

        inv.cantidad += delta;
        if (inv.cantidad <= 0)
            InventoryManager.instancia.items.Remove(inv);
    }

    // Esto "setea" cantidades para items mapeados, sin tocar otros (como pociones)
    public void SyncAllToCalderoSafe()
    {
        if (InventoryManager.instancia == null) return;

        // Calcula lo esperado por ItemSO
        Dictionary<ItemSO, int> expected = new Dictionary<ItemSO, int>();

        if (plantaToItemMapping != null)
        {
            foreach (var m in plantaToItemMapping)
            {
                if (m == null || m.itemSO == null) continue;
                int count = GetPlantCount(m.tipo, m.calidad);

                if (!expected.ContainsKey(m.itemSO)) expected[m.itemSO] = 0;
                expected[m.itemSO] += count;
            }
        }

        if (sueroToItemMapping != null)
        {
            foreach (var m in sueroToItemMapping)
            {
                if (m == null || m.itemSO == null) continue;
                int count = GetSerumCount(m.sueroNombre);

                if (!expected.ContainsKey(m.itemSO)) expected[m.itemSO] = 0;
                expected[m.itemSO] += count;
            }
        }

        // Aplica "set" solo a los items esperados
        foreach (var kv in expected)
        {
            SetCalderoItemCount(kv.Key, kv.Value);
        }

        // Si hay items mapeados que ya no deberian existir (count 0) tambien los baja
        // (solo los que esten en mapping)
        if (plantaToItemMapping != null)
        {
            foreach (var m in plantaToItemMapping)
            {
                if (m == null || m.itemSO == null) continue;
                if (!expected.ContainsKey(m.itemSO))
                    SetCalderoItemCount(m.itemSO, 0);
            }
        }

        if (sueroToItemMapping != null)
        {
            foreach (var m in sueroToItemMapping)
            {
                if (m == null || m.itemSO == null) continue;
                if (!expected.ContainsKey(m.itemSO))
                    SetCalderoItemCount(m.itemSO, 0);
            }
        }
    }

    private void SetCalderoItemCount(ItemSO itemSO, int count)
    {
        if (InventoryManager.instancia == null) return;

        var inv = InventoryManager.instancia.items.Find(i => i.item == itemSO);

        if (count <= 0)
        {
            if (inv != null) InventoryManager.instancia.items.Remove(inv);
            return;
        }

        if (inv == null)
        {
            InventoryManager.instancia.AddItem(itemSO, count);
            return;
        }

        inv.cantidad = count;
    }

    #endregion

    #region Save/Load

    private void SaveInventory()
    {
        PlayerPrefs.SetString(KEY_PLANTS, JsonUtility.ToJson(new PlantsList { items = plantas }));
        PlayerPrefs.SetString(KEY_SEEDS, JsonUtility.ToJson(new SeedsList { items = semillas }));
        PlayerPrefs.SetString(KEY_SERUMS, JsonUtility.ToJson(new SerumsList { items = sueros }));
        PlayerPrefs.SetInt(KEY_COINS, coins);
        PlayerPrefs.Save();
    }

    private void LoadInventory()
    {
        if (PlayerPrefs.HasKey(KEY_PLANTS))
        {
            var data = JsonUtility.FromJson<PlantsList>(PlayerPrefs.GetString(KEY_PLANTS));
            plantas = (data != null && data.items != null) ? data.items : new List<InventoryItem>();
        }
        else plantas = new List<InventoryItem>();

        if (PlayerPrefs.HasKey(KEY_SEEDS))
        {
            var data = JsonUtility.FromJson<SeedsList>(PlayerPrefs.GetString(KEY_SEEDS));
            semillas = (data != null && data.items != null) ? data.items : new List<SeedItem>();
        }
        else semillas = new List<SeedItem>();

        if (PlayerPrefs.HasKey(KEY_SERUMS))
        {
            var data = JsonUtility.FromJson<SerumsList>(PlayerPrefs.GetString(KEY_SERUMS));
            sueros = (data != null && data.items != null) ? data.items : new List<SerumItem>();
        }
        else sueros = new List<SerumItem>();

        coins = PlayerPrefs.GetInt(KEY_COINS, 0);
    }

    #endregion
}
