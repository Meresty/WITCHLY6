using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public Transform content;

    void Start()
    {
        RefreshInventory();
    }

    void OnEnable()
    {
        RefreshInventory();
    }

    public void RefreshInventory()
    {

        if (content == null)
        {
            Debug.LogError("[InventoryUI] ¡El campo 'Content' no está asignado en el Inspector!");
            return;
        }


        foreach (Transform child in content)
            Destroy(child.gameObject);


        if (InventoryManager.instancia == null)
        {
            Debug.LogError("[InventoryUI] ¡No hay InventoryManager en la escena!");
            return;
        }

        foreach (var inventoryItem in InventoryManager.instancia.items)
        {
            if (inventoryItem == null || inventoryItem.item == null)
                continue;

            GameObject slot = Instantiate(itemSlotPrefab, content);


            Transform iconoTransform = slot.transform.Find("Icono");
            Transform cantidadTransform = slot.transform.Find("Cantidad");
            Transform buttonTransform = slot.transform.Find("Button");

            if (iconoTransform == null || cantidadTransform == null || buttonTransform == null)
                continue;


            Image iconImage = iconoTransform.GetComponent<Image>();
            if (iconImage != null && inventoryItem.item.icon != null)
            {
                iconImage.sprite = inventoryItem.item.icon;
            }


            TextMeshProUGUI cantidadText = cantidadTransform.GetComponent<TextMeshProUGUI>();
            if (cantidadText != null)
            {
                cantidadText.text = inventoryItem.cantidad.ToString();
            }


            Button boton = buttonTransform.GetComponent<Button>();
            if (boton != null)
            {
                ItemSO itemRef = inventoryItem.item;
                boton.onClick.AddListener(() => UsarItemEnCaldero(itemRef));
            }
        }
    }


    void UsarItemEnCaldero(ItemSO item)
    {
        if (CalderoLogic.instancia != null)
        {
            CalderoLogic.instancia.AddIngredient(item);
            Debug.Log($"[InventoryUI] Item '{item.itemNombre}' enviado a CalderoLogic");
        }
        else
        {
            Debug.LogWarning("[InventoryUI] CalderoLogic no está activo en esta escena!");
        }
    }

    public void AddItem(ItemSO item, int cantidad)
    {
        if (InventoryManager.instancia != null)
        {
            InventoryManager.instancia.AddItem(item, cantidad);
            RefreshInventory();
        }
    }
}