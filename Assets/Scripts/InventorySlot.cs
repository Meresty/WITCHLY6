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

        Icono.sprite = item.icon;
        Icono.transform.localScale = new Vector3(item.widthModifier, item.heightModifier, 1f);
        Cantidad.text = amount == -1 ? "∞" : amount.ToString();
        if (callback != null)
        {
            clickButton.onClick.RemoveAllListeners();
            clickButton.onClick.AddListener(() => onClickCallback?.Invoke(currentItem));
        }

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

    public void ShowDataInDetails()
    {
        Debug.Log($"[InventorySlot] Mostrando detalles para: {currentItem.itemNombre}");
        InventoryUI.Instance.DisplayDetails(currentItem);
    }
}
