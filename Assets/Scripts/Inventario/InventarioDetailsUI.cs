using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventarioDetailsUI : MonoBehaviour
{
    public static InventarioDetailsUI Instance { get; private set; }

    [Header("UI")]
    public Image bg;
    public Image itemIcon;
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;
    public TMP_Text calidadText;

    [Header("Botones")]
    public Button plantarButton;
    public Button venderButton;

    private ItemInfo _selectedItem;

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

        Clear();

        // Hook de botones
        if (plantarButton != null)
        {
            plantarButton.onClick.RemoveAllListeners();
            plantarButton.onClick.AddListener(OnClickPlantar);
        }

        if (venderButton != null)
        {
            venderButton.onClick.RemoveAllListeners();
            venderButton.onClick.AddListener(OnClickVender);
        }
        else
        {
            Debug.LogWarning("[InventarioDetailsUI] venderButton no esta asignado en el Inspector.");
        }
    }

    public void ShowItemDetails(ItemInfo item)
    {
        _selectedItem = item;

        if (item == null)
        {
            Clear();
            return;
        }

        if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(true);
            itemIcon.sprite = item.icon;
            itemIcon.transform.localScale = new Vector3(item.widthModifier, item.heightModifier, 1f);
        }

        if (itemNameText != null) itemNameText.text = item.itemNombre;
        if (itemDescriptionText != null) itemDescriptionText.text = item.itemDescripcion;

        // Calidad + color bg
        if (calidadText != null && bg != null)
        {
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

        // Plantar solo tiene sentido si es SEMILLA (plantaTipo != NONE y calidad == NONE)
        bool esSemilla = (item.plantaTipo != PlantaTipo.NONE && item.calidad == PlantaCalidad.NONE);
        if (plantarButton != null) plantarButton.interactable = esSemilla;

        // Vender: deshabilita si no hay precio (o no hay InventorySystem)
        if (venderButton != null)
        {
            int price = (InventorySystem.Instance != null) ? InventorySystem.Instance.GetSellPrice(item) : 0;
            venderButton.interactable = (price > 0);
        }
    }

    private void Clear()
    {
        _selectedItem = null;

        if (itemNameText != null) itemNameText.text = "";
        if (itemDescriptionText != null) itemDescriptionText.text = "";
        if (calidadText != null) calidadText.text = "";

        if (itemIcon != null) itemIcon.gameObject.SetActive(false);

        if (plantarButton != null) plantarButton.interactable = false;
        if (venderButton != null) venderButton.interactable = false;
    }

    private void OnClickPlantar()
    {
        if (_selectedItem == null) return;

        bool esSemilla = (_selectedItem.plantaTipo != PlantaTipo.NONE && _selectedItem.calidad == PlantaCalidad.NONE);
        if (!esSemilla) return;

        if (InvernaderoManager.Instance != null)
            InvernaderoManager.Instance.TryPlantSeed(_selectedItem.plantaTipo);
    }

    private void OnClickVender()
    {
        // no se puede await en onClick normal -> lanzamos async
        _ = SellSelectedAsync();
    }

    private async Task SellSelectedAsync()
    {
        if (_selectedItem == null) return;
        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("[InventarioDetailsUI] No existe InventorySystem.Instance");
            return;
        }

        // 1 unidad por click (puedes cambiarlo luego)
        bool ok = await InventorySystem.Instance.SellItemAsync(_selectedItem, 1);

        if (!ok)
        {
            Debug.LogWarning("[InventarioDetailsUI] Venta fallo (sin item, sin precio o semilla infinita).");
            return;
        }

        // Si ya no queda stock, limpia detalles
        int remaining = GetRemainingCount(_selectedItem);
        if (remaining <= 0) Clear();
        else ShowItemDetails(_selectedItem); // refresca botones
    }

    private int GetRemainingCount(ItemInfo info)
    {
        if (InventorySystem.Instance == null || info == null) return 0;

        // suero
        if (info.plantaTipo == PlantaTipo.NONE)
            return InventorySystem.Instance.GetSerumCount(info.itemNombre);

        // planta
        if (info.calidad != PlantaCalidad.NONE)
            return InventorySystem.Instance.GetPlantCount(info.plantaTipo, info.calidad);

        // semilla
        return InventorySystem.Instance.GetSeedCount(info.plantaTipo);
    }
}
