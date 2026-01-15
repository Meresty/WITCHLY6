using UnityEngine;

public class SceneProperties : MonoBehaviour
{
    public static SceneProperties Instance { get; private set; }

    public bool showEnergyBar = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (BarraEnergiaSistema.Instance != null)
        {
            BarraEnergiaSistema.Instance.gameObject.SetActive(showEnergyBar);
        }
    }
}