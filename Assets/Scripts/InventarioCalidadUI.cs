using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventarioCalidadUI : MonoBehaviour
{
    [Header("Caja destino (UI del panel)")]
    public CajaCalidadUI cajaUI;

    [Header("UI lista izquierda")]
    public Transform content;
    public GameObject itemPrefab;

    [Header("DB sprites")]
    public PlantaBD plantDB;

    private void OnEnable()
    {
        Refresh();
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged += Refresh;
    }

    private void OnDisable()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged -= Refresh;
    }

    public void Refresh()
    {
        if (content == null || itemPrefab == null) return;

        foreach (Transform child in content)
            Destroy(child.gameObject);

        if (InventorySystem.Instance == null) return;

        foreach (var p in InventorySystem.Instance.plantas)
        {
            if (p == null) continue;
            if (p.cantidad <= 0) continue;

            // solo estandar/plata
            if (p.calidad != PlantaCalidad.Estandar && p.calidad != PlantaCalidad.Plata)
                continue;

            GameObject go = Instantiate(itemPrefab, content);

            Button btn = go.GetComponent<Button>();
            TMP_Text text = go.GetComponentInChildren<TMP_Text>(true);
            Image img = go.GetComponentInChildren<Image>(true);

            if (text != null)
                text.text = p.plantaTipo + " (" + p.calidad + ") x" + p.cantidad;

            if (img != null)
            {
                Sprite spr = GetPlantSprite(p.plantaTipo);
                img.sprite = spr;
                img.enabled = (spr != null);
            }

            if (btn != null)
            {
                PlantaTipo tipo = p.plantaTipo;
                PlantaCalidad calidad = p.calidad;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (cajaUI != null)
                        cajaUI.OnSeleccionInventario(tipo, calidad);
                });
            }
        }
    }

    private Sprite GetPlantSprite(PlantaTipo tipo)
    {
        if (plantDB == null && InvernaderoManager.Instance != null)
            plantDB = InvernaderoManager.Instance.plantDatabase;

        if (plantDB == null) return null;

        PlantData data = plantDB.GetPlantas(tipo);
        return (data != null) ? data.frutoSprite : null;
    }
}
