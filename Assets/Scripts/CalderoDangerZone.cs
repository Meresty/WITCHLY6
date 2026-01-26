using UnityEngine;
using UnityEngine.EventSystems;

public class CalderoDangerZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null) return;

        var drag = eventData.pointerDrag.GetComponent<DraggableInventorySlot>();
        if (drag == null) drag = eventData.pointerDrag.GetComponentInParent<DraggableInventorySlot>();
        if (drag == null) return;

        if (CalderoLogic.instancia == null) return;

        // BoundItem/BoundItemSO ya existen por compat
        ItemSO itemSO = drag.BoundItem;

        // Si no hay mapping (ej: semilla u otro), cuenta como incorrecto
        if (itemSO == null)
        {
            CalderoLogic.instancia.ForceIncorrectDrop();
            return;
        }

        CalderoLogic.instancia.AddIngredient(itemSO);
    }
}
