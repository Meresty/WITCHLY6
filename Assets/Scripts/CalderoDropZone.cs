using UnityEngine;
using UnityEngine.EventSystems;

public class CalderoDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private bool logDrops = false;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData?.pointerDrag == null) return;

        var drag = eventData.pointerDrag.GetComponent<DraggableInventorySlot>()
               ?? eventData.pointerDrag.GetComponentInParent<DraggableInventorySlot>();

        if (drag == null || CalderoLogic.instancia == null) return;

        ItemSO itemSO = drag.BoundItemSO; 

        if (logDrops)
            Debug.Log($"[DROPZONE] Soltaste: {(itemSO ? itemSO.name : "NULL")}");

        if (itemSO == null)
        {
            //CalderoLogic.instancia.ForceIncorrectDrop();
            return;
        }

        //CalderoLogic.instancia.AddIngredient(itemSO);
    }
}
