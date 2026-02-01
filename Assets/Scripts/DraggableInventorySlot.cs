using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Canvas rootCanvas;

    [Header("Drag Visual")]
    public bool useGhostIcon = true;
    [Range(0f, 1f)] public float originalIconAlphaOnDrag = 0.25f;

    private CanvasGroup cg;

    private ItemInfo boundInfo;
    private int boundAmount;

    private ItemSO cachedItemSO; // <- cache para que no dependa de cosas raras

    private GameObject ghostGO;
    private RectTransform ghostRT;

    public ItemInfo BoundItemInfo => boundInfo;
    public int BoundAmount => boundAmount;

    public ItemSO BoundItemSO
    {
        get
        {
            if (cachedItemSO != null) return cachedItemSO;
            if (InventorySystem.Instance == null) return null;
            return InventorySystem.Instance.ResolveItemSO(boundInfo);
        }
    }

    // compat
    public ItemSO BoundItem => BoundItemSO;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();

        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>();

        if (rootCanvas == null)
        {
            var c = GetComponentInParent<Canvas>();
            if (c != null) rootCanvas = c.rootCanvas;
        }
    }

    public void Bind(ItemInfo info, int amount)
    {
        boundInfo = info;
        boundAmount = amount;

        cachedItemSO = null;
        if (InventorySystem.Instance != null)
            cachedItemSO = InventorySystem.Instance.ResolveItemSO(boundInfo);

        // Debug ultra claro para encontrar el fallo
        if (cachedItemSO == null && boundInfo != null)
        {
            Debug.LogWarning($"[SLOT BIND] NO mapping ItemSO para info='{boundInfo.itemNombre}' tipo={boundInfo.plantaTipo} calidad={boundInfo.calidad}. Revisa mappings.");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (boundInfo == null || boundAmount <= 0) return;

        if (rootCanvas == null)
        {
            var c = GetComponentInParent<Canvas>();
            if (c != null) rootCanvas = c.rootCanvas;
        }
        if (rootCanvas == null) return;

        cg.blocksRaycasts = false;

        if (iconImage != null)
        {
            var c = iconImage.color;
            iconImage.color = new Color(c.r, c.g, c.b, originalIconAlphaOnDrag);
        }

        if (useGhostIcon) CreateGhost(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostRT == null) return;
        UpdateGhostPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        cg.blocksRaycasts = true;

        if (iconImage != null)
        {
            var c = iconImage.color;
            iconImage.color = new Color(c.r, c.g, c.b, 1f);
        }

        if (ghostGO != null)
        {
            Destroy(ghostGO);
            ghostGO = null;
            ghostRT = null;
        }
    }

    private void CreateGhost(PointerEventData eventData)
    {
        ghostGO = new GameObject("DragGhost");
        ghostGO.transform.SetParent(rootCanvas.transform, false);
        ghostGO.transform.SetAsLastSibling();

        ghostRT = ghostGO.AddComponent<RectTransform>();

        var img = ghostGO.AddComponent<Image>();
        var ghostCG = ghostGO.AddComponent<CanvasGroup>();
        ghostCG.blocksRaycasts = false;
        ghostCG.interactable = false;

        img.sprite = (iconImage != null && iconImage.sprite != null) ? iconImage.sprite : (boundInfo != null ? boundInfo.icon : null);
        img.preserveAspect = true;

        ghostRT.sizeDelta = (iconImage != null) ? iconImage.rectTransform.sizeDelta : new Vector2(80, 80);
        ghostRT.pivot = new Vector2(0.5f, 0.5f);
        ghostRT.anchorMin = new Vector2(0.5f, 0.5f);
        ghostRT.anchorMax = new Vector2(0.5f, 0.5f);

        UpdateGhostPosition(eventData);
    }

    private void UpdateGhostPosition(PointerEventData eventData)
    {
        if (rootCanvas == null || ghostRT == null) return;

        var canvasRT = rootCanvas.transform as RectTransform;

        Camera cam = null;
        if (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = rootCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, eventData.position, cam, out Vector2 localPoint))
            ghostRT.anchoredPosition = localPoint;
    }
}
