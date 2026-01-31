using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CajaCalidadUI : MonoBehaviour
{
    [Header("Slots UI (derecha)")]
    public Image entradaIcon1;
    public TextMeshProUGUI entradaCantidad1;

    public Image entradaIcon2;
    public TextMeshProUGUI entradaCantidad2;

    public Image salidaIcon;
    public TextMeshProUGUI salidaCantidad;

    [Header("Timer / Mensajes")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI messageText;

    [Header("Botones")]
    public Button mejorarButton;
    public Button cerrarButton;
    public Button cancelarEntradaButton;

    [Header("DB sprites (opcional)")]
    public PlantaBD plantDB;

    private CajaCalidad caja;

    private void Awake()
    {
        if (cerrarButton != null)
            cerrarButton.onClick.AddListener(Cerrar);

        if (mejorarButton != null)
            mejorarButton.onClick.AddListener(OnClickMejorar);

        if (cancelarEntradaButton != null)
            cancelarEntradaButton.onClick.AddListener(OnClickCancelarEntrada);
    }

    public void SetCaja(CajaCalidad nuevaCaja)
    {
        caja = nuevaCaja;

        // Conectar UI <-> caja
        if (caja != null)
            caja.ui = this;

        Refrescar();
    }

    // Este metodo lo llama CajaCalidad.NotifyUI()
    public void Refrescar(CajaCalidad cajaActual)
    {
        caja = cajaActual;
        Refrescar();
    }

    public void Refrescar()
    {
        if (caja == null) return;

        bool unlocked = caja.IsUnlocked();

        if (!unlocked)
        {
            SetMessage("Caja bloqueada.");
            SetSlotEmpty();
            SetTimer("--:--");
            SetButtonsState(false);
            return;
        }

        // Entrada
        if (caja.HasInput())
        {
            var tipo = caja.GetInputType();
            var calidad = caja.GetInputQuality();

            Sprite spr = GetPlantSprite(tipo);
            SetEntradaSlots(spr, "2"); // siempre requiere 2
            SetMessage(tipo + " " + calidad);
        }
        else
        {
            SetEntradaSlots(null, "0");
            SetMessage("Selecciona 2 plantas del inventario.");
        }

        // Salida
        PlantaCalidad? outQ = caja.GetOutputQuality();
        if (outQ.HasValue && caja.HasInput())
        {
            Sprite sprOut = GetPlantSprite(caja.GetInputType());
            salidaIcon.sprite = sprOut;
            salidaIcon.enabled = (sprOut != null);

            salidaCantidad.text = "1";
        }
        else
        {
            if (salidaIcon != null) { salidaIcon.sprite = null; salidaIcon.enabled = false; }
            if (salidaCantidad != null) salidaCantidad.text = "0";
        }

        // Timer
        TimeSpan t = caja.GetRemainingTime();
        if (t.TotalSeconds > 0)
            SetTimer(t.ToString(@"mm\:ss"));
        else
            SetTimer("--:--");

        // Botones
        bool canStart = caja.HasInput() && !caja.IsRunning();
        SetButtonsState(canStart);

        // Si esta corriendo, no se permite cambiar entrada
        if (cancelarEntradaButton != null)
            cancelarEntradaButton.interactable = !caja.IsRunning();
    }

    // Lo llama InventarioCalidadUI cuando el usuario clickea algo del inventario
    public void OnSeleccionInventario(PlantaTipo tipo, PlantaCalidad calidad)
    {
        if (caja == null)
        {
            SetMessage("No hay caja seleccionada.");
            return;
        }

        if (!caja.IsUnlocked())
        {
            SetMessage("Caja bloqueada.");
            return;
        }

        if (caja.IsRunning())
        {
            SetMessage("Caja ocupada. Espera a que termine.");
            return;
        }

        // Validar que tengas 2 (para evitar seleccionar cosas imposibles)
        if (InventorySystem.Instance == null)
        {
            SetMessage("No existe InventorySystem.");
            return;
        }

        if (!InventorySystem.Instance.HasPlant(tipo, calidad, 2))
        {
            SetMessage("Necesitas 2 plantas iguales para mejorar.");
            return;
        }

        string msg;
        if (!caja.TrySetInput(tipo, calidad, out msg))
        {
            SetMessage(msg);
            return;
        }

        SetMessage("Entrada seleccionada. Presiona MEJORAR.");
        Refrescar();
    }

    private void OnClickMejorar()
    {
        if (caja == null) return;

        string msg;
        bool ok = caja.StartUpgrade(out msg);
        SetMessage(msg);

        if (ok)
            Refrescar();
    }

    private void OnClickCancelarEntrada()
    {
        if (caja == null) return;
        caja.CancelInput();
        SetMessage("Entrada limpiada.");
        Refrescar();
    }

    public void Cerrar()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        // Refrescar timer mientras corre
        if (caja != null && caja.IsRunning())
        {
            Refrescar();
        }
    }

    // ---------------- Helpers UI ----------------

    private void SetButtonsState(bool canStart)
    {
        if (mejorarButton != null)
            mejorarButton.interactable = canStart;
    }

    private void SetMessage(string msg)
    {
        if (messageText != null)
            messageText.text = msg;
    }

    private void SetTimer(string s)
    {
        if (timerText != null)
            timerText.text = s;
    }

    private void SetSlotEmpty()
    {
        SetEntradaSlots(null, "0");
        if (salidaIcon != null) { salidaIcon.sprite = null; salidaIcon.enabled = false; }
        if (salidaCantidad != null) salidaCantidad.text = "0";
    }

    private void SetEntradaSlots(Sprite spr, string cantidad)
    {
        if (entradaIcon1 != null)
        {
            entradaIcon1.sprite = spr;
            entradaIcon1.enabled = (spr != null);
        }
        if (entradaIcon2 != null)
        {
            entradaIcon2.sprite = spr;
            entradaIcon2.enabled = (spr != null);
        }

        if (entradaCantidad1 != null) entradaCantidad1.text = cantidad;
        if (entradaCantidad2 != null) entradaCantidad2.text = cantidad;
    }

    private Sprite GetPlantSprite(PlantaTipo tipo)
    {
        if (tipo == PlantaTipo.NONE) return null;

        if (plantDB == null && InvernaderoManager.Instance != null)
            plantDB = InvernaderoManager.Instance.plantDatabase;

        if (plantDB == null) return null;

        PlantData data = plantDB.GetPlantas(tipo);
        return (data != null) ? data.frutoSprite : null;
    }
}
