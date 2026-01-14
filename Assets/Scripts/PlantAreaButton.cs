using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Botón de área de cultivo en la vista principal del invernadero
/// Muestra preview del estado y abre la vista detallada
/// </summary>
public class PlantAreaButton : MonoBehaviour
{
    [Header("Configuración")]
    public PlantaTipo areaPlantType;

    [Header("Referencias UI")]
    public Button areaButton;
    public Image areaIcon;
    public TextMeshProUGUI areaNameText;
    public TextMeshProUGUI statusText;
    public Image statusIndicator;

    [Header("Indicadores Visuales")]
    public Color emptyColor = new Color(0.5f, 0.5f, 0.5f); // Gris
    public Color growingColor = new Color(0.3f, 0.6f, 0.9f); // Azul
    public Color readyColor = new Color(1f, 0.84f, 0f); // Dorado

    private PlantData plantData;

    void Start()
    {
        if (areaButton != null)
        {
            areaButton.onClick.AddListener(OnAreaClicked);
        }

        // Obtener datos de la planta
        if (InvernaderoManager.Instance != null)
        {
            plantData = InvernaderoManager.Instance.plantDatabase.GetPlantas(areaPlantType);
        }

        // UpdateAreaDisplay();
    }

    void OnDestroy()
    {
        CancelInvoke();
    }

    /// <summary>
    /// Actualiza la visualización del área según su estado
    /// </summary>
    // void UpdateAreaDisplay()
    // {
    //     if (InvernaderoManager.Instance == null || plantData == null)
    //         return;

    //     // Obtener slots de esta área
    //     int activeSlots = InvernaderoManager.Instance.GetActiveSlots(areaPlantType);
    //     int readySlots = InvernaderoManager.Instance.GetReadySlots(areaPlantType);
    //     int totalSlots = 4;

    //     // Actualizar nombre
    //     if (areaNameText != null)
    //     {
    //         areaNameText.text = plantData.nombre;
    //     }

    //     // Actualizar icono
    //     if (areaIcon != null && plantData.plantaSprite != null)
    //     {
    //         areaIcon.sprite = plantData.plantaSprite;
    //     }

    //     // Actualizar texto de estado
    //     if (statusText != null)
    //     {
    //         if (activeSlots == 0)
    //         {
    //             statusText.text = "Vacío";
    //         }
    //         else if (readySlots > 0)
    //         {
    //             statusText.text = $"{readySlots}/{totalSlots} Listo";
    //         }
    //         else
    //         {
    //             statusText.text = $"{activeSlots}/{totalSlots} Creciendo";
    //         }
    //     }

    //     // Actualizar indicador de color
    //     if (statusIndicator != null)
    //     {
    //         if (activeSlots == 0)
    //         {
    //             statusIndicator.color = emptyColor;
    //         }
    //         else if (readySlots > 0)
    //         {
    //             statusIndicator.color = readyColor;
    //         }
    //         else
    //         {
    //             statusIndicator.color = growingColor;
    //         }
    //     }
    // }

    /// <summary>
    /// Abre la vista detallada de esta área
    /// </summary>
    void OnAreaClicked()
    {
        Debug.Log("Botón clickeado!");

        if (PlantAreaDetailView.Instance != null)
        {
            Debug.Log($"Instance encontrada {areaPlantType}");
            PlantAreaDetailView.Instance.OpenAreaDetail(areaPlantType);
        }
        else
        {
            Debug.LogError("PlantAreaDetailView no encontrado en la escena!");
        }
    }

    /// <summary>
    /// Fuerza una actualización inmediata
    /// </summary>
    // public void ForceUpdate()
    // {
    //     UpdateAreaDisplay();
    // }
}