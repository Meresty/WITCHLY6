using UnityEngine;
using UnityEngine.EventSystems;

public class CalderoDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private bool logDrops = true;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData?.pointerDrag == null) return;
        if (CalderoLogic.instancia == null) return;

        var drag = eventData.pointerDrag.GetComponent<DraggableInventorySlot>()
               ?? eventData.pointerDrag.GetComponentInParent<DraggableInventorySlot>();

        if (drag == null) return;

        ItemSO itemSO = drag.BoundItemSO;

        if (logDrops)
        {
            var info = drag.BoundItemInfo;
            Debug.Log($"[DROPZONE] drop={(itemSO ? itemSO.name : "NULL")} info={(info != null ? info.itemNombre : "NULL")} tipo={(info != null ? info.plantaTipo.ToString() : "NULL")} calidad={(info != null ? info.calidad.ToString() : "NULL")}");
        }

        // SI ES NULL: NO castigues, solo ignora (o muestra mensaje)
        if (itemSO == null)
        {
            Debug.LogWarning("[DROPZONE] BoundItemSO es NULL -> falta mapping en InventorySystem o boundInfo incompleto.");
            return;
        }

        // Canonicalize opcional si lo tienes
        if (InventorySystem.Instance != null)
            itemSO = InventorySystem.Instance.Canonicalize(itemSO);

        CalderoLogic.instancia.AddIngredient(itemSO);
    }
}
