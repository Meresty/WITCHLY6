using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image Icono;
    public Image bg;
    public TextMeshProUGUI Cantidad;
    public Button clickButton;

    private ItemInfo currentItem;
    private int currentAmount;
    private System.Action<ItemInfo> onClickCallback = null;

    public void Setup(ItemInfo item, int amount, System.Action<ItemInfo> callback = null)
    {
        currentItem = item;
        currentAmount = amount;
        onClickCallback = callback;

        if (Icono != null)
        {
            Icono.sprite = item.icon;
            Icono.transform.localScale = new Vector3(item.widthModifier, item.heightModifier, 1f);
        }

        if (Cantidad != null)
            Cantidad.text = amount == -1 ? "∞" : amount.ToString();

        if (clickButton != null)
        {
            clickButton.onClick.RemoveAllListeners();
            if (callback != null)
                clickButton.onClick.AddListener(() => onClickCallback?.Invoke(currentItem));
        }

        if (bg != null)
        {
            switch (item.calidad)
            {
                case PlantaCalidad.Estandar:
                    bg.color = Color.white - new Color(0f, 0f, 0f, 0.3f);
                    break;
                case PlantaCalidad.Plata:
                    bg.color = Color.cyan - new Color(0f, 0f, 0f, 0.6f);
                    break;
                case PlantaCalidad.Oro:
                    bg.color = Color.yellow - new Color(0f, 0f, 0f, 0.3f);
                    break;
                default:
                    bg.color = Color.white - new Color(0f, 0f, 0f, 0.3f);
                    break;
            }
        }

        // Bind para drag&drop
        var drag = GetComponent<DraggableInventorySlot>();
        if (drag != null)
        {
            drag.Bind(item, amount);
        }
    }

    public void ShowDataInDetails()
    {
        if (currentItem == null) return;
        Debug.Log($"[InventorySlot] Mostrando detalles para: {currentItem.itemNombre}");
        if (InventoryUI.Instance != null)
            InventoryUI.Instance.DisplayDetails(currentItem);
    }
}
