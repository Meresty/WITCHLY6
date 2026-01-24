using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instancia;

    [System.Serializable]
    public class InventoryItem
    {
        public ItemSO item;
        public int cantidad;
    }

    public List<InventoryItem> items = new List<InventoryItem>();

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(ItemSO item, int cantidad = 1)
    {
        if (item == null) return;

        var existente = items.Find(i => i.item == item);
        if (existente == null)
        {
            items.Add(new InventoryItem { item = item, cantidad = cantidad });
        }
        else
        {
            existente.cantidad += cantidad;
        }
    }

    public bool HasItem(ItemSO item, int cantidadRequerida = 1)
    {
        if (item == null) return false;
        var inv = items.Find(i => i.item == item);
        return inv != null && inv.cantidad >= cantidadRequerida;
    }

    public int GetItemCount(ItemSO item)
    {
        if (item == null) return 0;
        var inv = items.Find(i => i.item == item);
        return inv != null ? inv.cantidad : 0;
    }

    public void RemoveItem(ItemSO item, int cantidad = 1)
    {
        if (item == null) return;

        var inv = items.Find(i => i.item == item);
        if (inv == null) return;

        inv.cantidad -= cantidad;
        if (inv.cantidad <= 0)
            items.Remove(inv);
    }
}
