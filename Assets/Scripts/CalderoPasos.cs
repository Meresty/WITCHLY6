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
    [SerializeField] private string nombreEscenaInicial = "PantallaInicial";

    [Header("UI")]
    public Transform contenedorIngredientes;
    public GameObject iconoIngredientePrefab;
    public Image iconoPocionResultado;
    public TextMeshProUGUI textoResultado;

    [Header("Bloqueo en Fallo (opcional)")]
    [SerializeField] private GameObject panelBloqueo;
    [SerializeField] private TextMeshProUGUI txtClickParaSalir;

    private PocionSO potion;
    private int step = 0;
    private int vecesEquivocado = 0;   // cuantas veces fallo antes de ir a minijuego
    private float descuentoPrecio = 0f;
    private List<ItemSO> ingredientesAgregados = new List<ItemSO>();

    void Start()
    {
        CargarPocionSeleccionada();

        // Si venimos de Presicion, aqui se procesa
        if (PlayerPrefs.HasKey("MinijuegoExito"))
        {
            ProcesarResultadoMinijuego();
            return;
        }

        PrepararUIInicial();
        ActualizarUI();
    }

    // -----------------------------
    // PUBLIC API (para DropZone)
    // -----------------------------
    public void AddIngredient(ItemSO item)
    {
        if (potion == null) return;

        // Si llega null (ej: semilla o item sin mapping) => incorrecto directo
        if (item == null)
        {
            ForceIncorrectDrop();
            return;
        }

        if (step >= 3)
        {
            if (textoResultado != null) textoResultado.text = "¡Pocion ya completada!";
            return;
        }

        ItemSO esperado = GetEsperadoPorPaso(step);

        // si es correcto (mismo item y mismo orden)
        if (item == esperado)
        {
            OnIngredienteCorrecto(item);
        }
        else
        {
            OnIngredienteIncorrecto();
        }
    }

    // Compatibilidad: algunos scripts llaman esto
    public void ForceIncorrectDrop()
    {
        OnIngredienteIncorrecto();
    }

    // -----------------------------
    // LOGICA DE CORRECTO / INCORRECTO
    // -----------------------------
    private void OnIngredienteCorrecto(ItemSO item)
    {
        // Descontar del inventario real
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.ConsumeMappedItem(item, 1);
        }
        else if (InventoryManager.instancia != null)
        {
            InventoryManager.instancia.RemoveItem(item, 1);
        }

        ingredientesAgregados.Add(item);
        step++;

        if (textoResultado != null) textoResultado.text = $"¡Correcto! Paso {step}/3";

        ActualizarUI();

        if (step >= 3)
        {
            float precioFinal = CalcularPrecioFinal();

            if (textoResultado != null)
            {
                string textoDescuento = descuentoPrecio > 0 ? $"\n(Descuento aplicado: {descuentoPrecio * 100}%)" : "";
                textoResultado.text = $"¡Pocion creada: {potion.pocionNombre}!{textoDescuento}";
            }

            // Agregar la pocion al inventario del caldero
            if (InventoryManager.instancia != null && potion.itemPocion != null)
            {
                InventoryManager.instancia.AddItem(potion.itemPocion, 1);
            }

            // Limpia estado de caldero (por si vuelves a dropear)
            ClearSavedRunState();
        }
    }

    private void OnIngredienteIncorrecto()
    {
        vecesEquivocado++;

        GuardarEstadoParaMinijuego();

        if (textoResultado != null)
            textoResultado.text = $"¡Incorrecto! (Error {vecesEquivocado}/2)\nCargando minijuego...";

        SceneManager.LoadScene(nombreEscenaMinijuego);
    }

    // -----------------------------
    // MINIJUEGO RESULTADO
    // -----------------------------
    private void ProcesarResultadoMinijuego()
    {
        // Recargar la pocion
        CargarPocionSeleccionada();

        bool exito = PlayerPrefs.GetInt("MinijuegoExito", 0) == 1;

        step = PlayerPrefs.GetInt("CalderoStep", 0);
        vecesEquivocado = PlayerPrefs.GetInt("VecesEquivocado", 0);
        descuentoPrecio = PlayerPrefs.GetFloat("DescuentoPrecio", 0f);

        ingredientesAgregados.Clear();
        CargarIngredientesAgregadosDePrefs();

        PrepararUIInicial();

        if (exito)
        {
            // ejemplo: descuento acumulable
            descuentoPrecio += 0.20f;

            if (textoResultado != null)
                textoResultado.text = $"¡Salvado! Puedes intentar de nuevo\n(Descuento: {descuentoPrecio * 100}%)";
        }
        else
        {
            if (textoResultado != null)
                textoResultado.text = "¡Fallaste el minijuego!\nIngredientes perdidos :(";

            // Si fallo el minijuego, se pierden los ingredientes ya agregados
            if (InventorySystem.Instance != null)
            {
                foreach (var ing in ingredientesAgregados)
                    InventorySystem.Instance.ConsumeMappedItem(ing, 1);
            }
            else if (InventoryManager.instancia != null)
            {
                foreach (var ing in ingredientesAgregados)
                    InventoryManager.instancia.RemoveItem(ing, 1);
            }

            // Opcional: bloquear pantalla y regresar
            if (panelBloqueo != null)
                StartCoroutine(BloquearPantallaYEsperarClick());
        }

        // Limpia la señal del minijuego, pero conserva step/errores/descuento por si sigues intentando
        PlayerPrefs.DeleteKey("MinijuegoExito");
        PlayerPrefs.Save();

        ActualizarUI();
    }

    private void GuardarEstadoParaMinijuego()
    {
        PlayerPrefs.SetInt("CalderoStep", step);
        PlayerPrefs.SetInt("VecesEquivocado", vecesEquivocado);
        PlayerPrefs.SetFloat("DescuentoPrecio", descuentoPrecio);
        PlayerPrefs.SetString("PocionActual", potion != null ? potion.pocionNombre : "");

        // Guardar ingredientes agregados
        string ingredientesStr = "";
        foreach (var ing in ingredientesAgregados)
        {
            if (ing == null) continue;
            if (ingredientesStr != "") ingredientesStr += ",";
            ingredientesStr += ing.itemNombre;
        }
        PlayerPrefs.SetString("IngredientesAgregados", ingredientesStr);

        PlayerPrefs.Save();
    }

    private void CargarIngredientesAgregadosDePrefs()
    {
        if (potion == null) return;

        string ingredientesGuardados = PlayerPrefs.GetString("IngredientesAgregados", "");
        if (string.IsNullOrEmpty(ingredientesGuardados)) return;

        string[] nombres = ingredientesGuardados.Split(',');
        foreach (string nombre in nombres)
        {
            if (string.IsNullOrEmpty(nombre)) continue;

            // Importante: buscamos dentro de la receta actual para recuperar referencias ItemSO
            if (potion.suero != null && potion.suero.itemNombre == nombre) ingredientesAgregados.Add(potion.suero);
            else if (potion.ingrediente1 != null && potion.ingrediente1.itemNombre == nombre) ingredientesAgregados.Add(potion.ingrediente1);
            else if (potion.ingrediente2 != null && potion.ingrediente2.itemNombre == nombre) ingredientesAgregados.Add(potion.ingrediente2);
        }
    }

    private void ClearSavedRunState()
    {
        PlayerPrefs.DeleteKey("CalderoStep");
        PlayerPrefs.DeleteKey("VecesEquivocado");
        PlayerPrefs.DeleteKey("DescuentoPrecio");
        PlayerPrefs.DeleteKey("PocionActual");
        PlayerPrefs.DeleteKey("IngredientesAgregados");
        PlayerPrefs.Save();
    }

    // -----------------------------
    // UI
    // -----------------------------
    private void PrepararUIInicial()
    {
        if (potion != null)
        {
            if (iconoPocionResultado != null) iconoPocionResultado.sprite = potion.icon;
            if (notaUI != null) notaUI.MostrarReceta(potion);

            // si no viene del minijuego, mostramos "Creando"
            if (!PlayerPrefs.HasKey("MinijuegoExito"))
            {
                if (textoResultado != null) textoResultado.text = $"Creando: {potion.pocionNombre}";
            }
        }
    }

    private void ActualizarUI()
    {
        if (contenedorIngredientes == null || iconoIngredientePrefab == null) return;

        foreach (Transform child in contenedorIngredientes)
            Destroy(child.gameObject);

        foreach (var ing in ingredientesAgregados)
        {
            if (ing == null) continue;

            var icono = Instantiate(iconoIngredientePrefab, contenedorIngredientes);
            Image img = icono.GetComponent<Image>();
            if (img != null) img.sprite = ing.icon;
        }
    }

    // -----------------------------
    // HELPERS
    // -----------------------------
    private ItemSO GetEsperadoPorPaso(int paso)
    {
        if (potion == null) return null;

        if (paso == 0) return potion.suero;
        if (paso == 1) return potion.ingrediente1;
        if (paso == 2) return potion.ingrediente2;
        return null;
    }

    private void CargarPocionSeleccionada()
    {
        string nombrePocion = PlayerPrefs.GetString("PocionSeleccionada", "");
        if (string.IsNullOrEmpty(nombrePocion))
        {
            Debug.LogError("[CALDERO] No hay pocion seleccionada en PlayerPrefs!");
            potion = null;
            return;
        }

        potion = todasLasPociones.Find(p => p.pocionNombre == nombrePocion);

        if (potion == null)
            Debug.LogError($"[CALDERO] No se encontro la pocion '{nombrePocion}' en la lista!");
    }

    private float CalcularPrecioFinal()
    {
        if (potion == null) return 0f;
        float precioFinal = potion.precioBase * (1f - descuentoPrecio);
        potion.precioFinal = precioFinal;
        return precioFinal;
    }

    // -----------------------------
    // BLOQUEO (opcional)
    // -----------------------------
    private IEnumerator BloquearPantallaYEsperarClick()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        if (panelBloqueo != null) panelBloqueo.SetActive(true);
        if (txtClickParaSalir != null) txtClickParaSalir.text = "Click para volver al menu";

        bool esperandoClick = true;
        while (esperandoClick)
        {
            if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
                esperandoClick = false;
            yield return null;
        }

        // Limpia estado y vuelve
        ClearSavedRunState();
        SceneManager.LoadScene(nombreEscenaInicial);
    }
}
