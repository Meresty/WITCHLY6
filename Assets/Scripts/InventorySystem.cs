using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

#region Data Models

[Serializable]
public class InventoryItem
{
    public PlantaTipo plantaTipo;
    public PlantaCalidad calidad;
    public int cantidad;
}

[Serializable]
public class SerumItem
{
    public string sueroNombre;
    public int cantidad;
}

[Serializable]
public class SeedItem
{
    public PlantaTipo plantaTipo;
    public int cantidad; // -1 = infinito (no vender)
}

[Serializable]
public class PlantaItemSOMapping
{
    public PlantaTipo tipo;
    public PlantaCalidad calidad;
    public ItemSO itemSO;
}

[Serializable]
public class SueroItemSOMapping
{
    public string sueroNombre;
    public ItemSO itemSO;
}

[Serializable] public class PlantsList { public List<InventoryItem> items; }
[Serializable] public class SeedsList { public List<SeedItem> items; }
[Serializable] public class SerumsList { public List<SerumItem> items; }

#endregion

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Header("Inventario")]
    public List<InventoryItem> plantas = new List<InventoryItem>();
    public List<SeedItem> semillas = new List<SeedItem>();
    public List<SerumItem> sueros = new List<SerumItem>();

    [Header("Compat")]
    public int coins = 0;

    [Header("Referencias (UI usa estas BD para armar ItemInfo)")]
    public PlantaBD plantBD;
    public SueroDB sueroBD;

    [Header("Mapeo a ItemSO (Caldero)")]
    public PlantaItemSOMapping[] plantaToItemMapping;
    public SueroItemSOMapping[] sueroToItemMapping;

    [Header("Catalogo canonico (anti duplicados ItemSO)")]
    [SerializeField] private List<ItemSO> catalogoItems = new List<ItemSO>();

    [Header("Debug")]
    [SerializeField] private bool logResolve = false;

    public event Action OnInventoryChanged;

    private bool _isInitializing = false;

    // Keys (mantener para no romper guardado)
    private const string KEY_FIRST_TIME = "FirstTime";
    private const string KEY_PLANTS = "Plants";
    private const string KEY_SEEDS = "Seeds";
    private const string KEY_SERUMS = "Serums";
    private const string KEY_COINS = "Coins";

    // Canon cache
    private Dictionary<string, ItemSO> _canonByKey = new Dictionary<string, ItemSO>();

    // ===== PRECIOS DE VENTA (TABLA) =====
    private static readonly Dictionary<PlantaTipo, int> SELL_PLANTS = new Dictionary<PlantaTipo, int>
    {
        { PlantaTipo.Lumina,    13 },
        { PlantaTipo.Falsibaya,  8 },
        { PlantaTipo.Drakonia,   8 },
        { PlantaTipo.Eldebria,  10 },
        { PlantaTipo.Jiveria,   11 },
        { PlantaTipo.Lirien,    17 },
    };

    private static readonly Dictionary<PlantaTipo, int> SELL_SEEDS = new Dictionary<PlantaTipo, int>
    {
        { PlantaTipo.Lumina,    7 },
        { PlantaTipo.Eldebria,  5 },
        { PlantaTipo.Jiveria,   6 },
        { PlantaTipo.Lirien,    8 },
    };

    private static readonly Dictionary<string, int> SELL_SERUMS = new Dictionary<string, int>
    {
        { "suero de atadura",       19 },
        { "suero de fuerza",        17 },
        { "suero de congelamiento", 20 },
        { "suero de energia",       15 },
    };

    private void Awake()
    {
        // Singleton + absorcion de refs (evita que DontDestroy se quede sin BD)
        if (Instance != null && Instance != this)
        {
            Instance.AbsorbReferencesFrom(this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        RebuildCanonicalCache();
        InitializeInventory();
    }

    private void Start()
    {
        StartCoroutine(AutoBindFirebase());
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SyncAllToCalderoSafe();
        OnInventoryChanged?.Invoke();
    }

    private void AbsorbReferencesFrom(InventorySystem other)
    {
        if (other == null) return;

        if (plantBD == null && other.plantBD != null) plantBD = other.plantBD;
        if (sueroBD == null && other.sueroBD != null) sueroBD = other.sueroBD;

        if ((plantaToItemMapping == null || plantaToItemMapping.Length == 0) &&
            other.plantaToItemMapping != null && other.plantaToItemMapping.Length > 0)
            plantaToItemMapping = other.plantaToItemMapping;

        if ((sueroToItemMapping == null || sueroToItemMapping.Length == 0) &&
            other.sueroToItemMapping != null && other.sueroToItemMapping.Length > 0)
            sueroToItemMapping = other.sueroToItemMapping;

        if ((catalogoItems == null || catalogoItems.Count == 0) &&
            other.catalogoItems != null && other.catalogoItems.Count > 0)
            catalogoItems = other.catalogoItems;

        RebuildCanonicalCache();
    }

    private IEnumerator AutoBindFirebase()
    {
        // Si no existe en tu proyecto, comenta este bloque
        while (!FirebaseInitializer.IsReady) yield return null;
        while (GameSession.Instance == null || GameSession.Instance.CurrentUser == null) yield return null;

        string username = GameSession.Instance.CurrentUser.username;

        if (FirebaseCoinsManager.Instance == null)
        {
            var go = new GameObject("FirebaseCoinsManager");
            go.AddComponent<FirebaseCoinsManager>();
            yield return null;
        }

        FirebaseCoinsManager.Instance.BindUser(username);

        FirebaseCoinsManager.Instance.OnCoinsChanged += (c) => coins = c;
        coins = FirebaseCoinsManager.Instance.Coins;
    }

    private void InitializeInventory()
    {
        _isInitializing = true;

        if (!PlayerPrefs.HasKey(KEY_FIRST_TIME))
        {
            AddSemilla(PlantaTipo.Lumina, 5, save: false, notify: false);
            AddPlant(PlantaTipo.Lumina, PlantaCalidad.Estandar, 1, save: false, notify: false);
            AddSerum("Suero de Fuerza", 1, save: false, notify: false);

            PlayerPrefs.SetInt(KEY_FIRST_TIME, 1);
            PlayerPrefs.Save();
        }
        else
        {
            LoadInventory();
        }

        SaveInventory();
        _isInitializing = false;

        RebuildCanonicalCache();
        SyncAllToCalderoSafe();
        OnInventoryChanged?.Invoke();
    }

    // =========================
    // COMPAT METHODS
    // =========================
    public bool HasSeed(PlantaTipo type, int amount = 1)
    {
        var s = semillas.Find(x => x.plantaTipo == type);
        if (s == null) return false;
        if (s.cantidad == -1) return true;
        return s.cantidad >= amount;
    }

    public int GetSeedCount(PlantaTipo type)
    {
        var s = semillas.Find(x => x.plantaTipo == type);
        return s != null ? s.cantidad : 0;
    }

    public bool HasPlant(PlantaTipo type, PlantaCalidad quality, int amount = 1)
    {
        var p = plantas.Find(x => x.plantaTipo == type && x.calidad == quality);
        return p != null && p.cantidad >= amount;
    }

    public bool HasPlant(PlantaTipo type)
    {
        return plantas.Exists(x => x.plantaTipo == type && x.cantidad > 0);
    }

    public int GetPlantCount(PlantaTipo type, PlantaCalidad quality)
    {
        var p = plantas.Find(x => x.plantaTipo == type && x.calidad == quality);
        return p != null ? p.cantidad : 0;
    }

    public int GetPlantCount(PlantaTipo type)
    {
        int total = 0;
        foreach (var p in plantas)
            if (p.plantaTipo == type) total += p.cantidad;
        return total;
    }

    public int GetSerumCount(string serumName)
    {
        var s = sueros.Find(x => string.Equals(x.sueroNombre, serumName, StringComparison.OrdinalIgnoreCase));
        return s != null ? s.cantidad : 0;
    }

    public bool HasSerum(string serumName, int amount = 1)
    {
        return GetSerumCount(serumName) >= amount;
    }

    // =========================
    // ADD / REMOVE
    // =========================
    public void AddPlant(PlantaTipo type, PlantaCalidad quality, int amount, bool save = true, bool notify = true)
    {
        if (amount <= 0) return;

        var existing = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        if (existing != null) existing.cantidad += amount;
        else plantas.Add(new InventoryItem { plantaTipo = type, calidad = quality, cantidad = amount });

        if (!_isInitializing) SyncDeltaToCaldero(GetItemSOForPlant(type, quality), amount);

        if (save) SaveInventory();
        if (notify) OnInventoryChanged?.Invoke();
    }

    public bool RemovePlant(PlantaTipo type, PlantaCalidad quality, int amount = 1, bool save = true, bool notify = true)
    {
        if (amount <= 0) return true;

        var existing = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        if (existing == null || existing.cantidad < amount) return false;

        existing.cantidad -= amount;
        if (existing.cantidad <= 0) plantas.Remove(existing);

        SyncDeltaToCaldero(GetItemSOForPlant(type, quality), -amount);

        if (save) SaveInventory();
        if (notify) OnInventoryChanged?.Invoke();
        return true;
    }

    public void AddSemilla(PlantaTipo type, int amount, bool save = true, bool notify = true)
    {
        if (amount == 0) return;

        var existing = semillas.Find(s => s.plantaTipo == type);
        if (existing != null)
        {
            if (existing.cantidad == -1 || amount == -1) existing.cantidad = -1;
            else existing.cantidad += amount;
        }
        else
        {
            semillas.Add(new SeedItem { plantaTipo = type, cantidad = amount });
        }

        if (save) SaveInventory();
        if (notify) OnInventoryChanged?.Invoke();
    }

    public bool RemoveSeed(PlantaTipo type, int amount = 1, bool save = true, bool notify = true)
    {
        if (amount <= 0) return true;

        var existing = semillas.Find(s => s.plantaTipo == type);
        if (existing == null) return false;

        if (existing.cantidad == -1) return true;

        if (existing.cantidad < amount) return false;

        existing.cantidad -= amount;
        if (existing.cantidad <= 0) semillas.Remove(existing);

        if (save) SaveInventory();
        if (notify) OnInventoryChanged?.Invoke();
        return true;
    }

    public void AddSerum(string name, int amount, bool save = true, bool notify = true)
    {
        if (string.IsNullOrWhiteSpace(name) || amount <= 0) return;

        var existing = sueros.Find(s => string.Equals(s.sueroNombre, name, StringComparison.OrdinalIgnoreCase));
        if (existing != null) existing.cantidad += amount;
        else sueros.Add(new SerumItem { sueroNombre = name, cantidad = amount });

        if (!_isInitializing) SyncDeltaToCaldero(GetItemSOForSerum(name), amount);

        if (save) SaveInventory();
        if (notify) OnInventoryChanged?.Invoke();
    }

    public bool RemoveSerum(string name, int amount = 1, bool save = true, bool notify = true)
    {
        if (string.IsNullOrWhiteSpace(name) || amount <= 0) return true;

        var existing = sueros.Find(s => string.Equals(s.sueroNombre, name, StringComparison.OrdinalIgnoreCase));
        if (existing == null || existing.cantidad < amount) return false;

        existing.cantidad -= amount;
        if (existing.cantidad <= 0) sueros.Remove(existing);

        SyncDeltaToCaldero(GetItemSOForSerum(name), -amount);

        if (save) SaveInventory();
        if (notify) OnInventoryChanged?.Invoke();
        return true;
    }

    // =========================
    // COINS
    // =========================
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        if (FirebaseCoinsManager.Instance != null && GameSession.Instance != null && GameSession.Instance.CurrentUser != null)
        {
            if (!FirebaseCoinsManager.Instance.IsBound)
                FirebaseCoinsManager.Instance.BindUser(GameSession.Instance.CurrentUser.username);

            _ = FirebaseCoinsManager.Instance.AddCoinsAsync(amount);
            return;
        }

        coins += amount;
        SaveInventory();
        OnInventoryChanged?.Invoke();
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0) return true;

        if (FirebaseCoinsManager.Instance != null && GameSession.Instance != null && GameSession.Instance.CurrentUser != null)
        {
            if (!FirebaseCoinsManager.Instance.IsBound)
                FirebaseCoinsManager.Instance.BindUser(GameSession.Instance.CurrentUser.username);

            _ = FirebaseCoinsManager.Instance.TrySpendCoinsAsync(amount);
            return true;
        }

        if (coins < amount) return false;
        coins -= amount;
        SaveInventory();
        OnInventoryChanged?.Invoke();
        return true;
    }

    // =========================
    // SELL
    // =========================
    public int GetSellPrice(ItemInfo info)
    {
        if (info == null) return 0;

        if (info.plantaTipo == PlantaTipo.NONE)
        {
            string key = NormalizeKey(info.itemNombre);
            return SELL_SERUMS.TryGetValue(key, out int p) ? p : 0;
        }

        if (info.calidad != PlantaCalidad.NONE)
            return SELL_PLANTS.TryGetValue(info.plantaTipo, out int p2) ? p2 : 0;

        return SELL_SEEDS.TryGetValue(info.plantaTipo, out int p3) ? p3 : 0;
    }

    public async Task<bool> SellItemAsync(ItemInfo info, int amount = 1)
    {
        if (info == null) return false;
        if (amount <= 0) amount = 1;

        int unit = GetSellPrice(info);
        if (unit <= 0) return false;

        if (info.plantaTipo != PlantaTipo.NONE && info.calidad == PlantaCalidad.NONE)
        {
            var seed = semillas.Find(s => s.plantaTipo == info.plantaTipo);
            if (seed != null && seed.cantidad == -1) return false;
        }

        bool removed = false;

        if (info.plantaTipo == PlantaTipo.NONE)
            removed = RemoveSerum(info.itemNombre, amount, save: false, notify: false);
        else if (info.calidad != PlantaCalidad.NONE)
            removed = RemovePlant(info.plantaTipo, info.calidad, amount, save: false, notify: false);
        else
            removed = RemoveSeed(info.plantaTipo, amount, save: false, notify: false);

        if (!removed) return false;

        int total = unit * amount;

        if (FirebaseCoinsManager.Instance != null && GameSession.Instance != null && GameSession.Instance.CurrentUser != null)
        {
            if (!FirebaseCoinsManager.Instance.IsBound)
                FirebaseCoinsManager.Instance.BindUser(GameSession.Instance.CurrentUser.username);

            bool ok = await FirebaseCoinsManager.Instance.AddCoinsAsync(total);
            if (!ok)
            {
                if (info.plantaTipo == PlantaTipo.NONE) AddSerum(info.itemNombre, amount, save: false, notify: false);
                else if (info.calidad != PlantaCalidad.NONE) AddPlant(info.plantaTipo, info.calidad, amount, save: false, notify: false);
                else AddSemilla(info.plantaTipo, amount, save: false, notify: false);

                return false;
            }
        }
        else
        {
            coins += total;
        }

        SaveInventory();
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void SellPlant(PlantaTipo type, PlantaCalidad quality, int amount)
    {
        var temp = new ItemInfo { itemNombre = type.ToString(), plantaTipo = type, calidad = quality };
        _ = SellItemAsync(temp, amount);
    }

    public void SellSeed(PlantaTipo type, int amount)
    {
        var temp = new ItemInfo { itemNombre = type.ToString(), plantaTipo = type, calidad = PlantaCalidad.NONE };
        _ = SellItemAsync(temp, amount);
    }

    // =========================
    // CALDERO / MAPPING (FIXES IMPORTANTES)
    // =========================
    public ItemSO ResolveItemSO(ItemInfo info)
    {
        if (info == null) return null;

        // suero
        if (info.plantaTipo == PlantaTipo.NONE)
        {
            var so = GetItemSOForSerum(info.itemNombre);
            if (logResolve) Debug.Log($"[INV Resolve] SUERO name='{info.itemNombre}' => {(so ? so.name : "NULL")}");
            return so;
        }

        // planta con calidad
        if (info.calidad != PlantaCalidad.NONE)
        {
            var so = GetItemSOForPlant(info.plantaTipo, info.calidad);
            if (logResolve) Debug.Log($"[INV Resolve] PLANTA tipo={info.plantaTipo} calidad={info.calidad} => {(so ? so.name : "NULL")}");
            return so;
        }

        // planta sin calidad (fallback)
        var fallback = GetItemSOForPlantFallback(info.plantaTipo);
        if (logResolve) Debug.Log($"[INV Resolve] PLANTA-FALLBACK tipo={info.plantaTipo} calidad=NONE => {(fallback ? fallback.name : "NULL")}");
        return fallback;
    }

    public ItemSO GetItemSOForPlant(PlantaTipo tipo, PlantaCalidad calidad)
    {
        if (plantaToItemMapping == null) return null;

        for (int i = 0; i < plantaToItemMapping.Length; i++)
        {
            var m = plantaToItemMapping[i];
            if (m == null || m.itemSO == null) continue;
            if (m.tipo == tipo && m.calidad == calidad)
                return m.itemSO;
        }
        return null;
    }

    public ItemSO GetItemSOForPlantFallback(PlantaTipo tipo)
    {
        if (plantaToItemMapping == null) return null;

        // prioridad: estandar
        for (int i = 0; i < plantaToItemMapping.Length; i++)
        {
            var m = plantaToItemMapping[i];
            if (m == null || m.itemSO == null) continue;
            if (m.tipo == tipo && m.calidad == PlantaCalidad.Estandar)
                return m.itemSO;
        }

        // si no hay estandar, cualquier calidad
        for (int i = 0; i < plantaToItemMapping.Length; i++)
        {
            var m = plantaToItemMapping[i];
            if (m == null || m.itemSO == null) continue;
            if (m.tipo == tipo)
                return m.itemSO;
        }

        return null;
    }

    public ItemSO GetItemSOForSerum(string sueroNombre)
    {
        if (string.IsNullOrWhiteSpace(sueroNombre) || sueroToItemMapping == null) return null;

        string k = NormalizeItemKey(sueroNombre);

        for (int i = 0; i < sueroToItemMapping.Length; i++)
        {
            var m = sueroToItemMapping[i];
            if (m == null || m.itemSO == null) continue;

            if (NormalizeItemKey(m.sueroNombre) == k)
                return m.itemSO;
        }

        return null;
    }

    public bool ConsumeMappedItem(ItemSO itemSO, int amount = 1)
    {
        if (itemSO == null || amount <= 0) return false;

        itemSO = Canonicalize(itemSO);

        // plantas
        if (plantaToItemMapping != null)
        {
            foreach (var m in plantaToItemMapping)
            {
                if (m == null || m.itemSO == null) continue;
                if (Canonicalize(m.itemSO) == itemSO)
                    return RemovePlant(m.tipo, m.calidad, amount);
            }
        }

        // sueros
        if (sueroToItemMapping != null)
        {
            foreach (var m in sueroToItemMapping)
            {
                if (m == null || m.itemSO == null) continue;
                if (Canonicalize(m.itemSO) == itemSO)
                    return RemoveSerum(m.sueroNombre, amount);
            }
        }

        // fallback inventario viejo
        if (InventoryManager.instancia != null)
        {
            InventoryManager.instancia.RemoveItem(itemSO, amount);
            return true;
        }

        return false;
    }

    // =========================
    // SYNC con InventoryManager (viejo) para el caldero
    // =========================
    private void SyncDeltaToCaldero(ItemSO itemSO, int delta)
    {
        if (itemSO == null) return;
        if (InventoryManager.instancia == null) return;

        itemSO = Canonicalize(itemSO);

        var inv = InventoryManager.instancia.items.Find(i => i.item == itemSO);
        if (inv == null)
        {
            if (delta > 0) InventoryManager.instancia.AddItem(itemSO, delta);
            return;
        }

        inv.cantidad += delta;
        if (inv.cantidad <= 0) InventoryManager.instancia.items.Remove(inv);
    }

    public void SyncAllToCalderoSafe()
    {
        if (InventoryManager.instancia == null) return;

        Dictionary<ItemSO, int> expected = new Dictionary<ItemSO, int>();

        // plantas
        if (plantaToItemMapping != null)
        {
            foreach (var m in plantaToItemMapping)
            {
                if (m == null || m.itemSO == null) continue;

                int count = GetPlantCount(m.tipo, m.calidad);
                var canon = Canonicalize(m.itemSO);

                if (!expected.ContainsKey(canon)) expected[canon] = 0;
                expected[canon] += count;
            }
        }

        // sueros
        if (sueroToItemMapping != null)
        {
            foreach (var m in sueroToItemMapping)
            {
                if (m == null || m.itemSO == null) continue;

                int count = GetSerumCount(m.sueroNombre);
                var canon = Canonicalize(m.itemSO);

                if (!expected.ContainsKey(canon)) expected[canon] = 0;
                expected[canon] += count;
            }
        }

        foreach (var kv in expected)
            SetCalderoItemCount(kv.Key, kv.Value);
    }

    private void SetCalderoItemCount(ItemSO itemSO, int count)
    {
        if (InventoryManager.instancia == null) return;

        itemSO = Canonicalize(itemSO);

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

    // =========================
    // CANONICAL (anti duplicados)
    // =========================
    public void RebuildCanonicalCache()
    {
        _canonByKey.Clear();

        if (catalogoItems != null)
            foreach (var it in catalogoItems)
                AddCanon(it);

        if (plantaToItemMapping != null)
            foreach (var m in plantaToItemMapping)
                if (m != null) AddCanon(m.itemSO);

        if (sueroToItemMapping != null)
            foreach (var m in sueroToItemMapping)
                if (m != null) AddCanon(m.itemSO);
    }

    private void AddCanon(ItemSO it)
    {
        if (it == null) return;

        string key = NormalizeItemKey(it.name);
        if (string.IsNullOrEmpty(key)) return;

        if (!_canonByKey.ContainsKey(key))
            _canonByKey[key] = it;
    }

    public ItemSO Canonicalize(ItemSO item)
    {
        if (item == null) return null;

        string key = NormalizeItemKey(item.name);
        if (string.IsNullOrEmpty(key)) return item;

        if (_canonByKey != null && _canonByKey.TryGetValue(key, out var canon) && canon != null)
            return canon;

        return item;
    }

    // Normaliza duro para matching (mappings, clones, espacios, etc)
    public string NormalizeItemKey(string s)
    {
        s = NormalizeKey(s);
        s = s.Replace(" ", "").Replace("_", "").Replace("-", "");
        s = s.Replace("(clone)", "");
        return s;
    }

    // Quita acentos, trim, lower
    private string NormalizeKey(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        s = s.Trim().ToLowerInvariant();

        string formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);

        for (int i = 0; i < formD.Length; i++)
        {
            char ch = formD[i];
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    // =========================
    // SAVE / LOAD
    // =========================
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
}
