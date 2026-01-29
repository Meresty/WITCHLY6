using UnityEngine;
using UnityEngine.EventSystems;

public class CalderoDangerZone : MonoBehaviour, IDropHandler
{
    public ItemSO itemdrop;
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null) return;
        if (CalderoLogic.instancia == null) return;

        var drag = eventData.pointerDrag.GetComponent<DraggableInventorySlot>()
                   ?? eventData.pointerDrag.GetComponentInParent<DraggableInventorySlot>();

        if (drag == null) return;
        ItemSO itemSO = new ItemSO();
        
        
        
        itemSO = itemdrop;

        //ItemSO itemSO = drag.BoundItemSO;
        //Debug.Log(drag.BoundItemSO);
        CalderoLogic.instancia.AddIngredient(itemSO);
        /*
        if (itemSO != null)
        {

            
            Debug.Log($"[CALDERO] Drop detectado => {itemSO.name}");
            CalderoLogic.instancia.AddIngredient(itemSO);
        }
           
        else
        {
            Debug.LogWarning("[CALDERO] Drop en DangerZone pero BoundItemSO/BoundItem es NULL => mapping no resuelto.");
            //CalderoLogic.instancia.ForceIncorrectDrop();
            return;
        }
        */

       
    }
}
