
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventarioDetailsUI : MonoBehaviour
{
    public static InventarioDetailsUI Instance { get; private set; }

    public Image bg;
    public Image itemIcon;
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;
    public TMP_Text calidadText;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Debug.Log("[InventarioDetailsUI] Inicializando UI de Detalles de Inventario");
        itemNameText.text = "";
        itemDescriptionText.text = "";
        calidadText.text = "";
        itemIcon.gameObject.SetActive(false);
    }

    public void ShowItemDetails(ItemInfo item)
    {
        itemIcon.gameObject.SetActive(true);
        itemIcon.sprite = item.icon;
        itemNameText.text = item.itemNombre;
        itemDescriptionText.text = item.itemDescripcion;
        itemIcon.transform.localScale = new Vector3(item.widthModifier, item.heightModifier, 1f);
        switch (item.calidad)
        {
            case PlantaCalidad.Estandar:
                calidadText.text = "Est.";
                calidadText.color = Color.white;
                bg.color = Color.white - new Color(0f, 0f, 0f, 0.3f);
                break;
            case PlantaCalidad.Plata:
                calidadText.text = "Plata";
                calidadText.color = Color.cyan;
                bg.color = Color.cyan - new Color(0f, 0f, 0f, 0.6f);
                break;
            case PlantaCalidad.Oro:
                calidadText.text = "Oro";
                calidadText.color = Color.yellow;
                bg.color = Color.yellow - new Color(0f, 0f, 0f, 0.3f);
                break;
            default:
                calidadText.text = "";
                bg.color = Color.white - new Color(0f, 0f, 0f, 0.3f);
                break;
        }
    }
}