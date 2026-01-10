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
    public string titulo;            
    [TextArea(4, 12)]
    public string textoCarta;       

    [Header("Requisito de poción")]
    public string pocionRequerida;   
    public PotionQuality calidad;    

    [Header("Recompensas")]
    public int recompensaBase;       



    [Header("Metadatos")]
    public CartaStage etapa;        
}
