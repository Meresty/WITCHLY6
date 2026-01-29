using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CalderoLogic : MonoBehaviour
{
    public static CalderoLogic instancia;
    public NotaUI notaUI;

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

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        instancia = this;
    }

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

    //public void ForceIncorrectDrop() => OnIngredienteIncorrecto();

    public void AddIngredient(ItemSO item)
    {
        //if (potion == null) return;
        
        if (step >= 3)
        {
            Debug.Log("Entra:");
            if (textoResultado != null) textoResultado.text = "Pocion ya completada";
            return;
        }
        
        ItemSO esperado = GetEsperado(step);
        Debug.Log("Esperado: " + esperado);
        Debug.Log("Seleccionado: " + item);
        // DEBUG: te va a decir por que no esta matcheando
        //Debug.Log($"[CALDERO] step={step} drop='{Key(item)}'({item?.GetInstanceID()}) esperado='{Key(esperado)}'({esperado?.GetInstanceID()})");

        if (IsSameItem(item, esperado))
            OnIngredienteCorrecto(item);
        else
            OnIngredienteIncorrecto();
    }

    private ItemSO GetEsperado(int s)
    {
        if (potion == null) return null;
        if (s == 0) return potion.suero;
        if (s == 1) return potion.ingrediente1;
        if (s == 2) return potion.ingrediente2;
        return null;
    }

    // Normaliza nombre (por si hay "(Clone)" o espacios)
    private string Key(ItemSO x)
    {
        if (x == null) return "";
        return x.name.Replace("(Clone)", "").Trim().ToLowerInvariant();
    }

    private bool IsSameItem(ItemSO a, ItemSO b)
    {
        if (a == null || b == null) return false;
        if (a == b) return true;
        return Key(a) == Key(b);
    }

    void OnIngredienteCorrecto(ItemSO item)
    {
        bool consumed = false;

        if (InventorySystem.Instance != null)
            consumed = InventorySystem.Instance.ConsumeMappedItem(item, 1);

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
}
