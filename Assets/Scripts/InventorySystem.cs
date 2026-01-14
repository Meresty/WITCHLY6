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
    public int cantidad;
}

/// <summary>
/// Sistema de inventario del invernadero
/// Maneja plantas, semillas, sueros y monedas
/// SINCRONIZA con InventoryManager del caldero
/// </summary>
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

    [Header("Conexión con Caldero")]
    [Tooltip("Asigna los ItemSO correspondientes a cada planta")]
    public PlantaItemSOMapping[] plantaToItemMapping;

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
        // Verificar si es la primera vez que se juega
        if (true || !PlayerPrefs.HasKey("FirstTime"))
        {
            Debug.Log("Primera vez jugando - Inicializando inventario inicial");

            // RQF58: Inventario inicial
            AddSerum("Suero de Fuerza", 1);
            AddSerum("Suero de Energía", 1);
            AddPlant(PlantaTipo.Drakonia, PlantaCalidad.Estandar, 5);
            AddPlant(PlantaTipo.Drakonia, PlantaCalidad.Plata, 5);
            AddPlant(PlantaTipo.Falsibaya, PlantaCalidad.Estandar, 5);
            AddPlant(PlantaTipo.Drakonia, PlantaCalidad.Oro, 5);
            AddSemilla(PlantaTipo.Falsibaya, -1);
            AddSemilla(PlantaTipo.Drakonia, -1);

            // RQF61: Drakonia y Falsibaya siempre disponibles (son perennes)
            // Ya están en el inventario inicial

            PlayerPrefs.SetInt("FirstTime", 1);
            PlayerPrefs.Save();
        }
        else
        {
            LoadInventory();
        }
    }

    #region SEMILLAS

    /// <summary>
    /// Añade semillas al inventario
    /// </summary>
    public void AddSemilla(PlantaTipo type, int amount)
    {
        var existing = semillas.Find(s => s.plantaTipo == type);
        if (existing != null)
        {
            existing.cantidad += amount;
        }
        else
        {
            semillas.Add(new SeedItem
            {
                plantaTipo = type,
                cantidad = amount
            });
        }
        OnInventoryChanged?.Invoke();
        SaveInventory();

        Debug.Log($"Añadidas {amount} semillas de {type}");
    }

    /// <summary>
    /// Verifica si tiene suficientes semillas
    /// </summary>
    public bool HasSeed(PlantaTipo type, int amount = 1)
    {
        var seed = semillas.Find(s => s.plantaTipo == type);
        return seed != null && seed.cantidad >= amount;
    }

    /// <summary>
    /// Remueve semillas del inventario
    /// </summary>
    public bool RemoveSeed(PlantaTipo type, int amount = 1)
    {
        var seed = semillas.Find(s => s.plantaTipo == type);
        if (seed != null && (seed.cantidad >= amount || amount == -1))
        {
            seed.cantidad -= amount;
            if (seed.cantidad == 0)
            {
                semillas.Remove(seed);
            }
            if (amount == -1)
                seed.cantidad = -1;

            OnInventoryChanged?.Invoke();
            SaveInventory();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Obtiene la cantidad de semillas de un tipo
    /// </summary>
    public int GetSeedCount(PlantaTipo type)
    {
        var seed = semillas.Find(s => s.plantaTipo == type);
        return seed != null ? seed.cantidad : 0;
    }

    #endregion

    #region PLANTAS

    /// <summary>
    /// Añade plantas al inventario Y LAS SINCRONIZA CON EL CALDERO
    /// </summary>
    public void AddPlant(PlantaTipo type, PlantaCalidad quality, int amount)
    {
        var existing = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        if (existing != null)
        {
            existing.cantidad += amount;
        }
        else
        {
            plantas.Add(new InventoryItem
            {
                plantaTipo = type,
                calidad = quality,
                cantidad = amount
            });
        }

        // ⭐ SINCRONIZAR CON CALDERO
        SyncPlantToCaldero(type, quality, amount);

        OnInventoryChanged?.Invoke();
        SaveInventory();

        Debug.Log($"Añadidas {amount} plantas {type} de calidad {quality}");
    }

    /// <summary>
    /// Verifica si tiene suficientes plantas
    /// </summary>
    public bool HasPlant(PlantaTipo type, PlantaCalidad quality, int amount = 1)
    {
        var plant = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        return plant != null && plant.cantidad >= amount;
    }

    /// <summary>
    /// Remueve plantas del inventario
    /// </summary>
    public bool RemovePlant(PlantaTipo type, PlantaCalidad quality, int amount = 1)
    {
        var plant = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        if (plant != null && plant.cantidad >= amount)
        {
            plant.cantidad -= amount;
            if (plant.cantidad == 0)
            {
                plantas.Remove(plant);
            }
            OnInventoryChanged?.Invoke();
            SaveInventory();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Obtiene la cantidad de plantas de un tipo y calidad
    /// </summary>
    public int GetPlantCount(PlantaTipo type, PlantaCalidad quality)
    {
        var plant = plantas.Find(p => p.plantaTipo == type && p.calidad == quality);
        return plant != null ? plant.cantidad : 0;
    }

    #endregion

    #region SUEROS

    /// <summary>
    /// Añade sueros al inventario
    /// </summary>
    public void AddSerum(string serumName, int amount)
    {
        var existing = sueros.Find(s => s.sueroNombre == serumName);
        if (existing != null)
        {
            existing.cantidad += amount;
        }
        else
        {
            sueros.Add(new SerumItem { sueroNombre = serumName, cantidad = amount });
        }
        OnInventoryChanged?.Invoke();
        SaveInventory();

        Debug.Log($"Añadidos {amount} {serumName}");
    }

    /// <summary>
    /// Verifica si tiene suficientes sueros
    /// </summary>
    public bool HasSerum(string serumName, int amount = 1)
    {
        var serum = sueros.Find(s => s.sueroNombre == serumName);
        return serum != null && serum.cantidad >= amount;
    }

    /// <summary>
    /// Remueve sueros del inventario
    /// </summary>
    public bool RemoveSerum(string serumName, int amount = 1)
    {
        var serum = sueros.Find(s => s.sueroNombre == serumName);
        if (serum != null && serum.cantidad >= amount)
        {
            serum.cantidad -= amount;
            if (serum.cantidad == 0)
            {
                sueros.Remove(serum);
            }
            OnInventoryChanged?.Invoke();
            SaveInventory();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Obtiene la cantidad de un suero
    /// </summary>
    public int GetSerumCount(string serumName)
    {
        var serum = sueros.Find(s => s.sueroNombre == serumName);
        return serum != null ? serum.cantidad : 0;
    }

    #endregion

    #region MONEDAS

    /// <summary>
    /// Añade monedas al inventario
    /// </summary>
    public void AddCoins(int amount)
    {
        coins += amount;
        OnInventoryChanged?.Invoke();
        SaveInventory();

        Debug.Log($"Añadidas {amount} monedas. Total: {coins}");
    }

    /// <summary>
    /// Gasta monedas del inventario
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            OnInventoryChanged?.Invoke();
            SaveInventory();

            Debug.Log($"Gastadas {amount} monedas. Restante: {coins}");
            return true;
        }

        Debug.LogWarning($"Monedas insuficientes. Necesitas {amount}, tienes {coins}");
        return false;
    }

    #endregion

    #region VENTA DE ITEMS

    /// <summary>
    /// RQF59: Vende plantas por monedas según la tabla de precios
    /// </summary>
    public void SellPlant(PlantaTipo type, PlantaCalidad quality, int amount)
    {
        if (RemovePlant(type, quality, amount))
        {
            PlantData data = plantBD.GetPlantas(type);
            if (data != null)
            {
                int price = quality == PlantaCalidad.Estandar ? data.precioVentaEstandar :
                           quality == PlantaCalidad.Plata ? data.precioVentaPlata :
                           data.precioVentaOro;

                AddCoins(price * amount);

                Debug.Log($"Vendidas {amount} {type} ({quality}) por {price * amount} monedas");
            }
        }
    }

    /// <summary>
    /// Vende semillas por monedas
    /// </summary>
    public void SellSeed(PlantaTipo type, int amount)
    {
        if (RemoveSeed(type, amount))
        {
            PlantData data = plantBD.GetPlantas(type);
            if (data != null)
            {
                AddCoins(data.precioCompraEstandar * amount);

                Debug.Log($"Vendidas {amount} semillas de {type} por {data.precioCompraEstandar * amount} monedas");
            }
        }
    }

    /// <summary>
    /// Compra plantas del mercado (RQF52)
    /// </summary>
    public bool BuyPlant(PlantaTipo type, PlantaCalidad quality, int amount = 1)
    {
        PlantData data = plantBD.GetPlantas(type);
        if (data == null) return false;

        int price = quality == PlantaCalidad.Estandar ? data.precioCompraEstandar :
                   quality == PlantaCalidad.Plata ? data.precioCompraPlata :
                   data.precioCompraOro;

        int totalCost = price * amount;

        if (SpendCoins(totalCost))
        {
            AddPlant(type, quality, amount);
            Debug.Log($"Compradas {amount} {type} ({quality}) por {totalCost} monedas");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Compra semillas del mercado
    /// </summary>
    public bool BuySeed(PlantaTipo type, int amount = 1)
    {
        PlantData data = plantBD.GetPlantas(type);
        if (data == null) return false;

        int totalCost = data.precioCompraEstandar * amount;

        if (SpendCoins(totalCost))
        {
            AddSemilla(type, amount);
            Debug.Log($"Compradas {amount} semillas de {type} por {totalCost} monedas");
            return true;
        }

        return false;
    }

    #endregion

    #region SINCRONIZACIÓN CON CALDERO

    /// <summary>
    /// Sincroniza las plantas del invernadero con el inventario del caldero
    /// </summary>
    private void SyncPlantToCaldero(PlantaTipo tipo, PlantaCalidad calidad, int cantidad)
    {
        if (InventoryManager.instancia == null)
        {
            Debug.LogWarning("[InventorySystem] InventoryManager no está disponible aún");
            return;
        }

        ItemSO itemSO = GetItemSOForPlant(tipo, calidad);
        if (itemSO != null)
        {
            InventoryManager.instancia.AddItem(itemSO, cantidad);
            Debug.Log($"✅ Sincronizado: {cantidad}x {tipo} ({calidad}) → Caldero");
        }
        else
        {
            Debug.LogWarning($"⚠️ No hay ItemSO mapeado para {tipo} ({calidad})");
        }
    }

    /// <summary>
    /// Obtiene el ItemSO correspondiente a una planta
    /// </summary>
    private ItemSO GetItemSOForPlant(PlantaTipo tipo, PlantaCalidad calidad)
    {
        foreach (var mapping in plantaToItemMapping)
        {
            if (mapping.tipo == tipo && mapping.calidad == calidad)
            {
                return mapping.itemSO;
            }
        }
        return null;
    }

    #endregion

    #region GUARDADO/CARGA

    void SaveInventory()
    {
        // Guardar plantas
        PlayerPrefs.SetString("Plants", JsonUtility.ToJson(new PlantsList { items = plantas }));

        // Guardar semillas
        PlayerPrefs.SetString("Seeds", JsonUtility.ToJson(new SeedsList { items = semillas }));

        // Guardar sueros
        PlayerPrefs.SetString("Serums", JsonUtility.ToJson(new SerumsList { items = sueros }));

        // Guardar monedas
        PlayerPrefs.SetInt("Coins", coins);

        PlayerPrefs.Save();
    }

    void LoadInventory()
    {
        // Cargar plantas
        if (PlayerPrefs.HasKey("Plants"))
        {
            string json = PlayerPrefs.GetString("Plants");
            PlantsList data = JsonUtility.FromJson<PlantsList>(json);
            if (data != null && data.items != null)
                plantas = data.items;
        }

        // Cargar semillas
        if (PlayerPrefs.HasKey("Seeds"))
        {
            string json = PlayerPrefs.GetString("Seeds");
            SeedsList data = JsonUtility.FromJson<SeedsList>(json);
            if (data != null && data.items != null)
                semillas = data.items;
        }

        // Cargar sueros
        if (PlayerPrefs.HasKey("Serums"))
        {
            string json = PlayerPrefs.GetString("Serums");
            SerumsList data = JsonUtility.FromJson<SerumsList>(json);
            if (data != null && data.items != null)
                sueros = data.items;
        }

        // Cargar monedas
        coins = PlayerPrefs.GetInt("Coins", 0);

        Debug.Log($"Inventario cargado: {plantas.Count} tipos de plantas, {semillas.Count} tipos de semillas, {sueros.Count} sueros, {coins} monedas");
    }

    #endregion

    #region MÉTODOS DE UTILIDAD

    /// <summary>
    /// Limpia todo el inventario (para testing)
    /// </summary>
    [ContextMenu("Limpiar Todo el Inventario")]
    public void ClearInventory()
    {
        plantas.Clear();
        semillas.Clear();
        sueros.Clear();
        coins = 0;
        OnInventoryChanged?.Invoke();
        SaveInventory();

        Debug.Log("Inventario limpiado");
    }

    /// <summary>
    /// Imprime el inventario completo en la consola
    /// </summary>
    [ContextMenu("Imprimir Inventario")]
    public void PrintInventory()
    {
        Debug.Log("=== INVENTARIO COMPLETO ===");

        Debug.Log($"\nMONEDAS: {coins}");

        Debug.Log("\nPLANTAS:");
        foreach (var plant in plantas)
        {
            Debug.Log($"  - {plant.plantaTipo} ({plant.calidad}): {plant.cantidad}");
        }

        Debug.Log("\nSEMILLAS:");
        foreach (var seed in semillas)
        {
            Debug.Log($"  - {seed.plantaTipo}: {seed.cantidad}");
        }

        Debug.Log("\nSUEROS:");
        foreach (var serum in sueros)
        {
            Debug.Log($"  - {serum.sueroNombre}: {serum.cantidad}");
        }
    }

    #endregion
}

// ⭐ NUEVA CLASE: Mapeo entre plantas del invernadero y ItemSO del caldero
[System.Serializable]
public class PlantaItemSOMapping
{
    public PlantaTipo tipo;
    public PlantaCalidad calidad;
    public ItemSO itemSO;
}

// Clases auxiliares para serialización
[System.Serializable]
public class PlantsList
{
    public List<InventoryItem> items;
}

[System.Serializable]
public class SeedsList
{
    public List<SeedItem> items;
}

[System.Serializable]
public class SerumsList
{
    public List<SerumItem> items;
}