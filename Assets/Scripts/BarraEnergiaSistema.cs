using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Sistema de barra de energía del invernadero
/// RQF47, RQF53-56: Controla el consumo de energía y penalizaciones de tiempo
/// </summary>
public class BarraEnergiaSistema : MonoBehaviour
{
    public static BarraEnergiaSistema Instance { get; private set; }

    [Header("UI Referencias")]
    public Slider energySlider;
    public TextMeshProUGUI energyText;
    public Image fillImage;

    [Header("Colores de la Barra")]
    public Color highEnergyColor = new Color(0.2f, 0.8f, 0.2f); // Verde
    public Color mediumEnergyColor = new Color(0.9f, 0.9f, 0.2f); // Amarillo
    public Color lowEnergyColor = new Color(0.9f, 0.2f, 0.2f); // Rojo

    [Header("Configuración")]
    public float maxEnergy = 100f;
    private float currentEnergy;

    public event Action<float> OnEnergyChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeEnergy();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeEnergy()
    {
        // RQF47 y RQF53: La barra inicia al 100%
        if (!PlayerPrefs.HasKey("EnergyInitialized"))
        {
            currentEnergy = maxEnergy;
            PlayerPrefs.SetFloat("CurrentEnergy", currentEnergy);
            PlayerPrefs.SetInt("EnergyInitialized", 1);
            PlayerPrefs.Save();

            Debug.Log("Barra de energía inicializada al 100%");
        }
        else
        {
            currentEnergy = PlayerPrefs.GetFloat("CurrentEnergy", maxEnergy);
        }

        UpdateUI();
    }

    /// <summary>
    /// Obtiene la energía actual
    /// </summary>
    public float GetCurrentEnergy()
    {
        return currentEnergy;
    }

    /// <summary>
    /// Obtiene el porcentaje de energía (0-100)
    /// </summary>
    public float GetEnergyPercentage()
    {
        return (currentEnergy / maxEnergy) * 100f;
    }

    /// <summary>
    /// RQF40.4 y RQF56: Verifica si se puede plantar (no puede estar al 1% o menos)
    /// </summary>
    public bool CanPlant(int energyCost)
    {
        float newEnergy = currentEnergy - energyCost;

        // RQF56: Impedir plantar cuando está al 1%
        if (GetEnergyPercentage() <= 1f)
        {
            Debug.LogWarning("No puedes plantar: energía al 1% o menos");
            return false;
        }

        // Verificar que después de plantar no quede en 0% o menos
        return newEnergy >= 1f;
    }

    /// <summary>
    /// RQNF53.1: Consume energía al plantar un cultivo
    /// </summary>
    public bool ConsumeEnergy(int amount)
    {
        if (!CanPlant(amount))
        {
            Debug.LogWarning($"No se puede consumir {amount} de energía");
            return false;
        }

        currentEnergy -= amount;
        currentEnergy = Mathf.Max(currentEnergy, 1f); // Mínimo 1%

        UpdateUI();
        SaveEnergy();
        OnEnergyChanged?.Invoke(currentEnergy);

        Debug.Log($"Consumida {amount} energía. Restante: {GetEnergyPercentage():F1}%");

        return true;
    }

    /// <summary>
    /// Restaura energía al cosechar plantas
    /// </summary>
    public void RestoreEnergy(float amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Min(currentEnergy, maxEnergy); // Máximo 100%

        UpdateUI();
        SaveEnergy();
        OnEnergyChanged?.Invoke(currentEnergy);

        Debug.Log($"Restaurada {amount} energía. Total: {GetEnergyPercentage():F1}%");
    }

    /// <summary>
    /// RQF55, RQNF47.2, RQNF47.3: Calcula la penalización de tiempo según el nivel de energía
    /// </summary>
    public int GetTimePenalty()
    {
        float percentage = GetEnergyPercentage();

        // RQF54: 100%-65% = Sin penalización
        if (percentage >= 65f)
        {
            return 0;
        }
        // RQNF47.2: 65%-36% = +30 segundos
        else if (percentage >= 36f)
        {
            return 30;
        }
        // RQNF47.3: 35%-1% = +60 segundos
        else
        {
            return 60;
        }
    }

    /// <summary>
    /// RQNF53.2: Calcula el tiempo de crecimiento modificado según energía
    /// </summary>
    public float GetModifiedGrowthTime(float baseTimeSeconds)
    {
        int penalty = GetTimePenalty();
        return baseTimeSeconds + penalty;
    }

    /// <summary>
    /// RQNF49.1 y RQNF53.5: Calcula el tiempo de mejora de calidad modificado según energía
    /// </summary>
    public int GetModifiedQualityBoxTime(int baseTimeSeconds)
    {
        // Las cajas de calidad también se ven afectadas por la penalización
        int penalty = GetTimePenalty();
        return baseTimeSeconds + penalty;
    }

    /// <summary>
    /// Actualiza la interfaz visual de la barra
    /// </summary>
    void UpdateUI()
    {
        if (energySlider != null)
        {
            energySlider.maxValue = maxEnergy;
            energySlider.value = currentEnergy;
        }

        if (energyText != null)
        {
            float percentage = GetEnergyPercentage();
            energyText.text = $"{percentage:F0}%";
        }

        if (fillImage != null)
        {
            float percentage = GetEnergyPercentage();

            // Cambiar color según nivel
            if (percentage >= 65f)
            {
                fillImage.color = highEnergyColor;
            }
            else if (percentage >= 36f)
            {
                fillImage.color = mediumEnergyColor;
            }
            else
            {
                fillImage.color = lowEnergyColor;
            }
        }
    }

    /// <summary>
    /// Guarda el estado de la energía
    /// </summary>
    void SaveEnergy()
    {
        PlayerPrefs.SetFloat("CurrentEnergy", currentEnergy);
        PlayerPrefs.Save();
    }

    #region MÉTODOS DE DEBUGGING

    [ContextMenu("Reset Energy (100%)")]
    public void ResetEnergy()
    {
        currentEnergy = maxEnergy;
        UpdateUI();
        SaveEnergy();

        Debug.Log("Energía reseteada al 100%");
    }

    [ContextMenu("Set Energy to 50%")]
    public void SetEnergyTo50()
    {
        currentEnergy = maxEnergy * 0.5f;
        UpdateUI();
        SaveEnergy();

        Debug.Log("Energía establecida al 50%");
    }

    [ContextMenu("Set Energy to 40% (Penalización 30s)")]
    public void SetEnergyTo40()
    {
        currentEnergy = maxEnergy * 0.4f;
        UpdateUI();
        SaveEnergy();

        Debug.Log($"Energía al 40% - Penalización: {GetTimePenalty()}s");
    }

    [ContextMenu("Set Energy to 20% (Penalización 60s)")]
    public void SetEnergyTo20()
    {
        currentEnergy = maxEnergy * 0.2f;
        UpdateUI();
        SaveEnergy();

        Debug.Log($"Energía al 20% - Penalización: {GetTimePenalty()}s");
    }

    [ContextMenu("Set Energy to 2% (Casi bloqueado)")]
    public void SetEnergyTo2()
    {
        currentEnergy = maxEnergy * 0.02f;
        UpdateUI();
        SaveEnergy();

        Debug.Log($"Energía al 2% - Puede plantar? {CanPlant(1)}");
    }

    [ContextMenu("Mostrar Estado Actual")]
    public void ShowCurrentState()
    {
        float percentage = GetEnergyPercentage();
        int penalty = GetTimePenalty();
        bool canPlant = CanPlant(4); // Probar con consumo típico

        Debug.Log("=== ESTADO DE LA BARRA DE ENERGÍA ===");
        Debug.Log($"Energía: {currentEnergy:F1}/{maxEnergy} ({percentage:F1}%)");
        Debug.Log($"Penalización de tiempo: +{penalty} segundos");
        Debug.Log($"¿Puede plantar? {(canPlant ? "SÍ" : "NO")}");

        if (percentage >= 65f)
            Debug.Log("Estado: ÓPTIMO (sin penalización)");
        else if (percentage >= 36f)
            Debug.Log("Estado: MEDIO (+30s de penalización)");
        else if (percentage > 1f)
            Debug.Log("Estado: BAJO (+60s de penalización)");
        else
            Debug.Log("Estado: CRÍTICO (plantado bloqueado)");
    }

    #endregion
}