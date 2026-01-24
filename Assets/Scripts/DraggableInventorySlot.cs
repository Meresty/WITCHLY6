using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DraggableInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Nuevo (lo que usamos)
    public ItemSO BoundItemSO { get; private set; }
    public ItemInfo BoundItemInfo { get; private set; }
    public int BoundAmount { get; private set; }

    // Compatibilidad con scripts viejos
    public ItemSO BoundItem => BoundItemSO;

    private RectTransform rect;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private LayoutElement layoutElement;

    private Transform originalParent;
    private Vector2 originalAnchoredPos;
    private bool isDragging = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) rootCanvas = FindObjectOfType<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null) layoutElement = gameObject.AddComponent<LayoutElement>();
    }

    public void Bind(ItemInfo info, int amount)
    {
        BoundItemInfo = info;
        BoundAmount = amount;

        // usa InventorySystem para mapear a ItemSO (planta/suero)
        if (InventorySystem.Instance != null)
            BoundItemSO = InventorySystem.Instance.ResolveItemSO(info);
        else
            BoundItemSO = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (BoundItemInfo == null) return;
        if (BoundAmount == 0) return;

        isDragging = true;

        originalParent = transform.parent;
        originalAnchoredPos = rect.anchoredPosition;

        layoutElement.ignoreLayout = true;

        if (rootCanvas != null)
            transform.SetParent(rootCanvas.transform, true);

        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.9f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        if (rootCanvas == null) return;

        Vector2 localPoint;
        RectTransform canvasRect = rootCanvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out localPoint))
        {
            rect.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (originalParent != null)
            transform.SetParent(originalParent, true);

        layoutElement.ignoreLayout = false;
        rect.anchoredPosition = originalAnchoredPos;
    }
}
