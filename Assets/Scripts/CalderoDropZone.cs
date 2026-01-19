using UnityEngine;
using UnityEngine.EventSystems;

public class CalderoDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null) return;

        var drag = eventData.pointerDrag.GetComponent<CalderoDraggableItem>();
        if (drag == null) return;

        ItemSO item = drag.Item;
        if (item == null) return;

        if (InventoryManager.instancia == null)
        {
            Debug.LogWarning("[CalderoDropZone] InventoryManager.instancia es null.");
            return;
        }

        // 1) consume 1 del inventario
        bool ok = InventoryManager.instancia.RemoveItem(item, 1);
        if (!ok)
        {
            Debug.LogWarning($"[CalderoDropZone] No tienes {item.itemNombre} para consumir.");
            return;
        }

        // 2) manda al caldero
        if (CalderoLogic.instancia != null)
        {
            CalderoLogic.instancia.AddIngredient(item);
        }
        else
        {
            Debug.LogWarning("[CalderoDropZone] CalderoLogic.instancia es null. Reembolso el item.");
            InventoryManager.instancia.AddItem(item, 1);
            return;
        }

        // 3) marca consumido para que EndDrag lo destruya sin crashear
        drag.MarkConsumed();
    }
}
