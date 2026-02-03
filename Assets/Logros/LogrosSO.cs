using UnityEngine;

public enum LogroTipo//AchievementType
{
    PrimeraPocion, //FirstPotionCorrect
    Cosecha, // HarvestTotal
    CosechaTodasLasPlantas, // HarvestUniqueTypes
    EventoEstrellas, // StarsEventsCompleted
    EventoMeteoritos, // MeteorEventsCompleted
    Monedas, // CoinsReach
    Compras, // MarketPurchases
    CalidadPociones, // QualityPotions
    CartasAnonimas, // AnonymousCardsBought 
    CrearTodasLasPociones,// AllPotionsCrafted
    Cartas12Primeras, // CardsCompletedCount
    CompletarCartas, //AllCardsOnce
    CompletarCartas2, // AllCardsTwice
    CompletarLogros // AllAchievements
}

public enum RecompensaTipo //RewardType
{
    Monedas,
    DesbloquearCaja,   // “1 caja de aumento de calidad”
    Pocion      // pocion especial por premio
}

[CreateAssetMenu(fileName = "Logro", menuName = "Game/Logros/Logro")]
public class LogrosSO : ScriptableObject
{
    public string id; // unico, ej: "harvest_20"
    public string displayNombre;
    [TextArea] public string requerimientoTexto;
    public Sprite sprite;
    public LogroTipo logroTipo;
    public int targetValue = 1; // 1, 5, 10, 30, 1000, etc.
    public RecompensaTipo recompensaTipo;
    public int recompensaMonedas;
    public int recompensaCajas; // normalmente 1
    public ItemSO recompensaPocionSO; // si usas ItemSO para potions especiales
}
