using UnityEngine;

public class AchievementsPanelController : MonoBehaviour
{
    [Header("Panel principal")]
    public GameObject panelLogros;

    public void Open()
    {
        if (panelLogros != null) panelLogros.SetActive(true);
    }

    public void Close()
    {
        if (panelLogros != null) panelLogros.SetActive(false);
    }

    public void Toggle()
    {
        if (panelLogros != null) panelLogros.SetActive(!panelLogros.activeSelf);
    }
}
