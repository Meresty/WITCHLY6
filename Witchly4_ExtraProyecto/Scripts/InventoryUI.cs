using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [System.Serializable]
    public class InventoryItem
    {
        public ItemSO item;
        public int cantidad;
    }

    public GameObject itemSlotPrefab;
    public Transform content;
    public List<InventoryItem> items = new List<InventoryItem>();

    void Start()
    {
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        foreach (var inventoryItem in items)
        {
            // Validar el item
            if (inventoryItem == null || inventoryItem.item == null)
            {
                Debug.LogError("¡Item del inventario es NULL!");
                continue;
            }

            GameObject slot = Instantiate(itemSlotPrefab, content);

            // Buscar los hijos
            Transform iconoTransform = slot.transform.Find("Icono");
            Transform cantidadTransform = slot.transform.Find("Cantidad");
            Transform buttonTransform = slot.transform.Find("Button");

            // Validar que existan
            if (iconoTransform == null)
            {
                Debug.LogError("No se encontró el hijo 'Icono' en el prefab!");
                continue;
            }

            if (cantidadTransform == null)
            {
                Debug.LogError("No se encontró el hijo 'Cantidad' en el prefab!");
                continue;
            }

            if (buttonTransform == null)
            {
                Debug.LogError("No se encontró el hijo 'Button' en el prefab!");
                continue;
            }

            // Asignar valores
            Image iconImage = iconoTransform.GetComponent<Image>();
            if (iconImage != null && inventoryItem.item.icon != null)
            {
                iconImage.sprite = inventoryItem.item.icon;
            }
            else
            {
                Debug.LogError("El sprite del item es NULL o no hay componente Image");
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
                boton.onClick.AddListener(() => {
                    CalderoLogic.instancia.AddIngredient(itemRef);
                });
            }
        }
    }

    public void AddItem(ItemSO item, int cantidad)
    {
        var existente = items.Find(i => i.item == item);

        if (existente == null)
        {
            items.Add(new InventoryItem
            {
                item = item,
                cantidad = cantidad
            });
        }
        else
        {
            existente.cantidad += cantidad;
        }

        RefreshInventory();
    }


    public bool HasItem(ItemSO item)
    {
        var inv = items.Find(i => i.item == item);
        return inv != null && inv.cantidad > 0;
    }

    public void RemoveItem(ItemSO item)
    {
        var inv = items.Find(i => i.item == item);
        if (inv != null)
        {
            inv.cantidad--;
            if (inv.cantidad <= 0)
                items.Remove(inv);

            RefreshInventory();
        }
    }
}
