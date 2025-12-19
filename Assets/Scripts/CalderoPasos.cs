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

    [Header("Configuración")]
    public string nombreEscenaMinijuego = "Presicion";
    public Transform contenedorIngredientes;
    public GameObject iconoIngredientePrefab;
    public Image iconoPocionResultado;
    public TextMeshProUGUI textoResultado;
    public Button Btn_back;

    private PocionSO potion;
    private int step = 0;
    private int vecesEquivocado = 0; // Cuenta cuántas veces eligió ingrediente incorrecto
    private float descuentoPrecio = 0f; // Descuento acumulado
    private List<ItemSO> ingredientesAgregados = new List<ItemSO>();


    [Header("Bloqueo en Fallo")]
    [SerializeField] private GameObject panelBloqueo;
    [SerializeField] private TextMeshProUGUI txtClickParaSalir;
    [SerializeField] private string nombreEscenaInicial = "PantallaInicial";

    void Start()
    {
        if (InventoryManager.instancia == null)
        {
            Debug.LogError("⚠️ Carga desde pantalla inicial para que cargue el InventoryManager");
        }

        CargarPocionSeleccionada();

        if (PlayerPrefs.HasKey("MinijuegoExito"))
        {
            ProcesarResultadoMinijuego();
        }
        else
        {
            if (potion != null)
            {
                if (iconoPocionResultado != null)
                    iconoPocionResultado.sprite = potion.icon;

                if (textoResultado != null)
                    textoResultado.text = $"Creando: {potion.pocionNombre}";

                if (notaUI != null)
                    notaUI.MostrarReceta(potion);

                Debug.Log($"📝 Receta para: {potion.pocionNombre}");
            }
            else
            {
                Debug.LogError("⚠️ (caldero) No carga poción");
            }

            ActualizarUI();
        }
    }

    void ProcesarResultadoMinijuego()
    {
        CargarPocionSeleccionada();

        bool exito = PlayerPrefs.GetInt("MinijuegoExito") == 1;
        step = PlayerPrefs.GetInt("CalderoStep", 0);
        vecesEquivocado = PlayerPrefs.GetInt("VecesEquivocado", 0);
        descuentoPrecio = PlayerPrefs.GetFloat("DescuentoPrecio", 0f);

        // ✅ NUEVO: Verificar si los ingredientes ya fueron eliminados
        bool yaEliminados = PlayerPrefs.GetInt("IngredientesYaEliminados", 0) == 1;

        string ingredientesGuardados = PlayerPrefs.GetString("IngredientesAgregados", "");
        if (!string.IsNullOrEmpty(ingredientesGuardados))
        {
            string[] nombres = ingredientesGuardados.Split(',');
            foreach (string nombre in nombres)
            {
                if (potion.suero != null && potion.suero.itemNombre == nombre)
                    ingredientesAgregados.Add(potion.suero);
                else if (potion.ingrediente1 != null && potion.ingrediente1.itemNombre == nombre)
                    ingredientesAgregados.Add(potion.ingrediente1);
                else if (potion.ingrediente2 != null && potion.ingrediente2.itemNombre == nombre)
                    ingredientesAgregados.Add(potion.ingrediente2);
            }
        }

        if (exito)
        {
            descuentoPrecio += 0.20f;

            Debug.Log($"✅ ¡Minijuego salvado! Usaste oportunidad {vecesEquivocado}/2. Descuento: {descuentoPrecio * 100}%");

            if (textoResultado != null)
            {
                textoResultado.text = $"¡Salvado! Puedes intentar de nuevo\n(Descuento: {descuentoPrecio * 100}%)";
            }
        }
        else
        {
            Debug.Log("❌ Fallaste el minijuego. Perdiste todos los ingredientes");

            if (textoResultado != null)
                textoResultado.text = "¡Fallaste el minijuego!\nIngredientes perdidos :(";


            if (!yaEliminados && InventoryManager.instancia != null)
            {
                foreach (var ingred in ingredientesAgregados)
                {
                    InventoryManager.instancia.RemoveItem(ingred, 1);
                    Debug.Log($"❌ Ingrediente '{ingred.itemNombre}' eliminado por fallo en minijuego");
                }
            }
            else if (yaEliminados)
            {
                Debug.Log("ℹ️ Ingredientes ya fueron eliminados previamente (3 fallos)");
            }

            StartCoroutine(BloquearPantallaYEsperarClick());
        }


        PlayerPrefs.DeleteKey("MinijuegoExito");
        PlayerPrefs.DeleteKey("CalderoStep");
        PlayerPrefs.DeleteKey("PocionActual");
        PlayerPrefs.DeleteKey("IngredientesAgregados");
        PlayerPrefs.DeleteKey("VecesEquivocado");
        PlayerPrefs.DeleteKey("DescuentoPrecio");
        PlayerPrefs.DeleteKey("IngredientesYaEliminados"); 
        PlayerPrefs.Save();

        if (notaUI != null && potion != null)
            notaUI.MostrarReceta(potion);

        ActualizarUI();
    }

    public void AddIngredient(ItemSO item)
    {
        if (step >= 3)
        {
            if (textoResultado != null)
                textoResultado.text = "¡Poción ya completada!";
            return;
        }

        ItemSO esperado = null;
        if (step == 0) esperado = potion.suero;
        if (step == 1) esperado = potion.ingrediente1;
        if (step == 2) esperado = potion.ingrediente2;

        if (item == esperado)
        {
            OnIngredienteCorrecto(item);
        }
        else
        {
            OnIngredienteIncorrecto();
        }
    }

    void OnIngredienteCorrecto(ItemSO item)
    {
        if (InventoryManager.instancia != null)
        {
            InventoryManager.instancia.RemoveItem(item);
        }

        ingredientesAgregados.Add(item);
        step++;

        Debug.Log("✅ Ingrediente correcto");

        if (textoResultado != null)
            textoResultado.text = $"¡Correcto! Paso {step}/3";

        ActualizarUI();

        if (step >= 3)
        {

            float precioFinal = CalcularPrecioFinal();

            if (textoResultado != null)
            {
                string textoDescuento = descuentoPrecio > 0
                    ? $"\n(Descuento aplicado: {descuentoPrecio * 100}%)"
                    : "";
                textoResultado.text = $"¡Poción creada: {potion.pocionNombre}!{textoDescuento}";
            }


            if (InventoryManager.instancia != null && potion != null && potion.itemPocion != null)
            {
                InventoryManager.instancia.AddItem(potion.itemPocion, 1);
            }
        
        }
    }

    void OnIngredienteIncorrecto()
    {
        vecesEquivocado++;

        if (vecesEquivocado >= 3)
        {
            Debug.Log("Te equivocaste 3 veces. ¡Pierdes ingredientes sin minijuego!");

            // ⚠️ CAMBIO: Solo eliminamos ingredientes AQUÍ (no en minijuego)
            if (InventoryManager.instancia != null)
            {
                foreach (var ing in ingredientesAgregados)
                {
                    InventoryManager.instancia.RemoveItem(ing, 1);
                    Debug.Log($"❌ Ingrediente '{ing.itemNombre}' eliminado por 3 fallos");
                }
            }

            if (textoResultado != null)
                textoResultado.text = "¡3 errores!\nIngredientes perdidos sin oportunidad";

            // ✅ NUEVO: Marcamos que ya se eliminaron los ingredientes
            PlayerPrefs.SetInt("IngredientesYaEliminados", 1);
            PlayerPrefs.Save();

            StartCoroutine(BloquearPantallaYEsperarClick());
            return;
        }

        Debug.Log($"❌ Ingrediente incorrecto (Error #{vecesEquivocado}/2) → Cargando minijuego");

        if (textoResultado != null)
            textoResultado.text = $"¡Incorrecto! (Error {vecesEquivocado}/2)\nCargando minijuego...";

        // Guardar estado
        PlayerPrefs.SetInt("CalderoStep", step);
        PlayerPrefs.SetInt("VecesEquivocado", vecesEquivocado);
        PlayerPrefs.SetFloat("DescuentoPrecio", descuentoPrecio);
        PlayerPrefs.SetString("PocionActual", potion.pocionNombre);

        string ingredientesStr = "";
        foreach (var ing in ingredientesAgregados)
        {
            if (ingredientesStr != "") ingredientesStr += ",";
            ingredientesStr += ing.itemNombre;
        }
        PlayerPrefs.SetString("IngredientesAgregados", ingredientesStr);

        PlayerPrefs.Save();

        SceneManager.LoadScene(nombreEscenaMinijuego);
    }

    void ActualizarUI()
    {
        if (contenedorIngredientes == null || iconoIngredientePrefab == null)
            return;

        foreach (Transform child in contenedorIngredientes)
            Destroy(child.gameObject);

        foreach (var ing in ingredientesAgregados)
        {
            var icono = Instantiate(iconoIngredientePrefab, contenedorIngredientes);
            Image img = icono.GetComponent<Image>();
            if (img != null)
                img.sprite = ing.icon;
        }
    }

    public void ReiniciarCaldero()
    {
        step = 0;
        vecesEquivocado = 0;
        descuentoPrecio = 0f;
        ingredientesAgregados.Clear();
        ActualizarUI();

        if (textoResultado != null && potion != null)
            textoResultado.text = $"Creando: {potion.pocionNombre}";

        if (notaUI != null)
            notaUI.LimpiarNota();
    }

    void CargarPocionSeleccionada()
    {
        string nombrePocion = PlayerPrefs.GetString("PocionSeleccionada", "");

        if (string.IsNullOrEmpty(nombrePocion))
        {
            Debug.LogError("⚠️ [CALDERO] No hay poción seleccionada en PlayerPrefs!");
            return;
        }

        potion = todasLasPociones.Find(p => p.pocionNombre == nombrePocion);

        if (potion == null)
        {
            Debug.LogError($"⚠️ [CALDERO] No se encontró la poción '{nombrePocion}' en la lista!");
        }
        else
        {
            Debug.Log($"✅ [CALDERO] Poción cargada: {potion.pocionNombre}");
        }
    }

    float CalcularPrecioFinal()
    {
        if (potion == null) return 0f;

        float precioBase = potion.precioBase;
        float precioFinal = precioBase * (1f - descuentoPrecio);

        // Guardar el precio final en la poción (si lo necesitas después)
        potion.precioFinal = precioFinal;

        return precioFinal;
    }








    IEnumerator BloquearPantallaYEsperarClick()
    {
        yield return new WaitForSecondsRealtime(2f);

        if (panelBloqueo != null)
            panelBloqueo.SetActive(true);
        if (txtClickParaSalir != null)
        {
            txtClickParaSalir.text = "Click para volver al menú";
            StartCoroutine(Parpadear(txtClickParaSalir));

        }

    
        bool esperandoClick = true;
        while (esperandoClick)
        {
            if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
            {
                esperandoClick = false;
            }
            yield return null;
        }

        ReiniciarCaldero();
        RegresarAEscenaInicial();
    }


    IEnumerator Parpadear(TextMeshProUGUI texto)
    {
        if (texto == null) yield break;

        float duracionCiclo = 3f;

        while (texto.gameObject.activeSelf)
        {

            float tiempo = 0;
            while (tiempo < duracionCiclo / 2)
            {
                tiempo += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0.3f, tiempo / (duracionCiclo / 2));
                texto.color = new Color(texto.color.r, texto.color.g, texto.color.b, alpha);
                yield return null;
            }

            tiempo = 0;
            while (tiempo < duracionCiclo / 2)
            {
                tiempo += Time.deltaTime;
                float alpha = Mathf.Lerp(0.3f, 1f, tiempo / (duracionCiclo / 2));
                texto.color = new Color(texto.color.r, texto.color.g, texto.color.b, alpha);
                yield return null;
            }
        }
    }




    void RegresarAEscenaInicial()
    {
        Debug.Log("Regresando a escena inicial por 3 fallos");

        PlayerPrefs.DeleteKey("MinijuegoExito");
        PlayerPrefs.DeleteKey("CalderoStep");
        PlayerPrefs.DeleteKey("CalderoErrores");
        PlayerPrefs.DeleteKey("PocionActual");
        PlayerPrefs.DeleteKey("IngredientesAgregados");
        PlayerPrefs.Save();

        SceneManager.LoadScene(nombreEscenaInicial);
    }
}