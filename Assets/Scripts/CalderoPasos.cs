using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CalderoLogic : MonoBehaviour
{
    public static CalderoLogic instancia;

    [Header("UI")]
    public NotaUI notaUI;
    public Transform contenedorIngredientes;
    public GameObject iconoIngredientePrefab;
    public Image iconoPocionResultado;
    public TextMeshProUGUI textoResultado;

    [Header("Pociones disponibles")]
    public List<PocionSO> todasLasPociones = new List<PocionSO>();

    [Header("Config")]
    public string nombreEscenaMinijuego = "Presicion";

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private bool debugAssetPaths = true;
    [SerializeField] private bool ignorarEstadoGuardado = false; // ponlo true si sospechas que step se queda “pegado”

    private PocionSO potion;
    private int step = 0;
    private int vecesEquivocado = 0;
    private float descuentoPrecio = 0f;

    private readonly List<ItemSO> ingredientesAgregados = new List<ItemSO>();

    private const string K_STEP = "CalderoStep";
    private const string K_FAILS = "VecesEquivocado";
    private const string K_DISC = "DescuentoPrecio";
    private const string K_POCION = "PocionActual";

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        instancia = this;
    }

    private void Start()
    {
        CargarPocionSeleccionada();

        if (!ignorarEstadoGuardado)
            RestaurarEstadoSiAplica();
        else
            LimpiarEstadoLocal();

        if (potion != null)
        {
            if (iconoPocionResultado != null) iconoPocionResultado.sprite = potion.icon;
            if (textoResultado != null) textoResultado.text = $"Creando: {potion.pocionNombre}";
            if (notaUI != null) notaUI.MostrarReceta(potion);
        }

        ReconstruirListaIngredientesPorStep();
        ActualizarUI();

        if (debugLogs)
        {
            Debug.Log($"[CALDERO] START potion={(potion ? potion.pocionNombre : "NULL")} step={step} fails={vecesEquivocado}");
            DebugDumpReceta();
        }
    }

    public void ForceIncorrectDrop()
    {
        OnIngredienteIncorrecto(null, null);
    }

    public void AddIngredient(ItemSO itemDrop)
    {
        if (potion == null)
        {
            Debug.LogWarning("[CALDERO] potion es NULL. Revisa PlayerPrefs 'PocionSeleccionada' y 'todasLasPociones' en el inspector.");
            return;
        }

        if (step >= 3)
        {
            if (textoResultado != null) textoResultado.text = "Pocion ya completada";
            return;
        }

        ItemSO esperadoRaw = GetEsperado(step);
        ItemSO dropRaw = itemDrop;

        // Canonicalizar para evitar el bug típico: “mismo nombre pero otro asset”
        ItemSO esperado = Canon(esperadoRaw);
        ItemSO drop = Canon(dropRaw);

        if (debugLogs)
        {
            Debug.Log($"[CALDERO CHECK] step={step}");
            Debug.Log($"  esperadoRaw: {Ref(esperadoRaw)}");
            Debug.Log($"  dropRaw    : {Ref(dropRaw)}");
            Debug.Log($"  esperadoCAN: {Ref(esperado)}");
            Debug.Log($"  dropCAN    : {Ref(drop)}");
            Debug.Log($"  keyExp='{Key(esperado)}' keyDrop='{Key(drop)}'");
        }

        if (drop == null || esperado == null)
        {
            // Si esperado es null: receta mal configurada en el PocionSO o potion no tiene asignados los slots
            // Si drop es null: tu Draggable no está resolviendo BoundItemSO correctamente
            OnIngredienteIncorrecto(drop, esperado);
            return;
        }

        bool ok = IsSameItem(drop, esperado);

        if (ok)
            OnIngredienteCorrecto(drop);
        else
            OnIngredienteIncorrecto(drop, esperado);
    }

    private ItemSO GetEsperado(int s)
    {
        if (potion == null) return null;
        if (s == 0) return potion.suero;
        if (s == 1) return potion.ingrediente1;
        if (s == 2) return potion.ingrediente2;
        return null;
    }

    // Normaliza por nombre (sin acentos, sin espacios, sin guiones)
    private string Key(ItemSO x)
    {
        if (x == null) return "";
        if (InventorySystem.Instance != null) return InventorySystem.Instance.NormalizeItemKey(x.name);
        return x.name.Replace("(Clone)", "").Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
    }

    private ItemSO Canon(ItemSO x)
    {
        if (x == null) return null;
        if (InventorySystem.Instance == null) return x;
        return InventorySystem.Instance.Canonicalize(x);
    }

    private bool IsSameItem(ItemSO a, ItemSO b)
    {
        if (a == null || b == null) return false;
        if (a == b) return true;          // misma referencia (ideal)
        return Key(a) == Key(b);          // fallback por nombre normalizado
    }

    private void OnIngredienteCorrecto(ItemSO item)
    {
        bool consumed = false;

        // Consume del inventario principal (nuevo)
        if (InventorySystem.Instance != null)
            consumed = InventorySystem.Instance.ConsumeMappedItem(item, 1);

        // Fallback al inventario viejo
        if (!consumed && InventoryManager.instancia != null)
            InventoryManager.instancia.RemoveItem(item, 1);

        ingredientesAgregados.Add(item);
        step++;

        if (textoResultado != null)
            textoResultado.text = $"Correcto! Paso {step}/3";

        GuardarEstado();
        ActualizarUI();

        if (step >= 3)
            FinalizarPocion();
    }

    private void FinalizarPocion()
    {
        float precioFinal = CalcularPrecioFinal();

        if (textoResultado != null)
        {
            string textoDescuento = descuentoPrecio > 0 ? $"\n(Descuento: {descuentoPrecio * 100}%)" : "";
            textoResultado.text = $"Pocion creada: {potion.pocionNombre}{textoDescuento}";
        }

        if (InventoryManager.instancia != null && potion != null && potion.itemPocion != null)
            InventoryManager.instancia.AddItem(potion.itemPocion, 1);

        LimpiarEstado();
    }

    private void OnIngredienteIncorrecto(ItemSO drop, ItemSO esperado)
    {
        vecesEquivocado++;
        GuardarEstado();

        if (textoResultado != null)
            textoResultado.text = $"Incorrecto! (Error {vecesEquivocado})\nCargando minijuego...";

        if (debugLogs)
        {
            Debug.LogWarning($"[CALDERO FAIL] step={step} fails={vecesEquivocado}");
            Debug.LogWarning($"  esperado: {Ref(esperado)}");
            Debug.LogWarning($"  drop    : {Ref(drop)}");
        }

        SceneManager.LoadScene(nombreEscenaMinijuego);
    }

    private void ActualizarUI()
    {
        if (contenedorIngredientes == null || iconoIngredientePrefab == null) return;

        foreach (Transform child in contenedorIngredientes)
            Destroy(child.gameObject);

        foreach (var ing in ingredientesAgregados)
        {
            var icono = Instantiate(iconoIngredientePrefab, contenedorIngredientes);
            var img = icono.GetComponent<Image>();
            if (img != null) img.sprite = ing.icon;
        }
    }

    private void CargarPocionSeleccionada()
    {
        string nombrePocion = PlayerPrefs.GetString("PocionSeleccionada", "");
        if (string.IsNullOrEmpty(nombrePocion))
        {
            Debug.LogError("[CALDERO] No hay 'PocionSeleccionada' en PlayerPrefs!");
            return;
        }

        potion = todasLasPociones.Find(p => p.pocionNombre == nombrePocion);
        if (potion == null)
            Debug.LogError($"[CALDERO] No se encontro la pocion '{nombrePocion}' en 'todasLasPociones'.");
    }

    private void GuardarEstado()
    {
        PlayerPrefs.SetInt(K_STEP, step);
        PlayerPrefs.SetInt(K_FAILS, vecesEquivocado);
        PlayerPrefs.SetFloat(K_DISC, descuentoPrecio);
        PlayerPrefs.SetString(K_POCION, potion != null ? potion.pocionNombre : "");
        PlayerPrefs.Save();
    }

    private void RestaurarEstadoSiAplica()
    {
        if (potion == null) return;

        string p = PlayerPrefs.GetString(K_POCION, "");
        if (p != potion.pocionNombre)
        {
            LimpiarEstado();
            LimpiarEstadoLocal();
            return;
        }

        step = Mathf.Clamp(PlayerPrefs.GetInt(K_STEP, 0), 0, 3);
        vecesEquivocado = Mathf.Max(0, PlayerPrefs.GetInt(K_FAILS, 0));
        descuentoPrecio = Mathf.Max(0f, PlayerPrefs.GetFloat(K_DISC, 0f));
    }

    private void ReconstruirListaIngredientesPorStep()
    {
        ingredientesAgregados.Clear();
        if (potion == null) return;

        if (step >= 1) ingredientesAgregados.Add(Canon(potion.suero));
        if (step >= 2) ingredientesAgregados.Add(Canon(potion.ingrediente1));
        if (step >= 3) ingredientesAgregados.Add(Canon(potion.ingrediente2));
    }

    private void LimpiarEstadoLocal()
    {
        step = 0;
        vecesEquivocado = 0;
        descuentoPrecio = 0f;
        ingredientesAgregados.Clear();
    }

    private void LimpiarEstado()
    {
        PlayerPrefs.DeleteKey(K_STEP);
        PlayerPrefs.DeleteKey(K_FAILS);
        PlayerPrefs.DeleteKey(K_DISC);
        PlayerPrefs.DeleteKey(K_POCION);
        PlayerPrefs.Save();
    }

    private float CalcularPrecioFinal()
    {
        if (potion == null) return 0f;
        float precioFinal = potion.precioBase * (1f - descuentoPrecio);
        potion.precioFinal = precioFinal;
        return precioFinal;
    }

    // ===== DEBUG HELPERS =====

    private void DebugDumpReceta()
    {
        if (!debugLogs || potion == null) return;

        Debug.Log("[CALDERO RECETA]");
        Debug.Log("  suero       : " + Ref(potion.suero));
        Debug.Log("  ingrediente1: " + Ref(potion.ingrediente1));
        Debug.Log("  ingrediente2: " + Ref(potion.ingrediente2));
        Debug.Log("  itemPocion   : " + (potion.itemPocion ? potion.itemPocion.name : "NULL"));
    }

    private string Ref(ItemSO x)
    {
        if (x == null) return "NULL";

        string baseInfo = $"{x.name} | id={x.GetInstanceID()}";
        if (!debugAssetPaths) return baseInfo;

#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(x);
        return $"{baseInfo} | path={path}";
#else
        return baseInfo;
#endif
    }
}
