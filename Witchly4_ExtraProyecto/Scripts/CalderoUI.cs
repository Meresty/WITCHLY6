using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CalderoUI : MonoBehaviour
{
    public static CalderoUI instancia;

    private void Awake()
    {
        instancia = this;
    }

    [System.Serializable]
    public class Receta
    {
        public string nombre;
        public List<ItemSO> ingredientes;   // EXACTAMENTE 3
        public PocionSO pocionResultado;
    }

    public List<Receta> recetas;

    public List<ItemSO> ingredientesActuales = new List<ItemSO>();

    public InventoryUI inventario; // asigna tu InventoryUI en el inspector

    public Transform contenedorIngredientes;
    public GameObject iconoIngredientePrefab;

    public Image iconoPocionResultado;
    public TextMeshProUGUI textoResultado;


    // -------------------------
    // AÑADIR INGREDIENTE
    // -------------------------
    public void AddIngredient(ItemSO item)
    {
        if (ingredientesActuales.Count >= 3)
        {
            Debug.Log("Caldero lleno (máx 3 ingredientes)");
            return;
        }

        ingredientesActuales.Add(item);
        ActualizarUI();

        if (ingredientesActuales.Count == 3)
            VerificarReceta();
    }


    // -------------------------
    // ACTUALIZAR UI
    // -------------------------
    void ActualizarUI()
    {
        foreach (Transform child in contenedorIngredientes)
            Destroy(child.gameObject);

        foreach (var ing in ingredientesActuales)
        {
            var icono = Instantiate(iconoIngredientePrefab, contenedorIngredientes);
            icono.GetComponent<Image>().sprite = ing.icon;
        }
    }


    // -------------------------
    // VERIFICAR RECETA
    // -------------------------
    void VerificarReceta()
    {
        foreach (var receta in recetas)
        {
            if (CoincidenIngredientes(receta.ingredientes, ingredientesActuales))
            {
                CrearPocion(receta.pocionResultado);
                return;
            }
        }

        textoResultado.text = "Mezcla incorrecta";
        iconoPocionResultado.sprite = null;
    }


    // -------------------------
    // REVISAR SI LOS 3 INGREDIENTES COINCIDEN
    // -------------------------
    bool CoincidenIngredientes(List<ItemSO> r, List<ItemSO> actual)
    {
        if (r.Count != actual.Count) return false;

        var copiaR = new List<ItemSO>(r);
        var copiaA = new List<ItemSO>(actual);

        foreach (var ing in copiaA)
        {
            if (!copiaR.Contains(ing))
                return false;

            copiaR.Remove(ing);
        }

        return true;
    }


    // -------------------------
    // CREAR POCIÓN
    // -------------------------
    void CrearPocion(PocionSO pocion)
    {
        textoResultado.text = "Poción creada: " + pocion.pocionNombre;

        // Restar ingredientes del inventario
        foreach (var ing in ingredientesActuales)
            inventario.RemoveItem(ing);

        ingredientesActuales.Clear();
        ActualizarUI();
    }


    // -------------------------
    // REINICIAR CALDERO
    // -------------------------
    public void LimpiarCaldero()
    {
        ingredientesActuales.Clear();
        ActualizarUI();
        textoResultado.text = "";
        iconoPocionResultado.sprite = null;
    }
}
