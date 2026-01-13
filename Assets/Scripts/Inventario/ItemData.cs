using UnityEngine;

[System.Serializable]
public abstract class ItemData
{
    [Header("Identificacion")]
    public string nombre;
    public string descripcion = "(Sin descripción)";

    public abstract Sprite GetBaseSprite { get; }

    [Header("Venta")]
    public int precioVentaEstandar;
    public int precioVentaPlata;
    public int precioVentaOro;

    [Header("Compra")]
    public int precioCompraEstandar;
    public int precioCompraPlata;
    public int precioCompraOro;

    [Header("Otros")]
    public float widthModifier = 1f;
    public float heightModifier = 1f;
}