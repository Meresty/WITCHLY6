using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement; 

public class CalderoLogic : MonoBehaviour
{
    public static CalderoLogic instancia;
    public NotaUI notaUI;

    private void Awake()
    {
        instancia = this;
    }


    public PocionSO potion;
    public InventoryUI inventory;
    public string nombreEscenaMinijuego = "Presicion";


    public Transform contenedorIngredientes;
    public GameObject iconoIngredientePrefab;
    public Image iconoPocionResultado;
    public TextMeshProUGUI textoResultado;

    private GameObject panelBloqueo; 
    private TextMeshProUGUI txtClickParaSalir;

    private int step = 0;
    private int errores = 0;
    private List<ItemSO> ingredientesAgregados = new List<ItemSO>();

    void Start()
    {
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
            }

            ActualizarUI();
        }
    }

    void ProcesarResultadoMinijuego()
    {
        bool exito = PlayerPrefs.GetInt("MinijuegoExito") == 1;
        step = PlayerPrefs.GetInt("CalderoStep", 0);
        errores = PlayerPrefs.GetInt("CalderoErrores", 0);

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
            Debug.Log("¡Minijuego exitoso! Intenta de nuevo");
            if (textoResultado != null)
                textoResultado.text = "¡Salvado! Intenta de nuevo";
        }
        else
        {
            errores++;
            if (errores >= 3)
            {
                Debug.Log("Perdiste todos los ingredientes");
                if (textoResultado != null)
                    textoResultado.text = "¡Perdiste los ingredientes!";

                foreach (var ing in ingredientesAgregados)
                    inventory.RemoveItem(ing);

                ReiniciarCaldero();
            }
            else
            {
                Debug.Log($"Error {errores}/3, pero puedes seguir");
                if (textoResultado != null)
                    textoResultado.text = $"Error {errores}/3. ¡Cuidado!";
            }
        }

        PlayerPrefs.DeleteKey("MinijuegoExito");
        PlayerPrefs.DeleteKey("CalderoStep");
        PlayerPrefs.DeleteKey("CalderoErrores");
        PlayerPrefs.DeleteKey("PocionActual");
        PlayerPrefs.DeleteKey("IngredientesAgregados");
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
        inventory.RemoveItem(item);
        ingredientesAgregados.Add(item);
        step++;

        Debug.Log("Ingrediente correcto");

        if (textoResultado != null)
            textoResultado.text = $"¡Correcto! Paso {step}/3";

        ActualizarUI();

        if (step >= 3)
        {
            Debug.Log("¡Poción completada!");
            if (textoResultado != null)
                textoResultado.text = $"¡Poción creada: {potion.pocionNombre}!";

            // TODO: agregar la poción al inventario
        }
    }

    void OnIngredienteIncorrecto()
    {
        Debug.Log("Ingrediente incorrecto = cargar minijuego");

        if (textoResultado != null)
            textoResultado.text = "¡Incorrecto! Cargando minijuego...";

        PlayerPrefs.SetInt("CalderoStep", step);
        PlayerPrefs.SetInt("CalderoErrores", errores);
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
        errores = 0;
        ingredientesAgregados.Clear();
        ActualizarUI();

        if (textoResultado != null && potion != null)
            textoResultado.text = $"Creando: {potion.pocionNombre}";

        if (notaUI != null)
            notaUI.LimpiarNota();
    }
}