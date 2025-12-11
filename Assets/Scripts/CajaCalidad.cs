using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CajaCalidad : MonoBehaviour
{
    [Header("Referencias UI")]
    public Button cajaBtn;
    public Image cajaImg;
    public TextMeshProUGUI timerTxt;
    public GameObject lockedOverlay;
    public GameObject timerPanel;
    public Sprite bloqueadoSprite;
    public Sprite desbloqueadoSprite;
    public Sprite cargandoSprite;

    [Header("Estado")]
    public int cajaIndex; 
    public bool isDesbloqueado = false;
    public bool isCargando = false;

    private PlantaTipo actualPlantaTipo;
    private PlantaCalidad inputQuality;
    private PlantaCalidad outputQuality;
    private DateTime startTime;
    private int processingTimeSeconds;

    void Start()
    {
        if (cajaBtn != null)
        {
            cajaBtn.onClick.AddListener(OnBoxClicked);
        }

        CargarCaja();
        ActualizarSprites();
    }

    void Update()
    {
        if (isCargando)
        {
            UpdateTimer();
        }
    }

    public void DesbloquearCaja()
    {
        if (!isDesbloqueado)
        {
            isDesbloqueado = true;
            SaveBoxState();
            ActualizarSprites();
            Debug.Log($"Caja {cajaIndex} desbloqueada");
        }
    }

    void OnBoxClicked()
    {
        if (!isDesbloqueado)
        {
            Debug.Log("Esta caja está bloqueada");
            return;
        }

        if (isCargando)
        {
            Debug.Log("La caja está procesando");
            return;
        }


        CajaCalidadUI.Instance?.OpenBoxSelection(this);
    }

    public bool StartProcessing(PlantaTipo plantaTipo, PlantaCalidad quality)
    {
        if (isCargando)
        {
            Debug.Log("La caja ya está procesando");
            return false;
        }


        if (!InventorySystem.Instance.HasPlant(plantaTipo, quality, 2))
        {
            Debug.Log("Necesitas 2 plantas de la misma calidad");
            return false;
        }


        if (quality == PlantaCalidad.Oro)
        {
            Debug.Log("Las plantas de calidad Oro no pueden mejorarse más");
            return false;
        }


        if (!InventorySystem.Instance.RemovePlant(plantaTipo, quality, 2))
        {
            return false;
        }


        actualPlantaTipo = plantaTipo;
        inputQuality = quality;
        outputQuality = quality == PlantaCalidad.Estandar ? PlantaCalidad.Plata : PlantaCalidad.Oro;
        isCargando = true;
        startTime = DateTime.Now;


        int baseTime = quality == PlantaCalidad.Estandar ? 180 : 300;


        processingTimeSeconds = BarraEnergiaSistema.Instance.GetModifiedQualityBoxTime(baseTime);

        SaveBoxState();
        ActualizarSprites();

        Debug.Log($"Procesando {plantaTipo} de {quality} a {outputQuality} - Tiempo: {processingTimeSeconds}s");
        return true;
    }

    void UpdateTimer()
    {
        TimeSpan elapsed = DateTime.Now - startTime;
        int remainingSeconds = processingTimeSeconds - (int)elapsed.TotalSeconds;

        if (remainingSeconds <= 0)
        {
            CompleteProcessing();
        }
        else
        {
            int minutes = remainingSeconds / 60;
            int seconds = remainingSeconds % 60;

            if (timerTxt != null)
            {
                timerTxt.text = $"{minutes:D2}:{seconds:D2}";
            }
        }
    }

    void CompleteProcessing()
    {
        if (!isCargando)
            return;

        // Añadir planta mejorada al inventario
        InventorySystem.Instance.AddPlant(actualPlantaTipo, outputQuality, 1);

        // Resetear estado
        isCargando = false;

        SaveBoxState();
        ActualizarSprites();

        Debug.Log($"¡Planta mejorada a {outputQuality}!");

        // Mostrar notificación (opcional)
        // NotificationSystem.Instance?.ShowNotification($"¡Planta {currentPlantType} mejorada a {outputQuality}!");
    }

    void ActualizarSprites()
    {
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!isDesbloqueado);
        }

        if (cajaImg != null)
        {
            if (!isDesbloqueado)
            {
                cajaImg.sprite = bloqueadoSprite;
            }
            else if (isCargando)
            {
                cajaImg.sprite = cargandoSprite;
            }
            else
            {
                cajaImg.sprite = desbloqueadoSprite;
            }
        }

        if (timerPanel != null)
        {
            timerPanel.SetActive(isCargando);
        }

        if (cajaBtn != null)
        {
            cajaBtn.interactable = isDesbloqueado && !isCargando;
        }
    }

    void SaveBoxState()
    {
        string key = $"QualityBox_{cajaIndex}";
        PlayerPrefs.SetInt($"{key}_Unlocked", isDesbloqueado ? 1 : 0);
        PlayerPrefs.SetInt($"{key}_Processing", isCargando ? 1 : 0);

        if (isCargando)
        {
            PlayerPrefs.SetInt($"{key}_PlantType", (int)actualPlantaTipo);
            PlayerPrefs.SetInt($"{key}_InputQuality", (int)inputQuality);
            PlayerPrefs.SetInt($"{key}_OutputQuality", (int)outputQuality);
            PlayerPrefs.SetString($"{key}_StartTime", startTime.ToString());
            PlayerPrefs.SetInt($"{key}_ProcessTime", processingTimeSeconds);
        }

        PlayerPrefs.Save();
    }

    void CargarCaja()
    {
        string key = $"QualityBox_{cajaIndex}";

        // La primera caja se desbloquea después de 12 cartas
        // Por ahora, para testing, puedes activarla manualmente
        if (cajaIndex == 0 && !PlayerPrefs.HasKey($"{key}_Unlocked"))
        {
            // Verificar si se han completado 12 cartas
            // int completedCards = CardSystem.Instance.GetCompletedCards();
            // isUnlocked = completedCards >= 12;

            // Para testing:
            isDesbloqueado = false; // Cambiar a true para testing
        }
        else
        {
            isDesbloqueado = PlayerPrefs.GetInt($"{key}_Unlocked", 0) == 1;
        }

        isCargando = PlayerPrefs.GetInt($"{key}_Processing", 0) == 1;

        if (isCargando)
        {
            actualPlantaTipo = (PlantaTipo)PlayerPrefs.GetInt($"{key}_PlantType");
            inputQuality = (PlantaCalidad)PlayerPrefs.GetInt($"{key}_InputQuality");
            outputQuality = (PlantaCalidad)PlayerPrefs.GetInt($"{key}_OutputQuality");
            string timeString = PlayerPrefs.GetString($"{key}_StartTime");
            if (!string.IsNullOrEmpty(timeString))
            {
                startTime = DateTime.Parse(timeString);
            }
            processingTimeSeconds = PlayerPrefs.GetInt($"{key}_ProcessTime");
        }
    }

    [ContextMenu("Unlock This Box")]
    public void UnlockForTesting()
    {
        DesbloquearCaja();
    }

    [ContextMenu("Complete Processing Now")]
    public void CompleteNow()
    {
        if (isCargando)
        {
            CompleteProcessing();
        }
    }
}