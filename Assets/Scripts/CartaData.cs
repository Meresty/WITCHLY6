using UnityEngine;

public enum CartaStage
{
    Etapa1,
    Etapa2,
    Especial // 27–30
}

public enum PotionQuality
{
    Estandar,
    Plata,
    Oro
}

[System.Serializable]
public class CartaData
{
    [Header("Identificación")]
    public int numero;               // 1–30

    [Header("Contenido de la carta")]
    public string titulo;            // Título corto
    [TextArea(4, 12)]
    public string textoCarta;        // Texto completo

    [Header("Requisito de poción")]
    public string pocionRequerida;   // Nombre de la poción
    public PotionQuality calidad;    // Estandar / Plata / Oro

    [Header("Recompensas")]
    public int recompensaBase;       // Precio de venta base

    [Header("Metadatos")]
    public CartaStage etapa;         // Etapa1, Etapa2, Especial
}
