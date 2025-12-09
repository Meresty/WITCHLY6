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

    private System.Action<ItemSO> onClickCallback;

    public void Setup(ItemSO item, int amount, System.Action<ItemSO> callback)
    {
        currentItem = item;
        currentAmount = amount;
        onClickCallback = callback;

        Icono.sprite = item.icon;
        Cantidad.text = amount.ToString();

        clickButton.onClick.RemoveAllListeners();
        clickButton.onClick.AddListener(() => onClickCallback?.Invoke(currentItem));
    }
}
