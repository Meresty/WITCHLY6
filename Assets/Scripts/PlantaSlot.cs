using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// RQF36-42: Representa un espacio individual de cultivo en el invernadero
/// </summary>
public class PlantaSlot : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image slotImage;
    public Image plantImage;
    public TextMeshProUGUI timerText;
    public GameObject timerPanel;
    public Button harvestButton;

    [Header("Sprites")]
    public Sprite emptySlotSprite;
    public Sprite readyHighlightSprite;

    [Header("Efectos Visuales")]
    public Color normalColor = Color.white;
    public Color readyColor = new Color(1f, 0.84f, 0f);
    public GameObject readyParticles; // Opcional: partículas cuando está listo

    [Header("Estado")]
    [Tooltip("RQNF36.2: Tipo de planta que puede plantarse en este slot")]
    public PlantaTipo slotPlantType;
    public bool isOccupied = false;

    private PlantaTipo currentPlant;
    private SemillaCiclo currentSeedCycle;
    private DateTime plantedTime;
    private float growthTimeSeconds;
    private bool isReady = false;

    void Start()
    {
        if (harvestButton != null)
        {
            harvestButton.onClick.AddListener(OnHarvestClicked);
            harvestButton.gameObject.SetActive(false);
        }

        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }

        if (readyParticles != null)
        {
            readyParticles.SetActive(false);
        }

        UpdateSlotVisuals();
        LoadSlotState();
    }

    void Update()
    {
        // RQF38 y RQF38.1: Actualizar temporizador si hay planta creciendo
        if (isOccupied && !isReady)
        {
            UpdateTimer();
        }
    }

    /// <summary>
    /// RQF37 y RQNF40.2: Verifica si el slot puede recibir una planta
    /// </summary>
    public bool CanPlant()
    {
        return !isOccupied;
    }

    /// <summary>
    /// RQF40: Planta una semilla en este slot
    /// </summary>
    public bool PlantSeed(PlantaTipo plantType)
    {
        // RQF37 y RQNF40.2: Verificar que el slot esté vacío
        if (!CanPlant())
        {
            Debug.Log($"Slot ocupado, no se puede plantar");
            return false;
        }

        // RQNF36.2: Verificar que sea el tipo correcto de planta para este slot
        if (plantType != slotPlantType)
        {
            Debug.LogWarning($"Tipo de planta incorrecto. Este slot es para {slotPlantType}, intentas plantar {plantType}");
            return false;
        }

        PlantData data = InvernaderoManager.Instance.plantDatabase.GetPlantas(plantType);
        if (data == null)
        {
            Debug.LogError($"PlantData no encontrado para {plantType}");
            return false;
        }

        // RQNF40.4 y RQF56: Verificar energía suficiente
        if (!BarraEnergiaSistema.Instance.CanPlant(data.energiaConsumo))
        {
            Debug.LogWarning($"Energía insuficiente para plantar {plantType}");
            return false;
        }

        // RQNF40.3: Verificar y consumir semilla (solo si no es perenne)
        if (data.semillaCiclo == SemillaCiclo.Replantar)
        {
            if (!InventorySystem.Instance.HasSeed(plantType))
            {
                Debug.LogWarning($"No hay semillas de {plantType} en el inventario");
                // Restaurar energía si ya se consumió
                return false;
            }
            InventorySystem.Instance.RemoveSeed(plantType, 1);
        }

        // RQNF53.1: Consumir energía
        if (!BarraEnergiaSistema.Instance.ConsumeEnergy(data.energiaConsumo))
        {
            // Si falla, devolver la semilla
            if (data.semillaCiclo == SemillaCiclo.Replantar)
            {
                InventorySystem.Instance.AddSemilla(plantType, 1);
            }
            return false;
        }

        // Plantar
        currentPlant = plantType;
        currentSeedCycle = data.semillaCiclo;
        isOccupied = true;
        isReady = false;
        plantedTime = DateTime.Now;

        // RQF38.1 y RQNF53.2: Calcular tiempo de crecimiento con penalización de energía
        float baseTime = data.tiempoCrecimientoMinutos * 60f; // Convertir a segundos
        growthTimeSeconds = BarraEnergiaSistema.Instance.GetModifiedGrowthTime(baseTime);

        UpdateSlotVisuals();
        SaveSlotState();

        Debug.Log($"Plantado {plantType} - Tiempo: {growthTimeSeconds}s ({growthTimeSeconds / 60}min {growthTimeSeconds % 60}s)");
        return true;
    }

    /// <summary>
    /// RQF38: Actualiza el temporizador de crecimiento
    /// </summary>
    void UpdateTimer()
    {
        TimeSpan elapsed = DateTime.Now - plantedTime;
        float remainingSeconds = growthTimeSeconds - (int)elapsed.TotalSeconds;

        if (remainingSeconds <= 0)
        {
            // RQF39: Planta lista para cosechar
            isReady = true;

            if (timerText != null)
                timerText.text = "¡Listo!";

            if (harvestButton != null)
                harvestButton.gameObject.SetActive(true);

            // RQF39: Resaltar en dorado
            if (plantImage != null)
            {
                plantImage.color = readyColor;
            }

            if (readyParticles != null)
            {
                readyParticles.SetActive(true);
            }

            SaveSlotState();
        }
        else
        {
            // Mostrar tiempo restante en formato MM:SS
            float minutes = remainingSeconds / 60;
            int seconds = (int)remainingSeconds % 60;

            if (timerText != null)
                timerText.text = $"{minutes:D2}:{seconds:D2}";
        }
    }

    /// <summary>
    /// Llamado cuando se hace click en el botón de cosechar
    /// </summary>
    void OnHarvestClicked()
    {
        if (isReady && isOccupied)
        {
            Harvest();
        }
    }

    /// <summary>
    /// RQF41: Cosecha la planta del slot
    /// </summary>
    public void Harvest()
    {
        // RQNF41.1: Verificar que el contador haya llegado a 0
        if (!isReady || !isOccupied)
        {
            Debug.LogWarning("No se puede cosechar: planta no lista o slot vacío");
            return;
        }

        PlantData data = InvernaderoManager.Instance.plantDatabase.GetPlantas(currentPlant);
        if (data == null)
        {
            Debug.LogError($"PlantData no encontrado para {currentPlant}");
            return;
        }

        // Añadir planta cosechada al inventario
        InventorySystem.Instance.AddPlant(currentPlant, PlantaCalidad.Estandar, data.cosechaCantidad);

        // Restaurar energía consumida
        BarraEnergiaSistema.Instance.RestoreEnergy(data.energiaConsumo);

        Debug.Log($"Cosechado {data.cosechaCantidad}x {currentPlant}");

        // RQF42 y RQF42.1: Si es perenne, replantarla automáticamente
        if (currentSeedCycle == SemillaCiclo.Perenne)
        {
            // Reiniciar el ciclo sin consumir energía ni semilla adicional
            isReady = false;
            plantedTime = DateTime.Now;

            // Recalcular tiempo con la energía actual
            float baseTime = data.tiempoCrecimientoMinutos * 60;
            growthTimeSeconds = BarraEnergiaSistema.Instance.GetModifiedGrowthTime(baseTime);

            if (harvestButton != null)
                harvestButton.gameObject.SetActive(false);

            if (plantImage != null)
                plantImage.color = normalColor;

            if (readyParticles != null)
                readyParticles.SetActive(false);

            // Consumir energía nuevamente para la nueva planta
            BarraEnergiaSistema.Instance.ConsumeEnergy(data.energiaConsumo);

            Debug.Log($"Planta perenne replantada automáticamente");
        }
        else
        {
            // Limpiar slot completamente
            isOccupied = false;
            isReady = false;
            currentPlant = PlantaTipo.Lumina; // Valor por defecto
        }

        UpdateSlotVisuals();
        SaveSlotState();
    }

    /// <summary>
    /// Verifica si la planta está lista para cosechar
    /// </summary>
    public bool IsReady()
    {
        return isReady;
    }

    /// <summary>
    /// RQF37: Actualiza la visualización del slot
    /// </summary>
    void UpdateSlotVisuals()
    {
        // Actualizar imagen de la planta
        if (plantImage != null)
        {
            if (isOccupied)
            {
                PlantData data = InvernaderoManager.Instance.plantDatabase.GetPlantas(currentPlant);
                if (data != null)
                {
                    plantImage.sprite = data.plantaSprite;
                    plantImage.enabled = true;
                    plantImage.color = isReady ? readyColor : normalColor;
                }
            }
            else
            {
                plantImage.enabled = false;
            }
        }

        // Actualizar sprite del slot de fondo
        if (slotImage != null && emptySlotSprite != null)
        {
            if (!isOccupied)
            {
                slotImage.sprite = emptySlotSprite;
            }
            else if (isReady && readyHighlightSprite != null)
            {
                slotImage.sprite = readyHighlightSprite;
            }
        }

        // Mostrar/ocultar panel de temporizador
        if (timerPanel != null)
        {
            timerPanel.SetActive(isOccupied && !isReady);
        }

        // Mostrar/ocultar botón de cosechar
        if (harvestButton != null)
        {
            harvestButton.gameObject.SetActive(isReady);
        }
    }

    #region GUARDADO/CARGA

    void SaveSlotState()
    {
        string key = $"PlantSlot_{gameObject.GetInstanceID()}";
        PlayerPrefs.SetInt($"{key}_Occupied", isOccupied ? 1 : 0);
        PlayerPrefs.SetInt($"{key}_Ready", isReady ? 1 : 0);
        PlayerPrefs.SetInt($"{key}_PlantType", (int)currentPlant);
        PlayerPrefs.SetInt($"{key}_SeedCycle", (int)currentSeedCycle);
        PlayerPrefs.SetString($"{key}_PlantedTime", plantedTime.ToString("o")); // ISO 8601
        PlayerPrefs.SetFloat($"{key}_GrowthTime", growthTimeSeconds);
        PlayerPrefs.Save();
    }

    void LoadSlotState()
    {
        string key = $"PlantSlot_{gameObject.GetInstanceID()}";

        if (PlayerPrefs.HasKey($"{key}_Occupied"))
        {
            isOccupied = PlayerPrefs.GetInt($"{key}_Occupied") == 1;
            isReady = PlayerPrefs.GetInt($"{key}_Ready") == 1;
            currentPlant = (PlantaTipo)PlayerPrefs.GetInt($"{key}_PlantType");
            currentSeedCycle = (SemillaCiclo)PlayerPrefs.GetInt($"{key}_SeedCycle");

            string timeString = PlayerPrefs.GetString($"{key}_PlantedTime");
            if (!string.IsNullOrEmpty(timeString))
            {
                try
                {
                    plantedTime = DateTime.Parse(timeString);
                }
                catch
                {
                    Debug.LogWarning($"No se pudo parsear la fecha guardada: {timeString}");
                    plantedTime = DateTime.Now;
                }
            }

            growthTimeSeconds = PlayerPrefs.GetInt($"{key}_GrowthTime");

            UpdateSlotVisuals();

            Debug.Log($"Slot cargado: {currentPlant} - Ocupado: {isOccupied} - Listo: {isReady}");
        }
    }

    #endregion

    #region DEBUGGING

    [ContextMenu("Completar Crecimiento Instantáneo")]
    public void CompleteGrowthInstantly()
    {
        if (isOccupied && !isReady)
        {
            plantedTime = DateTime.Now.AddSeconds(-growthTimeSeconds);
            Debug.Log("Crecimiento completado instantáneamente");
        }
    }

    [ContextMenu("Limpiar Slot")]
    public void ClearSlot()
    {
        isOccupied = false;
        isReady = false;
        UpdateSlotVisuals();
        SaveSlotState();
        Debug.Log("Slot limpiado");
    }

    #endregion
}