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
    Lirien
}

[System.Serializable]
public enum PlantaCalidad
{
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
public class PlantasInfo
{
    public PlantaTipo plantaTipo;
    public string plantaNombre;
    public Sprite plantaSprite;
    public Sprite semillaSprite;

    public SemillaCiclo semillaCiclo;
    public int semillaPrecio;
    public int tiempoCrecimientoMinutos; 

    public int energiaConsumo;
    public int cosechaCantidad;
    public int precioVentaEstandar;
    public int precioVentaPlata;
    public int precioVentaOro;


    public int precioCompraEstandar;
    public int precioCompraPlata;
    public int precioCompraOro;
}
