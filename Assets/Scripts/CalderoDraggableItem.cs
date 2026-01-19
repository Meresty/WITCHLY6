using UnityEngine;
using UnityEngine.EventSystems;

public class CalderoDraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private ItemSO item;
    [SerializeField] private Transform dragRoot;

    private RectTransform rt;
    private CanvasGroup cg;

    private Transform originalParent;
    private Vector2 originalAnchoredPos;

    private bool consumed;

    public ItemSO Item => item;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
    }

    // Se llama desde CalderoInventoryUI al crear el slot
    public void Init(ItemSO itemSO, Transform dragRootTransform)
    {
        item = itemSO;
        dragRoot = dragRootTransform;
    }

    public void MarkConsumed()
    {
        consumed = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (dragRoot == null)
        {
            Debug.LogWarning("[CalderoDraggableItem] dragRoot es null. Asigna Drag Root en CalderoInventoryUI.");
            return;
        }

        originalParent = transform.parent;
        originalAnchoredPos = rt.anchoredPosition;

        transform.SetParent(dragRoot, true);
        transform.SetAsLastSibling();

        cg.blocksRaycasts = false; // clave para que el DropZone reciba el drop
    }

    public void OnDrag(PointerEventData eventData)
    {
        // seguimos el cursor
        rt.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        cg.blocksRaycasts = true;

        // Si el DropZone lo consumió, aquí lo destruimos (así evitamos MissingReference)
        if (consumed)
        {
            Destroy(gameObject);
            return;
        }

        // Si no se consumió, regresa al slot
        if (originalParent != null)
        {
            transform.SetParent(originalParent, true);
            rt.anchoredPosition = originalAnchoredPos;
        }
    }
}
