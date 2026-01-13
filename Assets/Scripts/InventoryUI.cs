using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public Transform semillasContent;
    public Transform plantasContent;
    public Transform suerosContent;

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
        RefreshSemillas();
        RefreshPlantas();
        RefreshSueros();
    }

    public void CheckNotNull(GameObject obj, string fieldName)
    {
        if (obj == null)
        {
            Debug.LogError($"[InventoryUI] ¡El campo '{fieldName}' no está asignado en el Inspector!");
        }
    }

    public void RefreshSemillas()
    {
        CheckNotNull(semillasContent.gameObject, "semillasContent");
        CheckNotNull(InventorySystem.Instance.gameObject, "InventorySystem");

        foreach (Transform child in semillasContent) { Destroy(child.gameObject); }

        foreach (var semilla in InventorySystem.Instance.semillas)
        {
            Debug.Log($"[InventoryUI] Procesando semilla: {semilla.plantaTipo} x{semilla.cantidad}");
            GameObject slot = Instantiate(itemSlotPrefab, semillasContent);

            PlantasData plantaData = InvernaderoManager.Instance.plantDatabase.GetPlantas(semilla.plantaTipo);
            slot.GetComponent<InventorySlot>().Setup(
                new ItemInfo {
                    itemNombre = plantaData.nombre,
                    itemDescripcion = plantaData.descripcion,
                    icon = plantaData.semillaSprite,
                    widthModifier = plantaData.widthModifier,
                    heightModifier = plantaData.heightModifier
                },
                semilla.cantidad
            );
        }
    }

    public void RefreshPlantas()
    {
        CheckNotNull(plantasContent.gameObject, "plantasContent");
        CheckNotNull(InventorySystem.Instance.gameObject, "InventorySystem");

        foreach (Transform child in plantasContent) { Destroy(child.gameObject); }

        foreach (var planta in InventorySystem.Instance.plantas)
        {
            Debug.Log($"[InventoryUI] Procesando planta: {planta.plantaTipo} x{planta.cantidad}");
            GameObject slot = Instantiate(itemSlotPrefab, plantasContent);

            PlantasData plantaData = InvernaderoManager.Instance.plantDatabase.GetPlantas(planta.plantaTipo);
            slot.GetComponent<InventorySlot>().Setup(
                new ItemInfo {
                    itemNombre = plantaData.nombre,
                    itemDescripcion = plantaData.descripcion,
                    icon = plantaData.frutoSprite,
                    calidad = planta.calidad,
                    widthModifier = plantaData.widthModifier,
                    heightModifier = plantaData.heightModifier
                },
                planta.cantidad
            );
        }
    }

    public void RefreshSueros()
    {
        CheckNotNull(suerosContent.gameObject, "suerosContent");
        CheckNotNull(InventorySystem.Instance.gameObject, "InventorySystem");

        foreach (Transform child in suerosContent) { Destroy(child.gameObject); }

        foreach (var suero in InventorySystem.Instance.sueros)
        {
            Debug.Log($"[InventoryUI] Procesando suero: {suero.sueroNombre} x{suero.cantidad}");
            GameObject slot = Instantiate(itemSlotPrefab, suerosContent);

            SueroData sueroInfo = InvernaderoManager.Instance.sueroDatabase.GetSueroByName(suero.sueroNombre);
            slot.GetComponent<InventorySlot>().Setup(
                new ItemInfo {
                    itemNombre = sueroInfo.nombre,
                    itemDescripcion = sueroInfo.descripcion,
                    icon = sueroInfo.sueroSprite,
                    widthModifier = sueroInfo.widthModifier,
                    heightModifier = sueroInfo.heightModifier
                },
                suero.cantidad
            );
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

    public void DisplayDetails(ItemInfo item)
    {
        InventarioDetailsUI.Instance.ShowItemDetails(item);
    }
}