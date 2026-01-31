using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public Image iconImage;          // arrastra aqui tu Icono (Image)
    public Canvas rootCanvas;        // opcional, se autodetecta

    [Header("Drag Visual")]
    public bool useGhostIcon = true;
    [Range(0f, 1f)] public float originalIconAlphaOnDrag = 0.25f;

    [Header("Caldero (opcional)")]
    public GameObject calderoObjeto;
    public CalderoDangerZone caldero;

    private CanvasGroup cg;
    public ItemInfo boundInfo;
    private int boundAmount;

    private GameObject ghostGO;
    private RectTransform ghostRT;

    public ItemInfo BoundItemInfo => boundInfo;
    public int BoundAmount => boundAmount;

    // Compat: scripts viejos esperan ItemSO
    public ItemSO BoundItemSO => (InventorySystem.Instance != null) ? InventorySystem.Instance.ResolveItemSO(boundInfo) : null;
    public ItemSO BoundItem => BoundItemSO;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();

        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>(true);

        if (rootCanvas == null)
        {
            var c = GetComponentInParent<Canvas>();
            if (c != null) rootCanvas = c.rootCanvas; // top canvas
        }
    }

    private void Start()
    {
        // Intentamos encontrar el caldero SOLO una vez al inicio.
        TryResolveCaldero();
    }

    /// <summary>
    /// Busca DangerZone si existe. Si no existe, NO pasa nada.
    /// Esto evita los NullReference + spam.
    /// </summary>
    private void TryResolveCaldero()
    {
        // Si ya lo tenemos, no hacemos nada
        if (caldero != null) return;

        // Busca por nombre (si en la escena no existe, regresa null y listo)
        calderoObjeto = GameObject.Find("DangerZone");
        if (calderoObjeto == null) return;

        caldero = calderoObjeto.GetComponent<CalderoDangerZone>();
        // Si no tiene ese componente, también lo dejamos como null sin tronar
    }

    public void Bind(ItemInfo info, int amount)
    {
        boundInfo = info;
        boundAmount = amount;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (boundInfo == null) return;
        if (boundAmount <= 0) return;

        if (rootCanvas == null)
        {
            var c = GetComponentInParent<Canvas>();
            if (c != null) rootCanvas = c.rootCanvas;
        }
        if (rootCanvas == null) return;

        // Solo si estás en la escena del caldero, tendrá sentido
        TryResolveCaldero();

        // deja pasar el drop al target
        cg.blocksRaycasts = false;

        // baja alpha del icono original
        if (iconImage != null)
        {
            var c = iconImage.color;
            iconImage.color = new Color(c.r, c.g, c.b, originalIconAlphaOnDrag);
        }

        if (useGhostIcon)
            CreateGhost(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostRT == null) return;
        UpdateGhostPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        cg.blocksRaycasts = true;

        // restaura alpha
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

        // Si existe caldero, le pasamos el itemdrop. Si no, NO hacemos nada.
        if (caldero != null)
        {
            caldero.itemdrop = BoundItemSO;
        }

        var img = ghostGO.AddComponent<Image>();
        var ghostCG = ghostGO.AddComponent<CanvasGroup>();

        ghostCG.blocksRaycasts = false;
        ghostCG.interactable = false;

        // sprite
        img.sprite = (iconImage != null && iconImage.sprite != null) ? iconImage.sprite : (boundInfo != null ? boundInfo.icon : null);
        img.preserveAspect = true;

        // size: copia del icono real
        if (iconImage != null)
            ghostRT.sizeDelta = iconImage.rectTransform.sizeDelta;
        else
            ghostRT.sizeDelta = new Vector2(80, 80);

        // pivot centrado
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
        {
            ghostRT.anchoredPosition = localPoint;
        }
    }
}
