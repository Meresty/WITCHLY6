using UnityEngine;

// TIPOS DE OBJETO DEL MERCADO
public enum TipoObjetoMercado
{
    Planta,
    Semilla,
    Suero,
    CartaEspecial // para las 3 cartas de 100 monedas
}

// SCRIPTABLEOBJECT PARA DEFINIR UN OBJETO DEL MERCADO
[CreateAssetMenu(menuName = "Witchly/Mercado/Objeto")]
public class DatosObjetoMercado : ScriptableObject
{
    [Header("Identidad")]
    public string id;              // opcional (LUMINA_SEMILLA, SUERO_FUERZA, etc.)
    public string nombreMostrado;  // lo que ve el jugador

    [Header("Datos de juego")]
    public TipoObjetoMercado tipo;
    public int precioBase;         // precio base del anexo

    [Header("Visual")]
    public Sprite icono;
}
