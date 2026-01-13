
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabsManager : MonoBehaviour
{
    public static TabsManager Instance { get; private set; }

    public GameObject semillasTabContent;
    public GameObject plantasTabContent;
    public GameObject suerosTabContent;

    public Button plantarButton;
    public TMP_Text plantarButtonText;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ShowSemillasTab();
    }

    public void ShowSemillasTab()
    {
        semillasTabContent.SetActive(true);
        plantasTabContent.SetActive(false);
        suerosTabContent.SetActive(false);
        plantarButton.gameObject.SetActive(true);
    }

    public void ShowPlantasTab()
    {
        semillasTabContent.SetActive(false);
        plantasTabContent.SetActive(true);
        suerosTabContent.SetActive(false);
        plantarButton.gameObject.SetActive(false);
    }

    public void ShowSuerosTab()
    {
        semillasTabContent.SetActive(false);
        plantasTabContent.SetActive(false);
        suerosTabContent.SetActive(true);
        plantarButton.gameObject.SetActive(false);
    }
}