using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Witchly.Mercado;

public class BuzonController : MonoBehaviour
{
    // Estado interno del buzón
    private enum BuzonStage
    {
        Etapa1,
        Etapa2,
        PostGame
    }

    [Header("Slots de sobres en pantalla")]
    public CartaSlotUI slot1;
    public CartaSlotUI slot2;
    public CartaSlotUI slot3;

    [Header("Panel de detalle de carta")]
    public GameObject panelDetalle;
    public TMP_Text txtTituloCarta;
    public TMP_Text txtCuerpoCarta;
    public TMP_Text txtCalidadCarta;
    public TMP_Text txtResultadoCarta;
    public Button btnElegirPocion;   // Abre inventario
    public Button btnCerrar;         // Cierra panel

    [Header("Economía")]
    public PlayerWallet playerWallet;

    [Header("Recetas especiales desbloqueadas (cartas 28-30)")]
    [Tooltip("Se pone en true cuando compras la carta especial de Poción de Ruptura")]
    public bool recetaRupturaDesbloqueada;
    [Tooltip("Se pone en true cuando compras la carta especial de Poción de Amor")]
    public bool recetaAmorDesbloqueada;
    [Tooltip("Se pone en true cuando compras la carta especial de Brebaje de Guardián")]
    public bool recetaGuardianDesbloqueada;

    [Header("DEBUG (opcional)")]
    public Button btnSiguienteRondaDebug;

    // ====== ESTADO INTERNO ======
    private List<CartaData> cartasEtapa1;
    private List<CartaData> cartasEtapa2;
    private List<CartaData> cartasEspeciales; // por si las necesitas en otro script

    // Pool actual (depende de la etapa del juego)
    private List<CartaData> poolDisponibles;   // cartas que pueden salir
    private List<CartaData> poolExcluidas;     // las 2 que se sacan temporalmente

    private List<CartaData> cartasRondaActual; // las 3 cartas visibles
    private CartaData cartaActual;             // carta abierta en el panel

    private HashSet<int> cartasCompletadas;    // por numero de carta (1-27)
    private int cartasResueltasEnRonda = 0;

    private BuzonStage estadoActual = BuzonStage.Etapa1;

    // --------------------------------------------------
    // Ciclo de vida
    // --------------------------------------------------

    private void Awake()
    {
        if (panelDetalle != null)
            panelDetalle.SetActive(false);
    }

    private void Start()
    {
        // 1) Cargar TODAS las cartas
        cartasEtapa1 = CartasConfig.CrearCartasEtapa1();      // 1–12
        cartasEtapa2 = CartasConfig.CrearCartasEtapa2();      // 13–27
        cartasEspeciales = CartasConfig.CrearCartasEspeciales(); // 28–30 (recetas)

        cartasCompletadas = new HashSet<int>();

        // 2) Inicializar pool con TODAS las de etapa 1
        poolDisponibles = new List<CartaData>(cartasEtapa1);
        poolExcluidas = new List<CartaData>();
        cartasRondaActual = new List<CartaData>(3);

        estadoActual = BuzonStage.Etapa1;

        // 3) Botones del panel
        ConfigurarBotonesPanel();

        // 4) Primera ronda SIEMPRE = intro (Valor, Revitalizante, Metamórfica)
        CrearRondaIntro();

        // 5) Botón de debug para avanzar ronda manualmente (si lo quieres)
        if (btnSiguienteRondaDebug != null)
        {
            btnSiguienteRondaDebug.onClick.RemoveAllListeners();
            btnSiguienteRondaDebug.onClick.AddListener(TerminarRondaYCrearOtra);
        }
    }

    // --------------------------------------------------
    // Configuración de botones de panel
    // --------------------------------------------------

    private void ConfigurarBotonesPanel()
    {
        if (btnCerrar != null)
        {
            btnCerrar.onClick.RemoveAllListeners();
            btnCerrar.onClick.AddListener(() =>
            {
                if (panelDetalle != null)
                    panelDetalle.SetActive(false);
            });
        }

        if (btnElegirPocion != null)
        {
            btnElegirPocion.onClick.RemoveAllListeners();
            btnElegirPocion.onClick.AddListener(OnElegirPocion);
        }
    }

    // --------------------------------------------------
    // Mostrar carta en panel
    // --------------------------------------------------

    public void MostrarCartaEnPanel(CartaData carta)
    {
        if (carta == null) return;

        cartaActual = carta;
        cartasResueltasEnRonda = Mathf.Clamp(cartasResueltasEnRonda, 0, cartasRondaActual.Count);

        if (panelDetalle != null)
            panelDetalle.SetActive(true);

        if (txtTituloCarta != null)
            txtTituloCarta.text = carta.titulo;

        if (txtCuerpoCarta != null)
            txtCuerpoCarta.text = carta.textoCarta;

        if (txtCalidadCarta != null)
            txtCalidadCarta.text = $"Calidad requerida: {carta.calidad}";

        if (txtResultadoCarta != null)
            txtResultadoCarta.text = "";
    }

    // --------------------------------------------------
    // Botón "Elegir Poción" ? aquí solo abres el inventario
    // --------------------------------------------------

    private void OnElegirPocion()
    {
        if (cartaActual == null)
            return;

        // Aquí deberías llamar a tu sistema de inventario, algo como:
        // InventarioUI.Instance.AbrirInventarioParaCarta(cartaActual, this);
        Debug.Log($"[Buzon] Abrir inventario para carta #{cartaActual.numero} ({cartaActual.pocionRequerida}).");
    }

    // --------------------------------------------------
    // LÓGICA DE ENTREGA: se llama desde el inventario
    // --------------------------------------------------
    // Llama a este método cuando el jugador pulse "Entregar"
    // en el inventario, pasando la poción seleccionada.

    public void ProcesarEntrega(string pocionEntregada, PotionQuality calidadEntregada)
    {
        if (cartaActual == null)
        {
            Debug.LogWarning("[Buzon] No hay carta activa al procesar entrega.");
            return;
        }

        string mensaje;
        int recompensa = CalcularRecompensa(cartaActual, pocionEntregada, calidadEntregada, out mensaje);

        // Sumar monedas
        if (playerWallet != null)
        {
            playerWallet.AddCoins(recompensa);
        }
        else
        {
            Debug.LogWarning("[Buzon] No hay PlayerWallet asignado, no se suman monedas.");
        }

        // Mostrar resultado en la carta
        if (txtResultadoCarta != null)
        {
            txtResultadoCarta.text = $"{mensaje}\n\nRecompensa: {recompensa} monedas.";
        }

        // Marcar carta como completada
        MarcarCartaCompletada(cartaActual);

        // Contabilizar carta resuelta dentro de la ronda actual
        cartasResueltasEnRonda++;

        // Si ya se resolvieron las 3 cartas de la ronda, pasamos a la siguiente
        if (cartasResueltasEnRonda >= cartasRondaActual.Count)
        {
            TerminarRondaYCrearOtra();
        }
    }

    // --------------------------------------------------
    // Cálculo de recompensa / penalización
    // --------------------------------------------------

    private int CalcularRecompensa(
        CartaData carta,
        string pocionEntregada,
        PotionQuality calidadEntregada,
        out string mensaje)
    {
        // 1) POCIÓN INCORRECTA ? 10 monedas, revelar la correcta
        if (pocionEntregada != carta.pocionRequerida)
        {
            mensaje =
                $"La poción entregada NO era la correcta.\n" +
                $"El cliente pidió: {carta.pocionRequerida}.\n" +
                $"Solo recibes 10 monedas.";
            return 10;
        }

        // 2) POCIÓN CORRECTA ? comprobar calidad
        int basePrice = carta.recompensaBase;

        // calidadEntregada vs calidad requerida
        if ((int)calidadEntregada < (int)carta.calidad)
        {
            // Calidad insuficiente ? penalización 20% del precio
            int penalizacion = Mathf.RoundToInt(basePrice * 0.2f);
            int recompensa = Mathf.Max(0, basePrice - penalizacion);

            mensaje =
                $"Entregaste la poción correcta ({carta.pocionRequerida}), " +
                $"pero con calidad {calidadEntregada}, inferior a la requerida ({carta.calidad}).\n" +
                $"Se aplica una penalización del 20% del precio ({penalizacion} monedas).";

            return recompensa;
        }
        else if ((int)calidadEntregada == (int)carta.calidad)
        {
            // Calidad exacta ? recompensa completa + bonus por ser Plata/Oro
            int recompensa = CalcularRecompensaConBonus(basePrice, carta.calidad);

            mensaje =
                $"¡Entrega correcta!\n" +
                $"Poción: {carta.pocionRequerida}\n" +
                $"Calidad: {calidadEntregada} (justo lo que pidió el cliente).";

            return recompensa;
        }
        else
        {
            // Calidad superior a la requerida
            // ? NO hay penalización, pero TAMPOCO se paga el bono extra
            //    que tendrías por pociones de mayor calidad.
            int recompensa = basePrice;

            mensaje =
                $"Entregaste una calidad superior ({calidadEntregada}) a la requerida ({carta.calidad}).\n" +
                $"No hay penalización, pero tampoco recibes el bono extra por esa calidad superior.";

            return recompensa;
        }
    }

    // Bonus de calidad Plata/Oro (ajusta los porcentajes según tu tabla real)
    private int CalcularRecompensaConBonus(int basePrice, PotionQuality calidadRequerida)
    {
        float multiplicador = 1f;

        switch (calidadRequerida)
        {
            case PotionQuality.Estandar:
                multiplicador = 1f;    // sin bonus
                break;
            case PotionQuality.Plata:
                multiplicador = 1.2f;  // +20% (ejemplo)
                break;
            case PotionQuality.Oro:
                multiplicador = 1.4f;  // +40% (ejemplo)
                break;
        }

        return Mathf.RoundToInt(basePrice * multiplicador);
    }

    // --------------------------------------------------
    // RONDA INTRO (siempre la primera de Etapa 1)
    // --------------------------------------------------

    private void CrearRondaIntro()
    {
        cartasRondaActual = new List<CartaData>();
        cartasResueltasEnRonda = 0;

        // Buscar las 3 cartas intro en la Etapa 1
        CartaData cartaValor = cartasEtapa1.Find(
            c => c.pocionRequerida == CartasConfig.POCION_VALOR_INFALIBLE);
        CartaData cartaRevitalizante = cartasEtapa1.Find(
            c => c.pocionRequerida == CartasConfig.REVITALIZANTE);
        CartaData cartaMetamorfica = cartasEtapa1.Find(
            c => c.pocionRequerida == CartasConfig.POCION_METAMORFICA);

        if (cartaValor != null) cartasRondaActual.Add(cartaValor);
        if (cartaRevitalizante != null) cartasRondaActual.Add(cartaRevitalizante);
        if (cartaMetamorfica != null) cartasRondaActual.Add(cartaMetamorfica);

        // Quitarlas del pool disponible de Etapa1 para que no salgan en aleatorio
        foreach (var carta in cartasRondaActual)
        {
            poolDisponibles.Remove(carta);
        }

        // Asignar a slots
        slot1.Configurar(this, cartasRondaActual.Count > 0 ? cartasRondaActual[0] : null);
        slot2.Configurar(this, cartasRondaActual.Count > 1 ? cartasRondaActual[1] : null);
        slot3.Configurar(this, cartasRondaActual.Count > 2 ? cartasRondaActual[2] : null);
    }

    // --------------------------------------------------
    // SISTEMA DE RONDAS (Etapa1, Etapa2, PostGame)
    // --------------------------------------------------

    private void CrearNuevaRonda()
    {
        cartasResueltasEnRonda = 0;

        // Si el pool actual se quedó corto, reciclamos las excluidas
        if (poolDisponibles.Count < 3 && poolExcluidas.Count > 0)
        {
            poolDisponibles.AddRange(poolExcluidas);
            poolExcluidas.Clear();
        }

        cartasRondaActual = Seleccionar3CartasAleatorias();

        slot1.Configurar(this, cartasRondaActual.Count > 0 ? cartasRondaActual[0] : null);
        slot2.Configurar(this, cartasRondaActual.Count > 1 ? cartasRondaActual[1] : null);
        slot3.Configurar(this, cartasRondaActual.Count > 2 ? cartasRondaActual[2] : null);
    }

    private List<CartaData> Seleccionar3CartasAleatorias()
    {
        var seleccion = new List<CartaData>();
        int cantidad = Mathf.Min(3, poolDisponibles.Count);

        for (int i = 0; i < cantidad; i++)
        {
            int idx = Random.Range(0, poolDisponibles.Count);
            seleccion.Add(poolDisponibles[idx]);
            poolDisponibles.RemoveAt(idx);
        }

        return seleccion;
    }

    public void TerminarRondaYCrearOtra()
    {
        if (cartasRondaActual == null || cartasRondaActual.Count == 0)
        {
            CrearNuevaRonda();
            return;
        }

        // De las 3 cartas, 1 regresa al pool, 2 se van a excluidas temporalmente
        int idxQueRegresa = Random.Range(0, cartasRondaActual.Count);

        for (int i = 0; i < cartasRondaActual.Count; i++)
        {
            var carta = cartasRondaActual[i];

            if (i == idxQueRegresa)
            {
                poolDisponibles.Add(carta);
            }
            else
            {
                poolExcluidas.Add(carta);
            }
        }

        cartasRondaActual.Clear();

        // Nueva ronda según el estado actual
        CrearNuevaRonda();
    }

    // --------------------------------------------------
    // Progreso y cambio de etapa
    // --------------------------------------------------

    private void MarcarCartaCompletada(CartaData carta)
    {
        if (carta == null) return;

        if (!cartasCompletadas.Contains(carta.numero))
            cartasCompletadas.Add(carta.numero);

        // Cambio de etapa cuando se completan 1–12
        if (estadoActual == BuzonStage.Etapa1 && TodasCartasEtapa1Completas())
        {
            estadoActual = BuzonStage.Etapa2;
            ReconstruirPoolEtapa2();
            Debug.Log("[Buzon] Etapa 2 desbloqueada.");
        }
        // Cambio a PostGame cuando se completan TODAS las de Etapa1 y Etapa2 (1–27)
        else if (estadoActual == BuzonStage.Etapa2 &&
                 TodasCartasEtapa1Completas() &&
                 TodasCartasEtapa2Completas())
        {
            estadoActual = BuzonStage.PostGame;
            ReconstruirPoolPostGame();
            Debug.Log("[Buzon] Juego completado. Modo infinito (todas las cartas).");
        }
    }

    private bool TodasCartasEtapa1Completas()
    {
        for (int i = 1; i <= 12; i++)
        {
            if (!cartasCompletadas.Contains(i))
                return false;
        }
        return true;
    }

    private bool TodasCartasEtapa2Completas()
    {
        for (int i = 13; i <= 27; i++)
        {
            if (!cartasCompletadas.Contains(i))
                return false;
        }
        return true;
    }

    private void ReconstruirPoolEtapa2()
    {
        poolDisponibles = new List<CartaData>();
        poolExcluidas = new List<CartaData>();

        foreach (var carta in cartasEtapa2)
        {
            if (CartaEsPermitidaPorRecetas(carta))
            {
                poolDisponibles.Add(carta);
            }
        }
    }

    private void ReconstruirPoolPostGame()
    {
        poolDisponibles = new List<CartaData>();
        poolExcluidas = new List<CartaData>();

        // Todas las cartas de Etapa1 + Etapa2 vuelven al pool
        poolDisponibles.AddRange(cartasEtapa1);
        poolDisponibles.AddRange(cartasEtapa2);
    }

    private bool CartaEsPermitidaPorRecetas(CartaData carta)
    {
        if (carta == null) return false;

        string p = carta.pocionRequerida;

        if (p == CartasConfig.POCION_RUPTURA && !recetaRupturaDesbloqueada)
            return false;
        if (p == CartasConfig.POCION_AMOR && !recetaAmorDesbloqueada)
            return false;
        if (p == CartasConfig.BREBAJE_GUARDIAN && !recetaGuardianDesbloqueada)
            return false;

        return true;
    }

    // --------------------------------------------------
    // Métodos para desbloquear recetas desde el mercado
    // --------------------------------------------------

    public void DesbloquearRecetaRuptura()
    {
        recetaRupturaDesbloqueada = true;
        if (estadoActual == BuzonStage.Etapa2)
            ReconstruirPoolEtapa2();
    }

    public void DesbloquearRecetaAmor()
    {
        recetaAmorDesbloqueada = true;
        if (estadoActual == BuzonStage.Etapa2)
            ReconstruirPoolEtapa2();
    }

    public void DesbloquearRecetaGuardian()
    {
        recetaGuardianDesbloqueada = true;
        if (estadoActual == BuzonStage.Etapa2)
            ReconstruirPoolEtapa2();
    }
}
