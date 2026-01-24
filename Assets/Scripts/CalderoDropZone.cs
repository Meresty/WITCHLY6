using UnityEngine;
using UnityEngine.EventSystems;

public class CalderoDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null) return;

        var drag = eventData.pointerDrag.GetComponent<DraggableInventorySlot>();
        if (drag == null) return;

        // Permite dropear cualquier cosa:
        // Si no hay ItemSO (ej. semilla), llegara null y CalderoLogic lo tratara como incorrecto => Presicion
        if (CalderoLogic.instancia != null)
        {
            CalderoLogic.instancia.AddIngredient(drag.BoundItemSO);
        }
    }
}
