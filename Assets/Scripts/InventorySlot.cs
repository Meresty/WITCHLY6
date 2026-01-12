using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image Icono;
    public TextMeshProUGUI Cantidad;
    public Button clickButton;

    private ItemSO currentItem;
    private int currentAmount;

    private System.Action<ItemSO> onClickCallback = null;

    public void Setup(ItemSO item, int amount, System.Action<ItemSO> callback = null)
    {
        currentItem = item;
        currentAmount = amount;
        onClickCallback = callback;

        Icono.sprite = item.icon;
        Cantidad.text = amount == -1 ? "∞" : amount.ToString();
        if (callback != null)
        {
            clickButton.onClick.RemoveAllListeners();
            clickButton.onClick.AddListener(() => onClickCallback?.Invoke(currentItem));
        }
    }

    public void ShowDataInDetails()
    {
        Debug.Log($"[InventorySlot] Mostrando detalles para: {currentItem.itemNombre}");
        InventoryUI.Instance.DisplayDetails(currentItem);
    }
}
