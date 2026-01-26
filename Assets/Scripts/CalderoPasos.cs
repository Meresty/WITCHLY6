using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CalderoLogic : MonoBehaviour
{
    public static CalderoLogic instancia;
    public NotaUI notaUI;

    private void Awake()
    {
        instancia = this;
    }

    [Header("Pociones disponibles")]
    public List<PocionSO> todasLasPociones = new List<PocionSO>();

    [Header("Configuracion")]
    public string nombreEscenaMinijuego = "Presicion";
    public Transform contenedorIngredientes;
    public GameObject iconoIngredientePrefab;
    public Image iconoPocionResultado;
    public TextMeshProUGUI textoResultado;

    private PocionSO potion;
    private int step = 0;
    private int vecesEquivocado = 0;
    private float descuentoPrecio = 0f;

    private readonly List<ItemSO> ingredientesAgregados = new List<ItemSO>();

    [Header("Bloqueo en Fallo")]
    [SerializeField] private GameObject panelBloqueo;
    [SerializeField] private TextMeshProUGUI txtClickParaSalir;
    [SerializeField] private string nombreEscenaInicial = "PantallaInicial";

    void Start()
    {
        CargarPocionSeleccionada();

        if (potion != null)
        {
            if (iconoPocionResultado != null) iconoPocionResultado.sprite = potion.icon;
            if (textoResultado != null) textoResultado.text = $"Creando: {potion.pocionNombre}";
            if (notaUI != null) notaUI.MostrarReceta(potion);
        }

        ActualizarUI();
    }

    // Para drops sin mapping (ej: semilla)
    public void ForceIncorrectDrop()
    {
        OnIngredienteIncorrecto();
    }

    // Esta la llama la zona roja
    public void AddIngredient(ItemSO item)
    {
        if (potion == null) return;

        if (step >= 3)
        {
            if (textoResultado != null) textoResultado.text = "Pocion ya completada";
            return;
        }

        ItemSO esperado = null;
        if (step == 0) esperado = potion.suero;
        if (step == 1) esperado = potion.ingrediente1;
        if (step == 2) esperado = potion.ingrediente2;

        // Permite cualquier item:
        // si no es el esperado (o esta fuera de orden) => minijuego
        if (item == esperado)
            OnIngredienteCorrecto(item);
        else
            OnIngredienteIncorrecto();
    }

    void OnIngredienteCorrecto(ItemSO item)
    {
        // Descarga del inventario real
        bool consumed = false;

        if (InventorySystem.Instance != null)
            consumed = InventorySystem.Instance.ConsumeMappedItem(item, 1);

        // Fallback por si no existe InventorySystem
        if (!consumed && InventoryManager.instancia != null)
            InventoryManager.instancia.RemoveItem(item, 1);

        ingredientesAgregados.Add(item);
        step++;

        if (textoResultado != null)
            textoResultado.text = $"Correcto! Paso {step}/3";

        ActualizarUI();

        if (step >= 3)
        {
            float precioFinal = CalcularPrecioFinal();

            if (textoResultado != null)
            {
                string textoDescuento = descuentoPrecio > 0 ? $"\n(Descuento: {descuentoPrecio * 100}%)" : "";
                textoResultado.text = $"Pocion creada: {potion.pocionNombre}{textoDescuento}";
            }

            // agrega pocion al inventario del caldero
            if (InventoryManager.instancia != null && potion != null && potion.itemPocion != null)
                InventoryManager.instancia.AddItem(potion.itemPocion, 1);
        }
    }

    void OnIngredienteIncorrecto()
    {
        vecesEquivocado++;

        PlayerPrefs.SetInt("CalderoStep", step);
        PlayerPrefs.SetInt("VecesEquivocado", vecesEquivocado);
        PlayerPrefs.SetFloat("DescuentoPrecio", descuentoPrecio);
        PlayerPrefs.SetString("PocionActual", potion != null ? potion.pocionNombre : "");
        PlayerPrefs.Save();

        if (textoResultado != null)
            textoResultado.text = $"Incorrecto! (Error {vecesEquivocado})\nCargando minijuego...";

        SceneManager.LoadScene(nombreEscenaMinijuego);
    }

    void ActualizarUI()
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

    void CargarPocionSeleccionada()
    {
        string nombrePocion = PlayerPrefs.GetString("PocionSeleccionada", "");
        if (string.IsNullOrEmpty(nombrePocion))
        {
            Debug.LogError("[CALDERO] No hay pocion seleccionada en PlayerPrefs!");
            return;
        }

        potion = todasLasPociones.Find(p => p.pocionNombre == nombrePocion);
        if (potion == null)
            Debug.LogError($"[CALDERO] No se encontro la pocion '{nombrePocion}' en la lista!");
    }

    float CalcularPrecioFinal()
    {
        if (potion == null) return 0f;
        float precioFinal = potion.precioBase * (1f - descuentoPrecio);
        potion.precioFinal = precioFinal;
        return precioFinal;
    }

    // Si tu UI usa bloqueo, lo puedes reactivar aqui (opcional)
    IEnumerator BloquearPantallaYEsperarClick()
    {
        if (panelBloqueo != null) panelBloqueo.SetActive(true);
        if (txtClickParaSalir != null) txtClickParaSalir.gameObject.SetActive(true);

        bool clicked = false;
        while (!clicked)
        {
            if (Input.GetMouseButtonDown(0)) clicked = true;
            yield return null;
        }

        SceneManager.LoadScene(nombreEscenaInicial);
    }
}
