using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ElegirPocion : MonoBehaviour
{
    public static ElegirPocion Instance { get; private set; }

    [Header("Lista de Pociones")]
    public List<PocionSO> todasLasPociones = new List<PocionSO>();

    [Header("Referencias UI - Lado Izquierdo (Grid de Pociones)")]
    public Transform contenedorGridPociones;
    public GameObject slotPocionPrefab;

    [Header("Referencias UI - Lado Derecho (Detalles)")]
    public Image imagenPocionGrande;
    public TextMeshProUGUI textoNombrePocion;
    public TextMeshProUGUI textoEfectoPocion;

    [Header("Ingredientes en Panel Derecho")]
    public Image imagenIngrediente1;
    public TextMeshProUGUI textoIngrediente1;
    public Image imagenIngrediente2;
    public TextMeshProUGUI textoIngrediente2;
    public Image imagenIngrediente3;
    public TextMeshProUGUI textoIngrediente3;

    [Header("Botón de Preparación")]
    public Button botonPrepararPocion;
    public TextMeshProUGUI textoBotonPreparar;

    [Header("Panel de Advertencia")]
    public GameObject panelAdvertencia;
    public TextMeshProUGUI textoAdvertencia;

    [Header("Configuración")]
    public string nombreEscenaCaldero = "Caldero";

    private PocionSO pocionSeleccionada;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {

        if (InventoryManager.instancia == null)
        {
            Debug.LogError("[ELEGIR POCIÓN] ¡No hay InventoryManager! Asegúrate de tener el GameObject en la primera escena.");
        }

        if (botonPrepararPocion != null)
        {
            botonPrepararPocion.onClick.AddListener(OnClickPrepararPocion);
        }

        if (panelAdvertencia != null)
        {
            panelAdvertencia.SetActive(false);
        }

        GenerarGridPociones();
        LimpiarPanelDetalles();
    }

    void LimpiarPanelDetalles()
    {
        if (imagenPocionGrande != null)
            imagenPocionGrande.enabled = false;

        if (textoNombrePocion != null)
            textoNombrePocion.text = "Selecciona una poción";

        if (textoEfectoPocion != null)
            textoEfectoPocion.text = "---";

        if (imagenIngrediente1 != null) imagenIngrediente1.enabled = false;
        if (imagenIngrediente2 != null) imagenIngrediente2.enabled = false;
        if (imagenIngrediente3 != null) imagenIngrediente3.enabled = false;

        if (textoIngrediente1 != null) textoIngrediente1.text = "???";
        if (textoIngrediente2 != null) textoIngrediente2.text = "???";
        if (textoIngrediente3 != null) textoIngrediente3.text = "???";

        if (botonPrepararPocion != null)
            botonPrepararPocion.interactable = false;
    }

    void GenerarGridPociones()
    {
        if (contenedorGridPociones == null || slotPocionPrefab == null)
        {
            Debug.LogError("[MENÚ POCIONES] Falta asignar contenedor o prefab!");
            return;
        }

        foreach (Transform child in contenedorGridPociones)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < todasLasPociones.Count; i++)
        {
            PocionSO pocion = todasLasPociones[i];

            if (pocion == null)
            {
                Debug.LogWarning($"[MENÚ POCIONES] Poción en índice {i} es null!");
                continue;
            }

            GameObject slot = Instantiate(slotPocionPrefab, contenedorGridPociones);

            SlotPocion slotComponent = slot.GetComponent<SlotPocion>();
            if (slotComponent != null)
            {
                slotComponent.ConfigurarSlot(pocion, this);
            }
            else
            {
                Image imagen = slot.GetComponent<Image>();
                if (imagen != null)
                {
                    imagen.sprite = pocion.icon;
                }

                Button boton = slot.GetComponent<Button>();
                if (boton != null)
                {
                    PocionSO pocionRef = pocion;
                    boton.onClick.AddListener(() => SeleccionarPocion(pocionRef));
                }
            }
        }

        Debug.Log($"[MENÚ POCIONES] Generadas {todasLasPociones.Count} pociones en el grid");
    }

    public void SeleccionarPocion(PocionSO pocion)
    {
        if (pocion == null)
        {
            Debug.LogWarning("[MENÚ POCIONES] Poción null seleccionada");
            return;
        }

        pocionSeleccionada = pocion;
        Debug.Log($"[MENÚ POCIONES] Poción seleccionada: {pocion.pocionNombre}");

        MostrarDetallesPocion(pocion);
        VerificarIngredientesDisponibles(pocion);
    }

    void MostrarDetallesPocion(PocionSO pocion)
    {
        if (imagenPocionGrande != null)
        {
            imagenPocionGrande.sprite = pocion.icon;
            imagenPocionGrande.enabled = true;
        }

        if (textoNombrePocion != null)
        {
            textoNombrePocion.text = pocion.pocionNombre;
        }

        if (textoEfectoPocion != null)
        {
            textoEfectoPocion.text = $"<b>Efecto:</b> {pocion.efecto}";
        }

        // Ingrediente 1 (Suero)
        if (pocion.suero != null)
        {
            if (imagenIngrediente1 != null)
            {
                imagenIngrediente1.sprite = pocion.suero.icon;
                imagenIngrediente1.enabled = true;
            }
            if (textoIngrediente1 != null)
            {
                textoIngrediente1.text = pocion.suero.itemNombre;
            }
        }
        else
        {
            if (imagenIngrediente1 != null) imagenIngrediente1.enabled = false;
            if (textoIngrediente1 != null) textoIngrediente1.text = "???";
        }

        // Ingrediente 2
        if (pocion.ingrediente1 != null)
        {
            if (imagenIngrediente2 != null)
            {
                imagenIngrediente2.sprite = pocion.ingrediente1.icon;
                imagenIngrediente2.enabled = true;
            }
            if (textoIngrediente2 != null)
            {
                textoIngrediente2.text = pocion.ingrediente1.itemNombre;
            }
        }
        else
        {
            if (imagenIngrediente2 != null) imagenIngrediente2.enabled = false;
            if (textoIngrediente2 != null) textoIngrediente2.text = "???";
        }

        // Ingrediente 3
        if (pocion.ingrediente2 != null)
        {
            if (imagenIngrediente3 != null)
            {
                imagenIngrediente3.sprite = pocion.ingrediente2.icon;
                imagenIngrediente3.enabled = true;
            }
            if (textoIngrediente3 != null)
            {
                textoIngrediente3.text = pocion.ingrediente2.itemNombre;
            }
        }
        else
        {
            if (imagenIngrediente3 != null) imagenIngrediente3.enabled = false;
            if (textoIngrediente3 != null) textoIngrediente3.text = "???";
        }
    }

    /// <summary>
    /// ACTUALIZADO: Ahora usa InventoryManager.instancia
    /// </summary>
    void VerificarIngredientesDisponibles(PocionSO pocion)
    {
        // CAMBIO PRINCIPAL: Usar la instancia singleton
        if (InventoryManager.instancia == null)
        {
            Debug.LogError("[MENÚ POCIONES] InventoryManager no existe!");
            if (botonPrepararPocion != null)
                botonPrepararPocion.interactable = false;
            return;
        }

        List<string> ingredientesFaltantes = new List<string>();

        // Verificar suero
        if (pocion.suero != null && !InventoryManager.instancia.HasItem(pocion.suero))
        {
            ingredientesFaltantes.Add(pocion.suero.itemNombre);
        }

        // Verificar ingrediente 1
        if (pocion.ingrediente1 != null && !InventoryManager.instancia.HasItem(pocion.ingrediente1))
        {
            ingredientesFaltantes.Add(pocion.ingrediente1.itemNombre);
        }

        // Verificar ingrediente 2
        if (pocion.ingrediente2 != null && !InventoryManager.instancia.HasItem(pocion.ingrediente2))
        {
            ingredientesFaltantes.Add(pocion.ingrediente2.itemNombre);
        }

        if (ingredientesFaltantes.Count > 0)
        {
            if (botonPrepararPocion != null)
            {
                botonPrepararPocion.interactable = false;
            }

            if (textoBotonPreparar != null)
            {
                textoBotonPreparar.text = "...";
            }

            if (textoAdvertencia != null)
            {
                textoAdvertencia.text = "Falta:\n\n" + string.Join("\n", ingredientesFaltantes);
            }

            Debug.LogWarning($"[MENÚ POCIONES] Faltan ingredientes: {string.Join(", ", ingredientesFaltantes)}");
        }
        else
        {
            if (botonPrepararPocion != null)
            {
                botonPrepararPocion.interactable = true;
            }

            if (textoBotonPreparar != null)
            {
                textoBotonPreparar.text = "Preparar";
            }

            Debug.Log($"[MENÚ POCIONES]  Todos los ingredientes disponibles para {pocion.pocionNombre}");
        }
    }

    void OnClickPrepararPocion()
    {
        if (pocionSeleccionada == null)
        {
            MostrarAdvertencia("Elige una poción primero");
            return;
        }

        if (!TieneTodosLosIngredientes(pocionSeleccionada))
        {
            MostrarAdvertencia("No tienes todos los ingredientes");
            return;
        }

        PlayerPrefs.SetString("PocionSeleccionada", pocionSeleccionada.pocionNombre);
        PlayerPrefs.Save();

        Debug.Log($"[MENÚ POCIONES] Preparando {pocionSeleccionada.pocionNombre} - Cargando caldero...");

        SceneManager.LoadScene(nombreEscenaCaldero);
    }

    bool TieneTodosLosIngredientes(PocionSO pocion)
    {
        if (InventoryManager.instancia == null) return false;

        bool tieneSuero = pocion.suero == null || InventoryManager.instancia.HasItem(pocion.suero);
        bool tieneIng1 = pocion.ingrediente1 == null || InventoryManager.instancia.HasItem(pocion.ingrediente1);
        bool tieneIng2 = pocion.ingrediente2 == null || InventoryManager.instancia.HasItem(pocion.ingrediente2);

        return tieneSuero && tieneIng1 && tieneIng2;
    }

    void MostrarAdvertencia(string mensaje)
    {
        if (panelAdvertencia != null)
        {
            panelAdvertencia.SetActive(true);

            if (textoAdvertencia != null)
            {
                textoAdvertencia.text = mensaje;
            }

            Invoke(nameof(OcultarAdvertencia), 2f);
        }
    }

    void OcultarAdvertencia()
    {
        if (panelAdvertencia != null)
        {
            panelAdvertencia.SetActive(false);
        }
    }
}

public class SlotPocion : MonoBehaviour
{
    public Image imagenPocion;
    public Button botonSlot;

    private PocionSO pocion;
    private ElegirPocion menu;

    public void ConfigurarSlot(PocionSO pocionData, ElegirPocion menuRef)
    {
        pocion = pocionData;
        menu = menuRef;

        if (imagenPocion == null)
            imagenPocion = GetComponent<Image>();

        if (imagenPocion != null && pocion != null)
        {
            imagenPocion.sprite = pocion.icon;
        }

        if (botonSlot == null)
            botonSlot = GetComponent<Button>();

        if (botonSlot != null)
        {
            botonSlot.onClick.RemoveAllListeners();
            botonSlot.onClick.AddListener(OnClick);
        }
    }

    void OnClick()
    {
        if (menu != null && pocion != null)
        {
            menu.SeleccionarPocion(pocion);
        }
    }
}