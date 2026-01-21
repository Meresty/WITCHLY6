using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_OrdenDeCompra : MonoBehaviour
{
    // =========================================================
    // MODO TIENDA (Controller)
    // =========================================================
    [Header("MODO TIENDA (Controller)")]
    [SerializeField] private bool esController = false;

    [Tooltip("Scroll View/Viewport/Content")]
    [SerializeField] private Transform content;

    [Tooltip("Prefab de tarjeta. Debe tener este mismo script con esController OFF.")]
    [SerializeField] private UI_OrdenDeCompra cardPrefab;

    [Header("DBs (ScriptableObjects)")]
    [SerializeField] private ScriptableObject plantaDB;
    [SerializeField] private ScriptableObject sueroDB;

    [Header("Referencias (Monedas e Inventario)")]
    [Tooltip("Arrastra aqui el GameObject que tenga MonedasManager.")]
    [SerializeField] private MonedasManager walletBehaviour;

    [Tooltip("Arrastra aqui el GameObject que tenga InventorySystem.")]
    [SerializeField] private InventorySystem inventoryBehaviour;

    [Header("Generacion")]
    [SerializeField] private int maxProductos = 12;
    [SerializeField] private Vector2Int cantidadMinMax = new Vector2Int(1, 10);

    [Header("Refresh tienda")]
    [Tooltip("Cada cuantas horas se refresca la tienda.")]
    [SerializeField] private float refreshCadaHoras = 2f;

    [Header("Fluctuacion de precio por refresh")]
    [Range(0f, 1f)][SerializeField] private float fluctuacionMin = 0.05f; // 5%
    [Range(0f, 1f)][SerializeField] private float fluctuacionMax = 0.09f; // 9%

    [Header("Calidad (solo Plantas y Semillas)")]
    [Range(0, 100)][SerializeField] private int probEstandar = 70;
    [Range(0, 100)][SerializeField] private int probPlata = 20;
    [Range(0, 100)][SerializeField] private int probOro = 10;

    [Header("Multiplicadores por calidad (precio)")]
    [SerializeField] private float multEstandar = 1f;
    [SerializeField] private float multPlata = 1.15f;
    [SerializeField] private float multOro = 1.30f;

    [Header("Vendedores (nombres aleatorios)")]
    [SerializeField] private List<string> vendorNames = new List<string>();

    private Coroutine refreshRoutine;

    // Cache por periodo
    private static float s_nextRefreshTime = -1f;
    private static List<Oferta> s_cachedOfertas;

    // =========================================================
    // MODO CARD (UI de una orden)
    // =========================================================
    [Header("MODO CARD (UI de una orden)")]
    [SerializeField] private int cantidadObjeto;
    [SerializeField] private int costoObjeto;

    [SerializeField] private Image iconoObjeto;
    [SerializeField] private TextMeshProUGUI nombreVendedor;
    [SerializeField] private TextMeshProUGUI nombreObjeto;
    [SerializeField] private TextMeshProUGUI text_cantidadObjeto;
    [SerializeField] private TextMeshProUGUI text_costoObjeto;

    [Header("Boton comprar (opcional)")]
    [SerializeField] private Button buyButton;

    private Oferta oferta;

    private enum TipoProducto { Planta, Semilla, Suero }
    private enum Calidad { Ninguna, Estandar, Plata, Oro }

    [Serializable]
    private class Oferta
    {
        public string vendedor;
        public string productoOriginal;  // se usa para inventario (para no romper nombres)
        public int cantidad;
        public int precioUnitario;
        public Sprite icono;
        public object source;

        public TipoProducto tipo;
        public Calidad calidad;

        // para inventario
        public PlantaTipo plantaTipo; // solo aplica a plantas/semillas
    }

    // =========================================================
    // Precios base (tu tabla)
    // =========================================================
    private static readonly Dictionary<string, int> PRECIOS_PLANTAS = new Dictionary<string, int>
    {
        { "lumina", 13 },
        { "falsibaya", 8 },
        { "drakonia", 8 },
        { "eldebria", 10 },
        { "jiveria", 11 },
        { "lirien", 17 },
    };

    private static readonly Dictionary<string, int> PRECIOS_SEMILLAS = new Dictionary<string, int>
    {
        { "lumina", 7 },
        { "eldebria", 5 },
        { "jiveria", 6 },
        { "lirien", 8 },
        // falsibaya N/A, drakonia N/A
    };

    private static readonly Dictionary<string, int> PRECIOS_SUEROS = new Dictionary<string, int>
    {
        { "suero de atadura", 19 },
        { "suero de fuerza", 17 },
        { "suero de congelamiento", 20 },
        { "suero de energia", 15 },
    };

    // =========================================================
    // Unity Lifecycle
    // =========================================================
    private void Awake()
    {
        EnsureDefaultVendors();

        if (!esController && buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(ComprarObjeto);
        }
    }

    private void OnEnable()
    {
        if (!esController) return;

        AutoWireControllerIfNeeded();
        RenderOrRefreshIfNeeded(force: false);

        if (refreshRoutine != null) StopCoroutine(refreshRoutine);
        refreshRoutine = StartCoroutine(RefreshLoop());
    }

    private void OnDisable()
    {
        if (!esController) return;

        if (refreshRoutine != null)
        {
            StopCoroutine(refreshRoutine);
            refreshRoutine = null;
        }
    }

    private void AutoWireControllerIfNeeded()
    {
        if (content == null)
        {
            var sr = GetComponentInChildren<ScrollRect>(true);
            if (sr != null && sr.content != null) content = sr.content;
        }

        if (walletBehaviour == null) walletBehaviour = MonedasManager.Instance;
        if (inventoryBehaviour == null) inventoryBehaviour = InventorySystem.Instance;
    }

    private IEnumerator RefreshLoop()
    {
        float seconds = Mathf.Max(1f, refreshCadaHoras * 3600f);
        while (true)
        {
            yield return new WaitForSecondsRealtime(seconds);
            RenderOrRefreshIfNeeded(force: true);
        }
    }

    // =========================================================
    // Controller Render
    // =========================================================
    private void RenderOrRefreshIfNeeded(bool force)
    {
        if (content == null || cardPrefab == null) return;

        float now = Time.realtimeSinceStartup;
        float refreshSeconds = Mathf.Max(1f, refreshCadaHoras * 3600f);

        bool needRefresh = force || s_cachedOfertas == null || s_nextRefreshTime < 0f || now >= s_nextRefreshTime;

        if (needRefresh)
        {
            s_cachedOfertas = BuildOfertasDesdeDBConTiposYPrecios();
            s_nextRefreshTime = now + refreshSeconds;
        }

        ClearContent();
        RenderCards(s_cachedOfertas);
    }

    private void ClearContent()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
    }

    private void RenderCards(List<Oferta> ofertas)
    {
        for (int i = 0; i < ofertas.Count; i++)
        {
            var card = Instantiate(cardPrefab, content);
            card.transform.localScale = Vector3.one;
            card.SetupCard(ofertas[i], walletBehaviour, inventoryBehaviour);
        }

        Canvas.ForceUpdateCanvases();
        if (content is RectTransform rt)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    // =========================================================
    // Ofertas
    // =========================================================
    private List<Oferta> BuildOfertasDesdeDBConTiposYPrecios()
    {
        var plantas = new List<object>();
        var sueros = new List<object>();

        CollectFromDB(plantaDB, new[] { "Plantas", "plantas" }, plantas);
        CollectFromDB(sueroDB, new[] { "Sueros", "sueros" }, sueros);

        var catalogo = new List<(TipoProducto tipo, object source, string nombreOriginal)>();

        // Plantas -> agrega Planta y (si hay precio) agrega Semilla
        for (int i = 0; i < plantas.Count; i++)
        {
            object obj = plantas[i];
            if (obj == null) continue;

            string nombre = GetNombreProducto(obj);
            if (string.IsNullOrWhiteSpace(nombre)) continue;

            catalogo.Add((TipoProducto.Planta, obj, nombre));

            if (GetPrecioBase(TipoProducto.Semilla, nombre, obj) > 0)
                catalogo.Add((TipoProducto.Semilla, obj, nombre));
        }

        // Sueros
        for (int i = 0; i < sueros.Count; i++)
        {
            object obj = sueros[i];
            if (obj == null) continue;

            string nombre = GetNombreProducto(obj);
            if (string.IsNullOrWhiteSpace(nombre)) continue;

            catalogo.Add((TipoProducto.Suero, obj, nombre));
        }

        if (catalogo.Count == 0) return new List<Oferta>();

        Shuffle(catalogo);

        int take = Mathf.Min(maxProductos, catalogo.Count);
        var result = new List<Oferta>(take);

        for (int i = 0; i < take; i++)
        {
            var entry = catalogo[i];

            Calidad cal = (entry.tipo == TipoProducto.Suero) ? Calidad.Ninguna : RollCalidad();

            int basePrice = GetPrecioBase(entry.tipo, entry.nombreOriginal, entry.source);
            if (basePrice <= 0) basePrice = 1;

            float multCal = GetMultCalidad(cal);

            float minF = Mathf.Min(fluctuacionMin, fluctuacionMax);
            float maxF = Mathf.Max(fluctuacionMin, fluctuacionMax);

            float pct = UnityEngine.Random.Range(minF, maxF);
            float sign = (UnityEngine.Random.value < 0.5f) ? -1f : 1f;
            float multFluct = 1f + (sign * pct);

            int precioUnit = Mathf.RoundToInt(basePrice * multCal * multFluct);
            precioUnit = Mathf.Max(1, precioUnit);

            int qty = UnityEngine.Random.Range(cantidadMinMax.x, cantidadMinMax.y + 1);

            // Resolver PlantaTipo para plantas/semillas (para poder guardar en inventario)
            PlantaTipo pt = default;
            if (entry.tipo != TipoProducto.Suero)
            {
                if (!TryResolvePlantaTipo(entry.source, entry.nombreOriginal, out pt))
                {
                    // Si no se puede resolver, aun se muestra, pero comprar no agregara al inventario
                    // (igual lo dejamos para que lo veas en consola)
                }
            }

            result.Add(new Oferta
            {
                vendedor = PickVendedorName(),
                productoOriginal = entry.nombreOriginal,
                cantidad = qty,
                precioUnitario = precioUnit,
                icono = TryGetSprite(entry.source),
                source = entry.source,
                tipo = entry.tipo,
                calidad = cal,
                plantaTipo = pt
            });
        }

        return result;
    }

    private bool TryResolvePlantaTipo(object source, string nombre, out PlantaTipo tipo)
    {
        tipo = default;

        // 1) buscar campo/prop de tipo PlantaTipo
        if (source != null)
        {
            var t = source.GetType();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var f in t.GetFields(flags))
            {
                if (f.FieldType == typeof(PlantaTipo))
                {
                    tipo = (PlantaTipo)f.GetValue(source);
                    return true;
                }
            }
            foreach (var p in t.GetProperties(flags))
            {
                if (p.PropertyType == typeof(PlantaTipo) && p.CanRead)
                {
                    tipo = (PlantaTipo)p.GetValue(source);
                    return true;
                }
            }
        }

        // 2) parse por nombre
        string key = NormalizeKey(nombre);
        if (Enum.TryParse(key, true, out tipo))
            return true;

        return false;
    }

    private string GetNombreProducto(object obj)
    {
        string n = GetStringMember(obj, "nombre", "Nombre", "itemNombre", "ItemNombre", "name", "Name");
        if (string.IsNullOrWhiteSpace(n) && obj is UnityEngine.Object uo) n = uo.name;
        if (string.IsNullOrWhiteSpace(n)) n = obj != null ? obj.ToString() : "";
        return n.Trim();
    }

    private int GetPrecioBase(TipoProducto tipo, string nombreProducto, object source)
    {
        string key = NormalizeKey(nombreProducto);

        if (tipo == TipoProducto.Planta && PRECIOS_PLANTAS.TryGetValue(key, out int pPlant)) return pPlant;
        if (tipo == TipoProducto.Semilla && PRECIOS_SEMILLAS.TryGetValue(key, out int pSeed)) return pSeed;
        if (tipo == TipoProducto.Suero && PRECIOS_SUEROS.TryGetValue(key, out int pSuero)) return pSuero;

        int p = GetIntMember(source,
            "precioVentaEstandar", "PrecioVentaEstandar",
            "precio", "Precio",
            "price", "Price",
            "precioBase", "PrecioBase",
            "costo", "Costo"
        );

        return p;
    }

    private Calidad RollCalidad()
    {
        int a = Mathf.Max(0, probEstandar);
        int b = Mathf.Max(0, probPlata);
        int c = Mathf.Max(0, probOro);
        int sum = a + b + c;
        if (sum <= 0) return Calidad.Estandar;

        int r = UnityEngine.Random.Range(1, sum + 1);
        if (r <= a) return Calidad.Estandar;
        if (r <= a + b) return Calidad.Plata;
        return Calidad.Oro;
    }

    private float GetMultCalidad(Calidad cal)
    {
        if (cal == Calidad.Plata) return multPlata;
        if (cal == Calidad.Oro) return multOro;
        if (cal == Calidad.Estandar) return multEstandar;
        return 1f;
    }

    private string CalidadToText(Calidad cal)
    {
        if (cal == Calidad.Plata) return "Plata";
        if (cal == Calidad.Oro) return "Oro";
        if (cal == Calidad.Estandar) return "Estandar";
        return "";
    }

    private string TipoToText(TipoProducto t)
    {
        if (t == TipoProducto.Planta) return "Planta";
        if (t == TipoProducto.Semilla) return "Semilla";
        return "Suero";
    }

    private string PickVendedorName()
    {
        EnsureDefaultVendors();
        if (vendorNames != null && vendorNames.Count > 0)
            return vendorNames[UnityEngine.Random.Range(0, vendorNames.Count)];
        return "Vendedor";
    }

    private void EnsureDefaultVendors()
    {
        if (vendorNames == null) vendorNames = new List<string>();
        if (vendorNames.Count > 0) return;

        vendorNames.AddRange(new[]
        {
            "Penryn","Merlin","Amarys","Oren","Selene","Thalion","Ezrael","Lilith",
            "Sara","Odrian","Grelia","Valinor","Serena","Agatha","Eldrin"
        });
    }

    // =========================================================
    // DB Read (Reflection)
    // =========================================================
    private void CollectFromDB(ScriptableObject db, string[] listNames, List<object> outList)
    {
        if (db == null) return;

        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var t = db.GetType();

        object listObj = null;

        foreach (var ln in listNames)
        {
            var f = t.GetField(ln, flags);
            if (f != null) { listObj = f.GetValue(db); break; }

            var p = t.GetProperty(ln, flags);
            if (p != null) { listObj = p.GetValue(db); break; }
        }

        if (!(listObj is IEnumerable enumerable)) return;

        foreach (var elem in enumerable)
        {
            if (elem == null) continue;
            outList.Add(elem);
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    // =========================================================
    // Card Setup + Comprar
    // =========================================================
    private MonedasManager cardWallet;
    private InventorySystem cardInventory;

    private void SetupCard(Oferta o, MonedasManager wallet, InventorySystem inventory)
    {
        esController = false;
        oferta = o;

        cardWallet = wallet != null ? wallet : MonedasManager.Instance;
        cardInventory = inventory != null ? inventory : InventorySystem.Instance;

        if (nombreVendedor != null) nombreVendedor.text = RemoveDiacritics(o.vendedor);

        string tipoTxt = TipoToText(o.tipo);
        string prodTxt = RemoveDiacritics(o.productoOriginal);

        string body;
        if (o.tipo == TipoProducto.Suero)
            body = tipoTxt + ":\n" + prodTxt;
        else
            body = tipoTxt + ":\n" + prodTxt + "\n(" + CalidadToText(o.calidad) + ")";

        if (nombreObjeto != null) nombreObjeto.text = body;

        cantidadObjeto = o.cantidad;
        costoObjeto = o.precioUnitario;

        if (text_cantidadObjeto != null) text_cantidadObjeto.text = cantidadObjeto.ToString();

        // Solo numero, sin simbolos
        if (text_costoObjeto != null) text_costoObjeto.text = costoObjeto.ToString();

        if (iconoObjeto != null)
        {
            iconoObjeto.sprite = o.icono;
            iconoObjeto.enabled = (o.icono != null);
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(ComprarObjeto);
        }
    }

    public void ComprarObjeto()
    {
        if (oferta == null) return;
        if (cantidadObjeto <= 0) return;

        cardWallet = cardWallet != null ? cardWallet : MonedasManager.Instance;
        cardInventory = cardInventory != null ? cardInventory : InventorySystem.Instance;

        if (cardWallet == null)
        {
            Debug.LogError("[TIENDA] MonedasManager no esta disponible.");
            return;
        }
        if (cardInventory == null)
        {
            Debug.LogError("[TIENDA] InventorySystem no esta disponible.");
            return;
        }

        int cost = Mathf.Max(1, oferta.precioUnitario);

        // 1) Cobrar
        if (!cardWallet.GastarMonedas(cost))
        {
            Debug.Log("[TIENDA] No alcanza el dinero. Costo: " + cost + " Coins: " + cardWallet.GetMonedas());
            return;
        }

        // 2) Guardar en inventario
        bool added = AddToInventory(cardInventory, oferta, 1);
        if (!added)
        {
            // Reembolso
            cardWallet.AnadirMonedas(cost);
            Debug.LogWarning("[TIENDA] No pude agregar al inventario. Reembolso aplicado.");
            return;
        }

        // 3) Bajar stock
        cantidadObjeto--;
        oferta.cantidad = cantidadObjeto;

        if (text_cantidadObjeto != null) text_cantidadObjeto.text = cantidadObjeto.ToString();

        if (cantidadObjeto <= 0)
            Destroy(gameObject);

        Debug.Log("[TIENDA] Compra OK: " + oferta.productoOriginal + " x1, costo " + cost);
    }

    private bool AddToInventory(InventorySystem inv, Oferta o, int amount)
    {
        try
        {
            if (o.tipo == TipoProducto.Suero)
            {
                // Evita duplicados por acentos: intenta usar el nombre que ya exista en el inventario
                string nameToUse = ResolveSerumNameForInventory(inv, o.productoOriginal);
                inv.AddSerum(nameToUse, amount);
                return true;
            }

            // Plantas/Semillas necesitan PlantaTipo
            if (!Enum.IsDefined(typeof(PlantaTipo), o.plantaTipo))
            {
                Debug.LogWarning("[TIENDA] No pude resolver PlantaTipo para: " + o.productoOriginal);
                return false;
            }

            if (o.tipo == TipoProducto.Semilla)
            {
                // Nota: tu InventorySystem no guarda calidad en semillas, aqui solo se agrega por tipo
                inv.AddSemilla(o.plantaTipo, amount);
                return true;
            }

            // Planta
            PlantaCalidad q = MapCalidadToPlantaCalidad(o.calidad);
            inv.AddPlant(o.plantaTipo, q, amount);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[TIENDA] Error al agregar al inventario: " + e.Message);
            return false;
        }
    }

    private string ResolveSerumNameForInventory(InventorySystem inv, string incomingName)
    {
        if (inv == null || inv.sueros == null) return incomingName;

        string key = NormalizeKey(incomingName);

        for (int i = 0; i < inv.sueros.Count; i++)
        {
            var s = inv.sueros[i];
            if (s == null) continue;

            if (NormalizeKey(s.sueroNombre) == key)
                return s.sueroNombre; // usa el nombre ya guardado (puede tener acento)
        }

        return incomingName;
    }

    private PlantaCalidad MapCalidadToPlantaCalidad(Calidad cal)
    {
        if (cal == Calidad.Plata) return PlantaCalidad.Plata;
        if (cal == Calidad.Oro) return PlantaCalidad.Oro;
        return PlantaCalidad.Estandar;
    }

    // =========================================================
    // Small reflection helpers for DB and sprite
    // =========================================================
    private static readonly BindingFlags RF = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private string GetStringMember(object obj, params string[] names)
    {
        if (obj == null) return null;

        var t = obj.GetType();
        foreach (var n in names)
        {
            var f = t.GetField(n, RF);
            if (f != null && f.FieldType == typeof(string)) return f.GetValue(obj) as string;

            var p = t.GetProperty(n, RF);
            if (p != null && p.PropertyType == typeof(string)) return p.GetValue(obj) as string;
        }
        return null;
    }

    private int GetIntMember(object obj, params string[] names)
    {
        if (obj == null) return 0;

        var t = obj.GetType();
        foreach (var n in names)
        {
            var f = t.GetField(n, RF);
            if (f != null && f.FieldType == typeof(int)) return (int)f.GetValue(obj);

            var p = t.GetProperty(n, RF);
            if (p != null && p.PropertyType == typeof(int)) return (int)p.GetValue(obj);
        }
        return 0;
    }

    private Sprite TryGetSprite(object obj)
    {
        if (obj == null) return null;

        var t = obj.GetType();

        var f = t.GetField("icon", RF) ?? t.GetField("Icon", RF) ?? t.GetField("sprite", RF) ?? t.GetField("Sprite", RF);
        if (f != null && typeof(Sprite).IsAssignableFrom(f.FieldType)) return f.GetValue(obj) as Sprite;

        var p = t.GetProperty("icon", RF) ?? t.GetProperty("Icon", RF) ?? t.GetProperty("sprite", RF) ?? t.GetProperty("Sprite", RF);
        if (p != null && typeof(Sprite).IsAssignableFrom(p.PropertyType)) return p.GetValue(obj) as Sprite;

        return null;
    }

    // =========================================================
    // Text normalize helpers
    // =========================================================
    private string NormalizeKey(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";

        s = s.Trim().ToLowerInvariant();
        string formD = s.Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder(formD.Length);
        for (int i = 0; i < formD.Length; i++)
        {
            char ch = formD[i];
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private string RemoveDiacritics(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;

        string formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);

        for (int i = 0; i < formD.Length; i++)
        {
            char ch = formD[i];
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
