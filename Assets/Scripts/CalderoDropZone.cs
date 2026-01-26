using UnityEngine;
using UnityEngine.EventSystems;

public class CalderoDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null) return;

        var drag = eventData.pointerDrag.GetComponent<DraggableInventorySlot>();
        if (drag == null) drag = eventData.pointerDrag.GetComponentInParent<DraggableInventorySlot>();
        if (drag == null) return;

        if (CalderoLogic.instancia == null) return;

        ItemSO itemSO = drag.BoundItemSO;   
        if (itemSO == null)
        {
            CalderoLogic.instancia.ForceIncorrectDrop();
            return;
        }

        CalderoLogic.instancia.AddIngredient(itemSO);
    }
}
