using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuzonPotionGate : MonoBehaviour
{
    [Header("Referencia de la pocion")]
    public PocionSO pocion;

    [Header("UI (opcional)")]
    public Button button;
    public GameObject lockOverlay;         // imagen/candado encima
    public bool hideIfLocked = false;      // si true, se oculta completo si no esta creada

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        string nombre = (pocion != null) ? pocion.pocionNombre : "";
        bool unlocked = BuzonProgress.IsUnlocked(nombre);

        if (hideIfLocked)
        {
            gameObject.SetActive(unlocked);
            return;
        }

        if (button != null) button.interactable = unlocked;
        if (lockOverlay != null) lockOverlay.SetActive(!unlocked);
    }
}
