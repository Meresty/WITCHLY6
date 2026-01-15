using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum  PlantaTipo
{
    Lumina,
    Falsibaya,
    Drakonia,
    Eldebria,
    Jiveria,
    Lirien,
    NONE = -1,
}

[System.Serializable]
public enum PlantaCalidad
{
    NONE,
    Estandar,
    Plata,
    Oro
}

[System.Serializable]
public enum SemillaCiclo
{
    Replantar,
    Perenne
}

[System.Serializable]
public class PlantData : ItemData
{
    [Header("Identificacion")]
    public PlantaTipo plantaTipo;

    public override Sprite GetBaseSprite => plantaSprite;

    [Header("Visuals")]
    public Sprite plantaSprite;
    public Sprite semillaSprite;
    public Sprite frutoSprite;

    [Header("Crecimiento")]
    public SemillaCiclo semillaCiclo;
    public float tiempoCrecimientoMinutos;
    public int energiaConsumo;
    public int cosechaCantidad;
}
