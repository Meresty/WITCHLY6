using UnityEngine;
using UnityEngine.EventSystems;

public class CalderoDangerZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null) return;

        var drag = eventData.pointerDrag.GetComponent<DraggableInventorySlot>();
        if (drag == null) return;

        // BoundItem existe por compatibilidad (alias de BoundItemSO)
        ItemSO item = drag.BoundItem;

        if (CalderoLogic.instancia == null) return;

        // Si no hay mapping (ej semilla) cuenta como incorrecto => Presicion
        if (item == null)
        {
            CalderoLogic.instancia.ForceIncorrectDrop();
            return;
        }

        CalderoLogic.instancia.AddIngredient(item);
    }
}
