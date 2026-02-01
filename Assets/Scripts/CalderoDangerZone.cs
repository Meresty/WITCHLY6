using UnityEngine;
using UnityEngine.EventSystems;

public class CalderoDangerZone : MonoBehaviour, IDropHandler
{
    public ItemSO itemdrop;
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null)
        {
            Debug.LogWarning("[DANGERZONE] eventData is NULL.");
            return;
        }

        if (eventData.pointerDrag == null)
        {
            Debug.LogWarning("[DANGERZONE] pointerDrag is NULL.");
            return;
        }

        if (CalderoLogic.instancia == null)
        {
            Debug.LogWarning("[DANGERZONE] CalderoLogic.instancia is NULL.");
            return;
        }

        Debug.Log($"[DANGERZONE] pointerDrag: {eventData.pointerDrag.name}");

        var drag = eventData.pointerDrag.GetComponent<DraggableInventorySlot>()
                   ?? eventData.pointerDrag.GetComponentInParent<DraggableInventorySlot>();

        if (drag == null)
        {
            Debug.LogWarning("[DANGERZONE] No DraggableInventorySlot found on pointerDrag or its parents.");
            return;
        }

        Debug.Log($"[DANGERZONE] DraggableInventorySlot found on: {drag.gameObject.name}");

        CalderoLogic.instancia.AddIngredient(drag.BoundItemSO);
    }

}