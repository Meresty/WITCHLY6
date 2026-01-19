using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instancia;

    public event Action OnInventoryChanged;

    [Serializable]
    public class InventoryEntry
    {
        public ItemSO item;
        public int cantidad;
    }

    [Header("Debug - Inventario actual")]
    [SerializeField] private List<InventoryEntry> items = new List<InventoryEntry>();

    private readonly Dictionary<ItemSO, InventoryEntry> index = new Dictionary<ItemSO, InventoryEntry>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoBoot()
    {
        if (instancia != null) return;

        var existing = FindObjectOfType<InventoryManager>();
        if (existing != null)
        {
            instancia = existing;
            return;
        }

        var go = new GameObject("InventoryManager");
        go.AddComponent<InventoryManager>();
    }

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            instancia.AbsorbFrom(this);
            Destroy(this);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);
        RebuildIndex();
    }

    private void RaiseChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    private void RebuildIndex()
    {
        index.Clear();

        for (int i = items.Count - 1; i >= 0; i--)
        {
            var e = items[i];
            if (e == null || e.item == null)
            {
                items.RemoveAt(i);
                continue;
            }

            if (index.TryGetValue(e.item, out var existing) && existing != null)
            {
                existing.cantidad += e.cantidad;
                items.RemoveAt(i);
                continue;
            }

            index[e.item] = e;
        }
    }

    private InventoryEntry GetOrCreate(ItemSO item)
    {
        if (item == null) return null;

        if (!index.TryGetValue(item, out var entry) || entry == null)
        {
            entry = new InventoryEntry { item = item, cantidad = 0 };
            items.Add(entry);
            index[item] = entry;
        }

        return entry;
    }

    private void RemoveEntry(ItemSO item)
    {
        if (item == null) return;
        if (!index.TryGetValue(item, out var entry) || entry == null) return;

        items.Remove(entry);
        index.Remove(item);
    }

    public void AddItem(ItemSO item, int cantidad = 1)
    {
        if (item == null) return;
        if (cantidad == 0) return;

        var entry = GetOrCreate(item);
        entry.cantidad += cantidad;

        if (entry.cantidad <= 0)
            RemoveEntry(item);

        RaiseChanged();
    }

    public void SetItemCount(ItemSO item, int nuevoConteo)
    {
        if (item == null) return;

        if (nuevoConteo <= 0)
        {
            RemoveEntry(item);
            RaiseChanged();
            return;
        }

        var entry = GetOrCreate(item);
        entry.cantidad = nuevoConteo;

        RaiseChanged();
    }

    public bool HasItem(ItemSO item, int cantidadRequerida = 1)
    {
        if (item == null) return false;
        if (cantidadRequerida <= 0) return true;

        if (!index.TryGetValue(item, out var entry) || entry == null) return false;
        return entry.cantidad >= cantidadRequerida;
    }

    public int GetItemCount(ItemSO item)
    {
        if (item == null) return 0;
        if (!index.TryGetValue(item, out var entry) || entry == null) return 0;
        return entry.cantidad;
    }

    public bool RemoveItem(ItemSO item, int cantidad = 1)
    {
        if (item == null) return false;
        if (cantidad <= 0) return true;

        if (!index.TryGetValue(item, out var entry) || entry == null) return false;
        if (entry.cantidad < cantidad) return false;

        entry.cantidad -= cantidad;

        if (entry.cantidad <= 0)
            RemoveEntry(item);

        RaiseChanged();
        return true;
    }

    public bool TryConsumeItems(List<ItemSO> receta)
    {
        if (receta == null || receta.Count == 0) return true;

        Dictionary<ItemSO, int> needed = new Dictionary<ItemSO, int>();

        for (int i = 0; i < receta.Count; i++)
        {
            var it = receta[i];
            if (it == null) continue;

            needed.TryGetValue(it, out int c);
            needed[it] = c + 1;
        }

        foreach (var kv in needed)
            if (!HasItem(kv.Key, kv.Value)) return false;

        foreach (var kv in needed)
            RemoveItem(kv.Key, kv.Value);

        return true;
    }

    public List<InventoryEntry> GetEntriesSnapshot()
    {
        return new List<InventoryEntry>(items);
    }

    private void AbsorbFrom(InventoryManager other)
    {
        if (other == null) return;

        other.RebuildIndex();

        foreach (var e in other.items)
        {
            if (e == null || e.item == null) continue;

            var mine = GetOrCreate(e.item);
            mine.cantidad = Mathf.Max(mine.cantidad, e.cantidad);
        }

        RaiseChanged();
    }
}
