using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        // Boton preparar
        if (botonPrepararPocion != null)
        {
            botonPrepararPocion.onClick.RemoveAllListeners();
            botonPrepararPocion.onClick.AddListener(OnClickPrepararPocion);
        }

        // Panel advertencia
        if (panelAdvertencia != null) panelAdvertencia.SetActive(false);

        // Generar UI
        GenerarGridPociones();
        LimpiarPanelDetalles();

        // Sync inicial (por si entraste directo al menu)
        SyncCalderoInventoryDesdeInvernadero();
    }

    private void LimpiarPanelDetalles()
    {
        if (imagenPocionGrande != null) imagenPocionGrande.enabled = false;

        if (textoNombrePocion != null) textoNombrePocion.text = "Selecciona una poción";
        if (textoEfectoPocion != null) textoEfectoPocion.text = "---";

        if (imagenIngrediente1 != null) imagenIngrediente1.enabled = false;
        if (imagenIngrediente2 != null) imagenIngrediente2.enabled = false;
        if (imagenIngrediente3 != null) imagenIngrediente3.enabled = false;

        if (textoIngrediente1 != null) textoIngrediente1.text = "???";
        if (textoIngrediente2 != null) textoIngrediente2.text = "???";
        if (textoIngrediente3 != null) textoIngrediente3.text = "???";

        SetBotonPreparar(false, "...");
    }

    private void SetBotonPreparar(bool enabled, string txt)
    {
        if (botonPrepararPocion != null) botonPrepararPocion.interactable = enabled;
        if (textoBotonPreparar != null) textoBotonPreparar.text = txt;
    }

    private void GenerarGridPociones()
    {
        if (contenedorGridPociones == null || slotPocionPrefab == null)
        {
            Debug.LogError("[ELEGIR POCIÓN] Falta asignar contenedorGridPociones o slotPocionPrefab.");
            return;
        }

        for (int i = contenedorGridPociones.childCount - 1; i >= 0; i--)
            Destroy(contenedorGridPociones.GetChild(i).gameObject);

        for (int i = 0; i < todasLasPociones.Count; i++)
        {
            PocionSO pocion = todasLasPociones[i];
            if (pocion == null) continue;

            GameObject slot = Instantiate(slotPocionPrefab, contenedorGridPociones);

            SlotPocion slotComponent = slot.GetComponent<SlotPocion>();
            if (slotComponent != null)
            {
                slotComponent.ConfigurarSlot(pocion, this);
            }
            else
            {
                Image imagen = slot.GetComponent<Image>();
                if (imagen != null) imagen.sprite = pocion.icon;

                Button boton = slot.GetComponent<Button>();
                if (boton != null)
                {
                    PocionSO pocionRef = pocion;
                    boton.onClick.RemoveAllListeners();
                    boton.onClick.AddListener(() => SeleccionarPocion(pocionRef));
                }
            }
        }
    }

    public void SeleccionarPocion(PocionSO pocion)
    {
        if (pocion == null) return;

        pocionSeleccionada = pocion;

        MostrarDetallesPocion(pocion);

        // 👇 CLAVE: antes de verificar, sync (InventorySystem -> InventoryManager)
        SyncCalderoInventoryDesdeInvernadero();

        VerificarIngredientesDisponibles(pocion);
    }

    private void MostrarDetallesPocion(PocionSO pocion)
    {
        if (imagenPocionGrande != null)
        {
            imagenPocionGrande.sprite = pocion.icon;
            imagenPocionGrande.enabled = true;
        }

        if (textoNombrePocion != null) textoNombrePocion.text = pocion.pocionNombre;
        if (textoEfectoPocion != null) textoEfectoPocion.text = $"<b>Efecto:</b> {pocion.efecto}";

        // Ingrediente 1 (Suero)
        SetIngredienteUI(pocion.suero, imagenIngrediente1, textoIngrediente1);

        // Ingrediente 2
        SetIngredienteUI(pocion.ingrediente1, imagenIngrediente2, textoIngrediente2);

        // Ingrediente 3
        SetIngredienteUI(pocion.ingrediente2, imagenIngrediente3, textoIngrediente3);
    }

    private void SetIngredienteUI(ItemSO item, Image img, TextMeshProUGUI txt)
    {
        if (item != null)
        {
            if (img != null) { img.sprite = item.icon; img.enabled = true; }
            if (txt != null) txt.text = item.itemNombre;
        }
        else
        {
            if (img != null) img.enabled = false;
            if (txt != null) txt.text = "???";
        }
    }

    private void VerificarIngredientesDisponibles(PocionSO pocion)
    {
        if (InventoryManager.instancia == null)
        {
            Debug.LogWarning("[ELEGIR POCIÓN] InventoryManager.instancia es null. Botón deshabilitado.");
            SetBotonPreparar(false, "...");
            return;
        }

        List<string> faltantes = new List<string>();

        if (!HasItem1(pocion.suero)) faltantes.Add(pocion.suero.itemNombre);
        if (!HasItem1(pocion.ingrediente1)) faltantes.Add(pocion.ingrediente1.itemNombre);
        if (!HasItem1(pocion.ingrediente2)) faltantes.Add(pocion.ingrediente2.itemNombre);

        if (faltantes.Count > 0)
        {
            SetBotonPreparar(false, "...");
            if (textoAdvertencia != null) textoAdvertencia.text = "Falta:\n\n" + string.Join("\n", faltantes);
        }
        else
        {
            SetBotonPreparar(true, "Preparar");
            if (textoAdvertencia != null) textoAdvertencia.text = "";
        }
    }

    private bool HasItem1(ItemSO item)
    {
        if (item == null) return true; // si no requiere, ok
        if (InventoryManager.instancia == null) return false;

        // No asumo que tengas HasItem(item, cantidad). Me voy por GetItemCount.
        int c = InventoryManager.instancia.GetItemCount(item);
        return c >= 1;
    }

    private void OnClickPrepararPocion()
    {
        if (pocionSeleccionada == null)
        {
            MostrarAdvertencia("Elige una poción primero");
            return;
        }

        // Sync final por seguridad
        SyncCalderoInventoryDesdeInvernadero();

        if (!TieneTodosLosIngredientes(pocionSeleccionada))
        {
            MostrarAdvertencia("No tienes todos los ingredientes");
            return;
        }

        PlayerPrefs.SetString("PocionSeleccionada", pocionSeleccionada.pocionNombre);
        PlayerPrefs.Save();

        SceneManager.LoadScene(nombreEscenaCaldero);
    }

    private bool TieneTodosLosIngredientes(PocionSO pocion)
    {
        return HasItem1(pocion.suero) && HasItem1(pocion.ingrediente1) && HasItem1(pocion.ingrediente2);
    }

    private void MostrarAdvertencia(string mensaje)
    {
        if (panelAdvertencia == null) return;

        panelAdvertencia.SetActive(true);
        if (textoAdvertencia != null) textoAdvertencia.text = mensaje;

        CancelInvoke(nameof(OcultarAdvertencia));
        Invoke(nameof(OcultarAdvertencia), 2f);
    }

    private void OcultarAdvertencia()
    {
        if (panelAdvertencia != null) panelAdvertencia.SetActive(false);
    }

    // =========================================================
    // SYNC: InventorySystem -> InventoryManager (plantas + sueros)
    // =========================================================

    private void SyncCalderoInventoryDesdeInvernadero()
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("[ELEGIR POCIÓN] InventorySystem.Instance es null (no hay inventario del invernadero).");
            return;
        }

        if (InventoryManager.instancia == null)
        {
            Debug.LogWarning("[ELEGIR POCIÓN] InventoryManager.instancia es null (no hay inventario del caldero).");
            return;
        }

        // Construimos el set de ItemSO que importan (solo los que usan las pociones)
        HashSet<ItemSO> needed = new HashSet<ItemSO>();
        for (int i = 0; i < todasLasPociones.Count; i++)
        {
            var p = todasLasPociones[i];
            if (p == null) continue;

            if (p.suero != null) needed.Add(p.suero);
            if (p.ingrediente1 != null) needed.Add(p.ingrediente1);
            if (p.ingrediente2 != null) needed.Add(p.ingrediente2);
        }

        // Para cada ItemSO requerido, calculamos cantidad REAL en InventorySystem
        foreach (var itemSO in needed)
        {
            if (itemSO == null) continue;

            int desired = GetCountFromInventorySystem(itemSO);
            SetAbsoluteCountInInventoryManager(itemSO, desired);
        }
    }

    private int GetCountFromInventorySystem(ItemSO itemSO)
    {
        var inv = InventorySystem.Instance;
        if (inv == null || itemSO == null) return 0;

        // Intento 1: si el nombre coincide con PlantaTipo, sumo todas las calidades
        if (TryGetPlantTotal(inv, itemSO.itemNombre, out int totalPlant))
            return totalPlant;

        // Intento 2: si no es planta, lo trato como suero por nombre (tolerante a acentos)
        return GetSerumCount(inv, itemSO.itemNombre);
    }

    private bool TryGetPlantTotal(InventorySystem inv, string itemNombre, out int total)
    {
        total = 0;
        if (inv == null || string.IsNullOrWhiteSpace(itemNombre)) return false;

        if (!Enum.TryParse<PlantaTipo>(itemNombre, true, out var tipo)) return false;
        if (tipo == PlantaTipo.NONE) return false;

        foreach (PlantaCalidad q in Enum.GetValues(typeof(PlantaCalidad)))
            total += inv.GetPlantCount(tipo, q);

        return true;
    }

    private int GetSerumCount(InventorySystem inv, string itemNombre)
    {
        if (inv == null || string.IsNullOrWhiteSpace(itemNombre)) return 0;

        string key = NormKey(itemNombre);

        for (int i = 0; i < inv.sueros.Count; i++)
        {
            var s = inv.sueros[i];
            if (s == null) continue;

            if (NormKey(s.sueroNombre) == key)
                return s.cantidad;
        }

        return 0;
    }

    private void SetAbsoluteCountInInventoryManager(ItemSO itemSO, int desired)
    {
        if (itemSO == null) return;
        if (desired < 0) desired = 0;

        int current = InventoryManager.instancia.GetItemCount(itemSO);
        int delta = desired - current;

        if (delta > 0) InventoryManager.instancia.AddItem(itemSO, delta);
        else if (delta < 0) InventoryManager.instancia.RemoveItem(itemSO, -delta);
    }

    private static string NormKey(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        s = s.Trim();

        // Quita acentos: "Energía" -> "Energia"
        string formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);

        foreach (char ch in formD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }

        return sb.ToString()
            .Normalize(NormalizationForm.FormC)
            .Trim()
            .ToLowerInvariant();
    }
}

// ------------------------------------------------------------
// Slot de poción (igual que el tuyo, solo un poquito más limpio)
// ------------------------------------------------------------
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

        if (imagenPocion == null) imagenPocion = GetComponent<Image>();
        if (botonSlot == null) botonSlot = GetComponent<Button>();

        if (imagenPocion != null && pocion != null) imagenPocion.sprite = pocion.icon;

        if (botonSlot != null)
        {
            botonSlot.onClick.RemoveAllListeners();
            botonSlot.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (menu != null && pocion != null)
            menu.SeleccionarPocion(pocion);
    }
}
