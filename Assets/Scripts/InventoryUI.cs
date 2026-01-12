using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public Transform content;
    public Image detailsSprite;

    public static InventoryUI Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Debug.Log("[InventoryUI] Inicializando UI de Inventario");
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


        if (InventorySystem.Instance == null)
        {
            Debug.LogError("[InventoryUI] ¡No hay InventoryManager en la escena!");
            return;
        }

        foreach (var semilla in InventorySystem.Instance.semillas)
        {
            Debug.Log($"[InventoryUI] Procesando semilla: {semilla.plantaTipo} x{semilla.cantidad}");
            if (semilla == null)
                continue;

            GameObject slot = Instantiate(itemSlotPrefab, content);

            // Transform iconoTransform = slot.transform.Find("Icono");
            // Transform cantidadTransform = slot.transform.Find("Cantidad");
            // Transform buttonTransform = slot.transform.Find("Button");

            // if (iconoTransform == null || cantidadTransform == null || buttonTransform == null)
            //     continue;

            // Image iconImage = iconoTransform.GetComponent<Image>();
            PlantasInfo plantasInfo = InvernaderoManager.Instance.plantDatabase.GetPlantas(semilla.plantaTipo);
            slot.GetComponent<InventorySlot>().Setup(
                new ItemSO { itemNombre = plantasInfo.plantaNombre, itemDescripcion = "Sin descripción",
                icon = plantasInfo.semillaSprite },
                semilla.cantidad
            );

            // if (iconImage != null && plantasInfo.semillaSprite != null)
            // {
            //     iconImage.sprite = plantasInfo.semillaSprite;
            // }


            // TextMeshProUGUI cantidadText = cantidadTransform.GetComponent<TextMeshProUGUI>();
            // if (cantidadText != null)
            // {
            //     cantidadText.text = semilla.cantidad == -1 ? "∞" : semilla.cantidad.ToString();
            // }

            // Button boton = buttonTransform.GetComponent<Button>();
            // if (boton != null)
            // {
            //     ItemSO itemRef = semilla;
            //     boton.onClick.AddListener(() => UsarItemEnCaldero(itemRef));
            // }
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

    public void DisplayDetails(ItemSO item)
    {
        Debug.Log($"[InventoryUI] Mostrando detalles para: {item.itemNombre}");
        detailsSprite.sprite = item.icon;
        detailsSprite.gameObject.SetActive(true);
    }
}