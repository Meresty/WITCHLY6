using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public Transform semillasContent;
    public Transform plantasContent;
    public Transform suerosContent;

    public static InventoryUI Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged += RefreshInventory;

        RefreshInventory();
    }

    void OnDestroy()
    {
        // FIX NullReference
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged -= RefreshInventory;
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

    void ClearChildren(Transform parent)
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

    public void RefreshSemillas()
    {
        if (semillasContent == null || itemSlotPrefab == null) return;
        if (InventorySystem.Instance == null) return;
        if (InventorySystem.Instance.plantBD == null) return;

        ClearChildren(semillasContent);

        foreach (var semilla in InventorySystem.Instance.semillas)
        {
            PlantData plantaData = InventorySystem.Instance.plantBD.GetPlantas(semilla.plantaTipo);
            if (plantaData == null) continue;

            GameObject slotGO = Instantiate(itemSlotPrefab, semillasContent);
            var slot = slotGO.GetComponent<InventorySlot>();

            slot.Setup(new ItemInfo
            {
                itemNombre = plantaData.nombre,
                itemDescripcion = plantaData.descripcion,
                icon = plantaData.semillaSprite,
                widthModifier = plantaData.widthModifier,
                heightModifier = plantaData.heightModifier,
                plantaTipo = semilla.plantaTipo,
                calidad = PlantaCalidad.NONE
            }, semilla.cantidad);
        }
    }

    public void RefreshPlantas()
    {
        if (plantasContent == null || itemSlotPrefab == null) return;
        if (InventorySystem.Instance == null) return;
        if (InventorySystem.Instance.plantBD == null) return;

        ClearChildren(plantasContent);

        foreach (var planta in InventorySystem.Instance.plantas)
        {
            PlantData plantaData = InventorySystem.Instance.plantBD.GetPlantas(planta.plantaTipo);
            if (plantaData == null) continue;

            GameObject slotGO = Instantiate(itemSlotPrefab, plantasContent);
            var slot = slotGO.GetComponent<InventorySlot>();

            slot.Setup(new ItemInfo
            {
                itemNombre = plantaData.nombre,
                itemDescripcion = plantaData.descripcion,
                icon = plantaData.frutoSprite,
                widthModifier = plantaData.widthModifier,
                heightModifier = plantaData.heightModifier,
                plantaTipo = planta.plantaTipo,     // IMPORTANTISIMO para mapear a ItemSO
                calidad = planta.calidad
            }, planta.cantidad);
        }
    }

    public void RefreshSueros()
    {
        if (suerosContent == null || itemSlotPrefab == null) return;
        if (InventorySystem.Instance == null) return;
        if (InventorySystem.Instance.sueroBD == null) return;

        ClearChildren(suerosContent);

        foreach (var suero in InventorySystem.Instance.sueros)
        {
            SueroData data = InventorySystem.Instance.sueroBD.GetSueroByName(suero.sueroNombre);
            if (data == null) continue;

            GameObject slotGO = Instantiate(itemSlotPrefab, suerosContent);
            var slot = slotGO.GetComponent<InventorySlot>();

            slot.Setup(new ItemInfo
            {
                itemNombre = data.nombre, // esto debe coincidir con sueroToItemMapping
                itemDescripcion = data.descripcion,
                icon = data.sueroSprite,
                widthModifier = data.widthModifier,
                heightModifier = data.heightModifier,
                plantaTipo = PlantaTipo.NONE,
                calidad = PlantaCalidad.NONE
            }, suero.cantidad);
        }
    }

    public void DisplayDetails(ItemInfo item)
    {
        if (InventarioDetailsUI.Instance != null)
            InventarioDetailsUI.Instance.ShowItemDetails(item);
    }
}
