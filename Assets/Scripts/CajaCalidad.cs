using System;
using UnityEngine;

public class CajaCalidad : MonoBehaviour
{
    [Header("Identidad de la caja")]
    [Tooltip("Indice unico 0..3 para que el guardado sea diferente por cada caja")]
    public int boxIndex = 0;

    [Header("UI (opcional)")]
    public CajaCalidadUI ui;

    [Header("Estado (solo lectura)")]
    [SerializeField] private bool unlocked = false;

    // Entrada actual (2 plantas iguales)
    private bool hasInput = false;
    private PlantaTipo inputType = PlantaTipo.NONE;
    private PlantaCalidad inputQuality = PlantaCalidad.NONE;

    // Timer
    private bool running = false;
    private long finishUtcTicks = 0;

    // ---------- Keys PlayerPrefs ----------
    private string KeyUnlocked => "QualityBox_" + boxIndex + "_Unlocked";
    private string KeyHasInput => "QualityBox_" + boxIndex + "_HasInput";
    private string KeyTipo => "QualityBox_" + boxIndex + "_Tipo";
    private string KeyCalidad => "QualityBox_" + boxIndex + "_Calidad";
    private string KeyRunning => "QualityBox_" + boxIndex + "_Running";
    private string KeyFinishTicks => "QualityBox_" + boxIndex + "_FinishTicks";

    private void Awake()
    {
        LoadState();
        NotifyUI();
        unlocked = true;
    }

    private void Update()
    {
        if (!unlocked) return;

        if (running && finishUtcTicks > 0)
        {
            long nowTicks = DateTime.UtcNow.Ticks;
            if (nowTicks >= finishUtcTicks)
                CompleteUpgrade();
            else
                NotifyUI(); // refrescar timer
        }
    }

    // ===================== API PUBLICA =====================

    public bool IsUnlocked() => unlocked;
    public bool HasInput() => hasInput;
    public bool IsRunning() => running;

    public PlantaTipo GetInputType() => inputType;
    public PlantaCalidad GetInputQuality() => inputQuality;

    public TimeSpan GetRemainingTime()
    {
        if (!running || finishUtcTicks <= 0) return TimeSpan.Zero;
        long now = DateTime.UtcNow.Ticks;
        long remaining = finishUtcTicks - now;
        if (remaining <= 0) return TimeSpan.Zero;
        return TimeSpan.FromTicks(remaining);
    }

    public PlantaCalidad? GetOutputQuality()
    {
        if (!hasInput) return null;

        if (inputQuality == PlantaCalidad.Estandar) return PlantaCalidad.Plata;
        if (inputQuality == PlantaCalidad.Plata) return PlantaCalidad.Oro;

        return null; // Oro no mejora
    }

    public void UnlockBox()
    {
        unlocked = true;
        SaveState();
        NotifyUI();
    }

    /// <summary>
    /// Selecciona la "receta" (tipo + calidad). No consume inventario aqui.
    /// </summary>
    public bool TrySetInput(PlantaTipo tipo, PlantaCalidad calidad, out string message)
    {
        message = "";

        if (!unlocked)
        {
            message = "La caja esta bloqueada.";
            return false;
        }

        if (running)
        {
            message = "La caja esta ocupada. Espera a que termine.";
            return false;
        }

        if (tipo == PlantaTipo.NONE || calidad == PlantaCalidad.NONE)
        {
            message = "Seleccion no valida.";
            return false;
        }

        if (calidad == PlantaCalidad.Oro)
        {
            message = "No se puede mejorar mas una planta de calidad Oro.";
            return false;
        }

        hasInput = true;
        inputType = tipo;
        inputQuality = calidad;

        SaveState();
        NotifyUI();
        return true;
    }

    /// <summary>
    /// Inicia la mejora: valida inventario (2 plantas), calcula tiempo (con penalizacion),
    /// consume las 2 plantas y arranca el timer.
    /// </summary>
    public bool StartUpgrade(out string message)
    {
        message = "";

        if (!unlocked)
        {
            message = "La caja esta bloqueada.";
            return false;
        }

        if (!hasInput)
        {
            message = "Primero selecciona 2 plantas del mismo tipo y calidad.";
            return false;
        }

        if (running)
        {
            message = "Ya hay una mejora en proceso.";
            return false;
        }

        var outQ = GetOutputQuality();
        if (outQ == null)
        {
            message = "No se puede mejorar mas esta calidad.";
            return false;
        }

        if (InventorySystem.Instance == null)
        {
            message = "No existe InventorySystem en la escena.";
            return false;
        }

        if (!InventorySystem.Instance.HasPlant(inputType, inputQuality, 2))
        {
            message = "No tienes 2 plantas de ese tipo y calidad.";
            return false;
        }

        int baseSeconds = (inputQuality == PlantaCalidad.Estandar) ? (3 * 60) : (5 * 60);
        int penaltySeconds = CalculateEnergyPenaltySeconds();
        int totalSeconds = baseSeconds + penaltySeconds;

        bool removed = InventorySystem.Instance.RemovePlant(inputType, inputQuality, 2);
        if (!removed)
        {
            message = "No se pudieron consumir las plantas del inventario.";
            return false;
        }

        running = true;
        finishUtcTicks = DateTime.UtcNow.AddSeconds(totalSeconds).Ticks;

        SaveState();
        NotifyUI();

        message = "Mejora iniciada.";
        return true;
    }

    public void CancelInput()
    {
        if (running) return;
        hasInput = false;
        inputType = PlantaTipo.NONE;
        inputQuality = PlantaCalidad.NONE;

        SaveState();
        NotifyUI();
    }

    // ===================== INTERNOS =====================

    private int CalculateEnergyPenaltySeconds()
    {
        float energyPercent = 100f;

        // Compatible con tu sistema de energia si existe
        var energia = FindAnyObjectByType<BarraEnergiaSistema>();
        if (energia != null)
        {
            energyPercent = energia.GetEnergyPercentage();
        }

        // 65% a 36% => +30s
        if (energyPercent <= 65f && energyPercent >= 36f) return 30;

        // 35% a 1% => +60s
        if (energyPercent <= 35f && energyPercent >= 1f) return 60;

        return 0;
    }

    private void CompleteUpgrade()
    {
        var outQ = GetOutputQuality();
        if (outQ != null && InventorySystem.Instance != null)
        {
            InventorySystem.Instance.AddPlant(inputType, outQ.Value, 1);
        }

        running = false;
        finishUtcTicks = 0;

        // Si quieres limpiar entrada al terminar, descomenta:
        // hasInput = false; inputType = PlantaTipo.NONE; inputQuality = PlantaCalidad.NONE;

        SaveState();
        NotifyUI();
    }

    private void SaveState()
    {
        PlayerPrefs.SetInt(KeyUnlocked, unlocked ? 1 : 0);

        PlayerPrefs.SetInt(KeyHasInput, hasInput ? 1 : 0);
        PlayerPrefs.SetInt(KeyTipo, (int)inputType);
        PlayerPrefs.SetInt(KeyCalidad, (int)inputQuality);

        PlayerPrefs.SetInt(KeyRunning, running ? 1 : 0);
        PlayerPrefs.SetString(KeyFinishTicks, finishUtcTicks.ToString());

        PlayerPrefs.Save();
    }

    private void LoadState()
    {
        unlocked = PlayerPrefs.GetInt(KeyUnlocked, 0) == 1;

        hasInput = PlayerPrefs.GetInt(KeyHasInput, 0) == 1;
        inputType = (PlantaTipo)PlayerPrefs.GetInt(KeyTipo, (int)PlantaTipo.NONE);
        inputQuality = (PlantaCalidad)PlayerPrefs.GetInt(KeyCalidad, (int)PlantaCalidad.NONE);

        running = PlayerPrefs.GetInt(KeyRunning, 0) == 1;

        string ticksStr = PlayerPrefs.GetString(KeyFinishTicks, "0");
        if (!long.TryParse(ticksStr, out finishUtcTicks))
            finishUtcTicks = 0;

        // Si el timer vencio mientras el juego estaba cerrado
        if (running && finishUtcTicks > 0 && DateTime.UtcNow.Ticks >= finishUtcTicks)
        {
            CompleteUpgrade();
        }
    }

    private void NotifyUI()
    {
        if (ui != null)
        {
            ui.Refrescar(this);
        }
    }
}
