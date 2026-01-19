using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

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
    public int cantidad; // -1 = infinito
}

#endregion

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Header("Inventario")]
    public List<InventoryItem> plantas = new List<InventoryItem>();
    public List<SeedItem> semillas = new List<SeedItem>();
    public List<SerumItem> sueros = new List<SerumItem>();
    public int coins = 0;

    [Header("BDs (ScriptableObjects)")]
    public PlantaBD plantBD;
    public SueroDB sueroBD;

    // Compat por si algún script tuyo usa estos nombres
    public PlantaBD PlantaBDRef => plantBD;
    public SueroDB SueroDBRef => sueroBD;

    public event Action OnInventoryChanged;

    // -------------------------
    // Singleton "scene-preferred"
    // (evita MissingReference cuando cargas escenas con otro InventorySystem)
    // -------------------------
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeInventory();
            RaiseChanged();
            return;
        }

        if (Instance != this)
        {
            // Mantén ESTE (el de la escena actual) para no romper referencias del Inspector/UI,
            // y destruye el anterior pero absorbiendo su data.
            AbsorbFrom(Instance);

            var old = Instance;
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (old != null && old.gameObject != null)
                Destroy(old.gameObject);

            RaiseChanged();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // -------------------------
    // Init / Save / Load
    // -------------------------
    private void InitializeInventory()
    {
        // Si quieres inventario inicial, descomenta y ajusta a tu gusto.
        // Si ya tienes guardado, esto no corre.
        if (!PlayerPrefs.HasKey("FirstTime"))
        {
            // Ejemplo (ajusta nombres EXACTOS a tus sueros si tienen acentos):
            // AddSerum("Suero de Fuerza", 1);
            // AddSerum("Suero de Energia", 1);
            // AddPlant(PlantaTipo.Drakonia, PlantaCalidad.Estandar, 5);
            // AddSemilla(PlantaTipo.Drakonia, -1);

            PlayerPrefs.SetInt("FirstTime", 1);
            PlayerPrefs.Save();

            SaveInventory();
        }
        else
        {
            LoadInventory();
        }
    }

    [Serializable] private class PlantsList { public List<InventoryItem> items; }
    [Serializable] private class SeedsList { public List<SeedItem> items; }
    [Serializable] private class SerumsList { public List<SerumItem> items; }

    private void SaveInventory()
    {
        PlayerPrefs.SetString("Plants", JsonUtility.ToJson(new PlantsList { items = plantas }));
        PlayerPrefs.SetString("Seeds", JsonUtility.ToJson(new SeedsList { items = semillas }));
        PlayerPrefs.SetString("Serums", JsonUtility.ToJson(new SerumsList { items = sueros }));
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.Save();
    }

    private void LoadInventory()
    {
        if (PlayerPrefs.HasKey("Plants"))
        {
            var data = JsonUtility.FromJson<PlantsList>(PlayerPrefs.GetString("Plants"));
            if (data != null && data.items != null) plantas = data.items;
        }

        if (PlayerPrefs.HasKey("Seeds"))
        {
            var data = JsonUtility.FromJson<SeedsList>(PlayerPrefs.GetString("Seeds"));
            if (data != null && data.items != null) semillas = data.items;
        }

        if (PlayerPrefs.HasKey("Serums"))
        {
            var data = JsonUtility.FromJson<SerumsList>(PlayerPrefs.GetString("Serums"));
            if (data != null && data.items != null) sueros = data.items;
        }

        coins = PlayerPrefs.GetInt("Coins", 0);
    }

    private void RaiseChanged()
    {
        OnInventoryChanged?.Invoke();
        SaveInventory();
    }

    // -------------------------
    // SEMILLAS
    // -------------------------
    public void AddSemilla(PlantaTipo type, int amount)
    {
        var existing = semillas.Find(s => s.plantaTipo == type);

        if (amount == -1)
        {
            if (existing == null) semillas.Add(new SeedItem { plantaTipo = type, cantidad = -1 });
            else existing.cantidad = -1;

            RaiseChanged();
            return;
        }

        if (existing != null)
        {
            if (existing.cantidad != -1) existing.cantidad += amount;
        }
        else
        {
            semillas.Add(new SeedItem { plantaTipo = type, cantidad = amount });
        }

        RaiseChanged();
    }

    public bool HasSeed(PlantaTipo type, int amount = 1)
    {
        var seed = semillas.Find(s => s.plantaTipo == type);
        if (seed == null) return false;
        if (seed.cantidad == -1) return true;
        return seed.cantidad >= amount;
    }

    public bool RemoveSeed(PlantaTipo type, int amount = 1)
    {
        var seed = semillas.Find(s => s.plantaTipo == type);
        if (seed == null) return false;

        if (seed.cantidad == -1) return true; // infinito

        if (seed.cantidad < amount) return false;

        seed.cantidad -= amount;
        if (seed.cantidad <= 0) semillas.Remove(seed);

        RaiseChanged();
        return true;
    }

    public int GetSeedCount(PlantaTipo type)
    {
        var seed = semillas.Find(s => s.plantaTipo == type);
        return seed != null ? seed.cantidad : 0;
    }

    // -------------------------
    // PLANTAS
    // -------------------------
    public void AddPlant(PlantaTipo type, PlantaCalidad quality, int amount)
    {
        var existing = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        if (existing != null) existing.cantidad += amount;
        else plantas.Add(new InventoryItem { plantaTipo = type, calidad = quality, cantidad = amount });

        RaiseChanged();
    }

    public bool HasPlant(PlantaTipo type, PlantaCalidad quality, int amount = 1)
    {
        var plant = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        return plant != null && plant.cantidad >= amount;
    }

    public bool RemovePlant(PlantaTipo type, PlantaCalidad quality, int amount = 1)
    {
        var plant = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        if (plant == null || plant.cantidad < amount) return false;

        plant.cantidad -= amount;
        if (plant.cantidad <= 0) plantas.Remove(plant);

        RaiseChanged();
        return true;
    }

    public int GetPlantCount(PlantaTipo type, PlantaCalidad quality)
    {
        var plant = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        return plant != null ? plant.cantidad : 0;
    }

    // -------------------------
    // SUEROS
    // -------------------------
    public void AddSerum(string serumName, int amount)
    {
        if (string.IsNullOrWhiteSpace(serumName)) return;

        var existing = sueros.Find(s => string.Equals(s.sueroNombre, serumName, StringComparison.OrdinalIgnoreCase));
        if (existing != null) existing.cantidad += amount;
        else sueros.Add(new SerumItem { sueroNombre = serumName, cantidad = amount });

        RaiseChanged();
    }

    public bool HasSerum(string serumName, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(serumName)) return false;

        var serum = sueros.Find(s => string.Equals(s.sueroNombre, serumName, StringComparison.OrdinalIgnoreCase));
        return serum != null && serum.cantidad >= amount;
    }

    public bool RemoveSerum(string serumName, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(serumName)) return false;

        var serum = sueros.Find(s => string.Equals(s.sueroNombre, serumName, StringComparison.OrdinalIgnoreCase));
        if (serum == null || serum.cantidad < amount) return false;

        serum.cantidad -= amount;
        if (serum.cantidad <= 0) sueros.Remove(serum);

        RaiseChanged();
        return true;
    }

    // Alias por compat si ya tenías UseSerum en otros scripts
    public bool UseSerum(string serumName, int amount = 1) => RemoveSerum(serumName, amount);

    public int GetSerumCount(string serumName)
    {
        var serum = sueros.Find(s => string.Equals(s.sueroNombre, serumName, StringComparison.OrdinalIgnoreCase));
        return serum != null ? serum.cantidad : 0;
    }

    // -------------------------
    // MONEDAS
    // -------------------------
    public void AddCoins(int amount)
    {
        coins += amount;
        RaiseChanged();
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount) return false;
        coins -= amount;
        RaiseChanged();
        return true;
    }

    // -------------------------
    // VENDER / COMPRAR
    // -------------------------
    public void SellPlant(PlantaTipo type, PlantaCalidad quality, int amount)
    {
        if (!RemovePlant(type, quality, amount)) return;

        int price = GetPlantPriceVenta(type, quality);
        AddCoins(price * amount);
    }

    public void SellSeed(PlantaTipo type, int amount)
    {
        if (!RemoveSeed(type, amount)) return;

        int price = GetPlantPriceCompra(type); // normalmente semilla usa compra/estandar
        AddCoins(price * amount);
    }

    public bool BuyPlant(PlantaTipo type, PlantaCalidad quality, int amount = 1)
    {
        int price = GetPlantPriceCompra(type, quality);
        int total = price * amount;

        if (!SpendCoins(total)) return false;

        AddPlant(type, quality, amount);
        return true;
    }

    public bool BuySeed(PlantaTipo type, int amount = 1)
    {
        int price = GetPlantPriceCompra(type);
        int total = price * amount;

        if (!SpendCoins(total)) return false;

        AddSemilla(type, amount);
        return true;
    }

    public bool SellSerum(string nombre, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(nombre) || amount <= 0) return false;
        if (!HasSerum(nombre, amount)) return false;

        int price = GetSueroPrecioVenta(nombre);

        RemoveSerum(nombre, amount);
        AddCoins(price * amount);

        UnityEngine.Debug.Log($"Vendidos {amount} sueros {nombre} por {price * amount} coins");
        return true;
    }

    // -------------------------
    // PRICE LOOKUP (reflection-safe)
    // -------------------------
    private int GetPlantPriceVenta(PlantaTipo type, PlantaCalidad quality)
    {
        object plantData = FindPlantData(type);
        if (plantData == null) return 1;

        // Campos típicos
        if (quality == PlantaCalidad.Plata)
            return GetIntMember(plantData, "precioVentaPlata") ?? GetIntMember(plantData, "PrecioVentaPlata") ?? 1;

        if (quality == PlantaCalidad.Oro)
            return GetIntMember(plantData, "precioVentaOro") ?? GetIntMember(plantData, "PrecioVentaOro") ?? 1;

        // default Estandar
        return GetIntMember(plantData, "precioVentaEstandar") ?? GetIntMember(plantData, "PrecioVentaEstandar") ?? 1;
    }

    private int GetPlantPriceCompra(PlantaTipo type, PlantaCalidad quality = PlantaCalidad.Estandar)
    {
        object plantData = FindPlantData(type);
        if (plantData == null) return 1;

        if (quality == PlantaCalidad.Plata)
            return GetIntMember(plantData, "precioCompraPlata") ?? GetIntMember(plantData, "PrecioCompraPlata") ?? 1;

        if (quality == PlantaCalidad.Oro)
            return GetIntMember(plantData, "precioCompraOro") ?? GetIntMember(plantData, "PrecioCompraOro") ?? 1;

        return GetIntMember(plantData, "precioCompraEstandar") ?? GetIntMember(plantData, "PrecioCompraEstandar") ?? 1;
    }

    private int GetSueroPrecioVenta(string nombre)
    {
        int price = 1;
        if (sueroBD == null) return price;

        object listObj =
            GetMemberValue(sueroBD, "sueros") ??
            GetMemberValue(sueroBD, "Sueros");

        if (listObj is IEnumerable enumerable)
        {
            foreach (var elem in enumerable)
            {
                if (elem == null) continue;

                string n = GetStringMember(elem, "nombre") ?? GetStringMember(elem, "Nombre");
                if (!string.Equals(n, nombre, StringComparison.OrdinalIgnoreCase)) continue;

                // En suero suele venir precioVentaEstandar (o similar)
                int p =
                    GetIntMember(elem, "precioVentaEstandar") ??
                    GetIntMember(elem, "PrecioVentaEstandar") ??
                    GetIntMember(elem, "precioVenta") ??
                    GetIntMember(elem, "PrecioVenta") ??
                    1;

                price = p;
                break;
            }
        }

        return price;
    }

    private object FindPlantData(PlantaTipo type)
    {
        if (plantBD == null) return null;

        object listObj =
            GetMemberValue(plantBD, "plantas") ??
            GetMemberValue(plantBD, "Plantas");

        if (!(listObj is IEnumerable enumerable)) return null;

        foreach (var elem in enumerable)
        {
            if (elem == null) continue;

            // 1) match por enum (plantaTipo / tipo)
            object tipoObj =
                GetMemberValue(elem, "plantaTipo") ??
                GetMemberValue(elem, "PlantaTipo") ??
                GetMemberValue(elem, "tipo") ??
                GetMemberValue(elem, "Tipo");

            if (tipoObj != null && tipoObj.GetType().IsEnum)
            {
                if (tipoObj.Equals(type)) return elem;
            }

            // 2) match por nombre string (nombre / Nombre)
            string n = GetStringMember(elem, "nombre") ?? GetStringMember(elem, "Nombre");
            if (!string.IsNullOrWhiteSpace(n) &&
                string.Equals(n, type.ToString(), StringComparison.OrdinalIgnoreCase))
                return elem;
        }

        return null;
    }

    // -------------------------
    // Helpers reflection (safe)
    // -------------------------
    private object GetMemberValue(object obj, string member)
    {
        if (obj == null) return null;

        var t = obj.GetType();
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var f = t.GetField(member, flags);
        if (f != null) return f.GetValue(obj);

        var p = t.GetProperty(member, flags);
        if (p != null) return p.GetValue(obj);

        return null;
    }

    private string GetStringMember(object obj, string member)
    {
        var t = obj.GetType();
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var f = t.GetField(member, flags);
        if (f != null && f.FieldType == typeof(string)) return (string)f.GetValue(obj);

        var p = t.GetProperty(member, flags);
        if (p != null && p.PropertyType == typeof(string)) return (string)p.GetValue(obj);

        return null;
    }

    private int? GetIntMember(object obj, string member)
    {
        var t = obj.GetType();
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var f = t.GetField(member, flags);
        if (f != null && f.FieldType == typeof(int)) return (int)f.GetValue(obj);

        var p = t.GetProperty(member, flags);
        if (p != null && p.PropertyType == typeof(int)) return (int)p.GetValue(obj);

        return null;
    }

    // -------------------------
    // Merge data (para el singleton scene-preferred)
    // -------------------------
    private void AbsorbFrom(InventorySystem other)
    {
        if (other == null) return;

        // Preferimos conservar lo que ya existía (state),
        // pero tomamos referencias BD si aquí vienen null.
        if (plantBD == null && other.plantBD != null) plantBD = other.plantBD;
        if (sueroBD == null && other.sueroBD != null) sueroBD = other.sueroBD;

        // Merge inventario: nos quedamos con el "máximo" por seguridad (evita duplicaciones por cargas)
        // PLANTAS (tipo+calidad)
        foreach (var p in other.plantas)
        {
            if (p == null) continue;
            var mine = plantas.Find(x => x.plantaTipo == p.plantaTipo && x.calidad == p.calidad);
            if (mine == null) plantas.Add(new InventoryItem { plantaTipo = p.plantaTipo, calidad = p.calidad, cantidad = p.cantidad });
            else mine.cantidad = Mathf.Max(mine.cantidad, p.cantidad);
        }

        // SEMILLAS
        foreach (var s in other.semillas)
        {
            if (s == null) continue;
            var mine = semillas.Find(x => x.plantaTipo == s.plantaTipo);
            if (mine == null) semillas.Add(new SeedItem { plantaTipo = s.plantaTipo, cantidad = s.cantidad });
            else
            {
                if (mine.cantidad == -1 || s.cantidad == -1) mine.cantidad = -1;
                else mine.cantidad = Mathf.Max(mine.cantidad, s.cantidad);
            }
        }

        // SUEROS
        foreach (var su in other.sueros)
        {
            if (su == null || string.IsNullOrWhiteSpace(su.sueroNombre)) continue;
            var mine = sueros.Find(x => string.Equals(x.sueroNombre, su.sueroNombre, StringComparison.OrdinalIgnoreCase));
            if (mine == null) sueros.Add(new SerumItem { sueroNombre = su.sueroNombre, cantidad = su.cantidad });
            else mine.cantidad = Mathf.Max(mine.cantidad, su.cantidad);
        }

        coins = Mathf.Max(coins, other.coins);
    }

    [ContextMenu("Limpiar Todo el Inventario")]
    public void ClearInventory()
    {
        plantas.Clear();
        semillas.Clear();
        sueros.Clear();
        coins = 0;
        RaiseChanged();
    }
}
