using System.Collections.Generic;
using UnityEngine;

public class CalderoInventoryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform content;

    [Header("Drag")]
    [SerializeField] private Transform dragRoot; // tu Canvas

    [Header("Opcional")]
    [SerializeField] private bool ocultarSiCantidadCero = true;

    private void OnEnable()
    {
        if (InventoryManager.instancia != null)
            InventoryManager.instancia.OnInventoryChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (InventoryManager.instancia != null)
            InventoryManager.instancia.OnInventoryChanged -= Refresh;
    }

    public void Refresh()
    {
        if (itemSlotPrefab == null || content == null)
        {
            Debug.LogError("[CalderoInventoryUI] Falta itemSlotPrefab o content.");
            return;
        }

        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        if (InventoryManager.instancia == null) return;

        List<InventoryManager.InventoryEntry> entries = InventoryManager.instancia.GetEntriesSnapshot();
        foreach (var entry in entries)
        {
            if (entry == null || entry.item == null) continue;
            if (ocultarSiCantidadCero && entry.cantidad <= 0) continue;

            GameObject go = Instantiate(itemSlotPrefab, content);

            // Inicializa el drag
            var drag = go.GetComponent<CalderoDraggableItem>();
            if (drag == null) drag = go.AddComponent<CalderoDraggableItem>();
            drag.Init(entry.item, dragRoot);

            // Tu InventorySlot
            var slot = go.GetComponent<InventorySlot>();
            if (slot == null) continue;

            ItemInfo info = new ItemInfo();
            info.itemNombre = entry.item.itemNombre;
            info.itemDescripcion = entry.item.itemDescripcion ?? "";
            info.icon = entry.item.icon;

            slot.Setup(info, entry.cantidad, (selected) => { });
        }
    }
}
