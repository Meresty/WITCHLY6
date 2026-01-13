
using UnityEngine;

[System.Serializable]
public class ItemInfo
{
    public string itemNombre;
    public string itemDescripcion = "";
    public Sprite icon;
    public PlantaCalidad calidad = PlantaCalidad.NONE;
    public float widthModifier = 1f;
    public float heightModifier = 1f;
}