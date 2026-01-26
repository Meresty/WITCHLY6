using System;
using UnityEngine;
using UnityEngine.UI;

public enum TiendaItemType { Plant, Seed, Serum }

public class TiendaBuyButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button buyButton;

    [Header("Data (se llena con Bind)")]
    [SerializeField] private TiendaItemType itemType;
    [SerializeField] private int price;
    [SerializeField] private int amount = 1;
    [SerializeField] private ItemInfo item;

    private bool busy;

    private void Awake()
    {
        if (buyButton == null) buyButton = GetComponent<Button>();

        // BLINDAJE: elimina listeners viejos y conecta este
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnClickBuy);
        }
    }

    // Llamalo cuando creas cada card
    public void Bind(ItemInfo itemInfo, int itemPrice, TiendaItemType type, int qty = 1)
    {
        item = itemInfo;
        price = Mathf.Max(0, itemPrice);
        itemType = type;
        amount = Mathf.Max(1, qty);

        Debug.Log($"[BUY-BIND] item={(item != null ? item.itemNombre : "NULL")} price={price} type={itemType} qty={amount}");
    }

    public async void OnClickBuy()
    {
        if (busy) return;
        busy = true;

        try
        {
            Debug.Log("[BUY] Click detectado");

            if (!FirebaseInitializer.IsReady)
            {
                Debug.LogWarning("[BUY] Firebase no listo");
                return;
            }

            string username = (GameSession.Instance != null) ? GameSession.Instance.Username : "";
            Debug.Log("[BUY] username=" + username);

            if (string.IsNullOrEmpty(username))
            {
                Debug.LogWarning("[BUY] No hay usuario en GameSession (estas entrando a Tienda sin login?)");
                return;
            }

            if (item == null)
            {
                Debug.LogWarning("[BUY] item == null (no se llamo Bind en la card)");
                return;
            }

            if (FirebaseCoinsManager.Instance == null)
            {
                Debug.LogWarning("[BUY] No existe FirebaseCoinsManager en DontDestroyOnLoad");
                return;
            }

            if (!FirebaseCoinsManager.Instance.IsBound)
                FirebaseCoinsManager.Instance.BindUser(username);

            if (FirebaseInventoryManager.Instance == null)
            {
                Debug.LogWarning("[BUY] No existe FirebaseInventoryManager en DontDestroyOnLoad");
                return;
            }

            if (!FirebaseInventoryManager.Instance.IsBound)
                FirebaseInventoryManager.Instance.BindUser(username);

            Debug.Log($"[BUY] Intentando pagar price={price} coinsLocal={FirebaseCoinsManager.Instance.Coins}");

            // 1) COBRAR (transaccion en Firebase)
            bool paid = await FirebaseCoinsManager.Instance.TrySpendCoinsAsync(price);
            Debug.Log("[BUY] paid=" + paid);

            if (!paid)
            {
                Debug.Log("[TIENDA] No tienes monedas suficientes.");
                return;
            }

            // 2) INVENTARIO LOCAL
            bool localOk = AddToLocalInventory();
            Debug.Log("[BUY] localOk=" + localOk);

            // 3) INVENTARIO FIREBASE
            string itemId = BuildFirebaseItemId();
            Debug.Log("[BUY] itemId=" + itemId);

            bool fbInvOk = await FirebaseInventoryManager.Instance.AddItemAsync(itemId, amount);
            Debug.Log("[BUY] fbInvOk=" + fbInvOk);

            Debug.Log("[BUY] Compra terminada OK");
        }
        catch (Exception ex)
        {
            Debug.LogError("[BUY] Error: " + ex);
        }
        finally
        {
            busy = false;
        }
    }

    private bool AddToLocalInventory()
    {
        InventorySystem inv = null;

        // intenta Instance si existe
        try { inv = InventorySystem.Instance; } catch { }

        // fallback si no tienes singleton
#if UNITY_2023_1_OR_NEWER
        if (inv == null) inv = FindAnyObjectByType<InventorySystem>();
#else
        if (inv == null) inv = FindObjectOfType<InventorySystem>();
#endif

        if (inv == null) return false;

        switch (itemType)
        {
            case TiendaItemType.Serum:
                inv.AddSerum(item.itemNombre, amount);
                return true;

            case TiendaItemType.Seed:
                inv.AddSemilla(item.plantaTipo, amount);
                return true;

            case TiendaItemType.Plant:
                inv.AddPlant(item.plantaTipo, item.calidad, amount);
                return true;
        }

        return false;
    }

    private string BuildFirebaseItemId()
    {
        switch (itemType)
        {
            case TiendaItemType.Serum:
                return "serum_" + Normalize(item.itemNombre);

            case TiendaItemType.Seed:
                return "seed_" + item.plantaTipo.ToString().ToLowerInvariant();

            case TiendaItemType.Plant:
                return "plant_" + item.plantaTipo.ToString().ToLowerInvariant() + "_" + item.calidad.ToString().ToLowerInvariant();
        }
        return "unknown";
    }

    private string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "x";
        s = s.Trim().ToLowerInvariant().Replace(" ", "_");
        // evita caracteres invalidos en keys de Firebase
        s = s.Replace(".", "_").Replace("#", "_").Replace("$", "_").Replace("[", "_").Replace("]", "_").Replace("/", "_");
        return s;
    }
}
