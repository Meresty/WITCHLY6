using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Witchly.Mercado;

public class BuzonController : MonoBehaviour
{
    private enum BuzonStage { Etapa1, Etapa2, PostGame }

    [Header("Slots de sobres en pantalla")]
    public CartaSlotUI slot1;
    public CartaSlotUI slot2;
    public CartaSlotUI slot3;

    [Header("Panel de detalle de carta")]
    public GameObject panelPociones;
    public GameObject backResCarta;
    public GameObject panelDetalle;
    public TMP_Text txtTituloCarta;
    public TMP_Text txtCuerpoCarta;
    public TMP_Text txtCalidadCarta;
    public TMP_Text txtResultadoCarta;
    public Button btnElegirPocion;
    public Button btnCerrar;

    [Header("Economia")]
    public WalletFirebase playerWallet;

    [Header("Recetas especiales desbloqueadas (cartas 28-30)")]
    public bool recetaRupturaDesbloqueada;
    public bool recetaAmorDesbloqueada;
    public bool recetaGuardianDesbloqueada;

    [Header("DEBUG (opcional)")]
    public Button btnSiguienteRondaDebug;

    private List<CartaData> cartasEtapa1;
    private List<CartaData> cartasEtapa2;
    private List<CartaData> cartasEspeciales;

    private List<CartaData> poolDisponibles;
    private List<CartaData> poolExcluidas;

    private List<CartaData> cartasRondaActual;
    private CartaData cartaActual;

    private HashSet<int> cartasCompletadas;
    private int cartasResueltasEnRonda = 0;
    private BuzonStage estadoActual = BuzonStage.Etapa1;

    private void Awake()
    {
        if (panelDetalle != null) panelDetalle.SetActive(false);
        if (panelPociones != null) panelPociones.SetActive(false);
    }

    private void Start()
    {
        cartasEtapa1 = CartasConfig.CrearCartasEtapa1();
        cartasEtapa2 = CartasConfig.CrearCartasEtapa2();
        cartasEspeciales = CartasConfig.CrearCartasEspeciales();

        cartasCompletadas = new HashSet<int>();

        poolDisponibles = new List<CartaData>(cartasEtapa1);
        poolExcluidas = new List<CartaData>();
        cartasRondaActual = new List<CartaData>(3);

        estadoActual = BuzonStage.Etapa1;

        ConfigurarBotonesPanel();
        ResolverWallet();
        CrearRondaIntro();

        if (btnSiguienteRondaDebug != null)
        {
            btnSiguienteRondaDebug.onClick.RemoveAllListeners();
            btnSiguienteRondaDebug.onClick.AddListener(TerminarRondaYCrearOtra);
        }
    }

    private void ResolverWallet()
    {
        if (playerWallet != null) return;

        if (WalletFirebase.Instance != null) playerWallet = WalletFirebase.Instance;
        else playerWallet = FindObjectOfType<WalletFirebase>();

        if (playerWallet == null)
            Debug.LogError("[BuzonController] WalletFirebase not found!");
    }

    private void ConfigurarBotonesPanel()
    {
        if (btnCerrar != null)
        {
            btnCerrar.onClick.RemoveAllListeners();
            btnCerrar.onClick.AddListener(() =>
            {
                if (panelDetalle != null) panelDetalle.SetActive(false);
                if (panelPociones != null) panelPociones.SetActive(false);
            });
        }

        if (btnElegirPocion != null)
        {
            btnElegirPocion.onClick.RemoveAllListeners();
            btnElegirPocion.onClick.AddListener(OnElegirPocion);
        }
    }

    // ✅ esto lo llamas desde el boton de cada pocion (OnClick)
    public void GetNamePocion(string nombrePocion)
    {
        // safety: si ya no esta desbloqueada, no permitas entregar
        if (!BuzonProgress.IsUnlocked(nombrePocion))
        {
            Debug.LogWarning("[Buzon] Esa pocion no esta disponible (ya se entrego o no se creo).");
            return;
        }

        ProcesarEntrega(nombrePocion, PotionQuality.Estandar);
    }

    public void MostrarCartaEnPanel(CartaData carta)
    {
        if (carta == null) return;

        cartaActual = carta;
        cartasResueltasEnRonda = Mathf.Clamp(cartasResueltasEnRonda, 0, cartasRondaActual.Count);

        if (panelDetalle != null) panelDetalle.SetActive(true);

        if (txtTituloCarta != null) txtTituloCarta.text = carta.titulo;
        if (txtCuerpoCarta != null) txtCuerpoCarta.text = carta.textoCarta;
        if (txtCalidadCarta != null) txtCalidadCarta.text = $"Calidad requerida: {carta.calidad}";

        if (backResCarta != null) backResCarta.SetActive(false);
        if (txtResultadoCarta != null) txtResultadoCarta.text = "";

        // ✅ si ya se completo esa carta: ocultar Elegir Pocion
        bool yaEntregada = cartasCompletadas.Contains(carta.numero);
        if (btnElegirPocion != null) btnElegirPocion.gameObject.SetActive(!yaEntregada);
    }

    private void OnElegirPocion()
    {
        if (cartaActual == null) return;

        // ✅ refresca gates SIEMPRE antes de mostrar
        RefreshGates();

        if (panelPociones != null) panelPociones.SetActive(true);
        Debug.Log($"[Buzon] Abrir inventario para carta #{cartaActual.numero} ({cartaActual.pocionRequerida}).");
    }

    private void RefreshGates()
    {
        if (panelPociones == null) return;

        foreach (var gate in panelPociones.GetComponentsInChildren<BuzonPotionGate>(true))
            gate.Refresh();
    }

    public void ProcesarEntrega(string pocionEntregada, PotionQuality calidadEntregada)
    {
        if (cartaActual == null)
        {
            Debug.LogWarning("[Buzon] No hay carta activa al procesar entrega.");
            return;
        }

        // ✅ 1) consumir la pocion (ya no se puede volver a entregar)
        BuzonProgress.ConsumePotion(pocionEntregada);

        // cierra menu de pociones
        if (panelPociones != null) panelPociones.SetActive(false);

        // ✅ 2) refrescar gates (por si reabres el menu luego)
        RefreshGates();

        // recompensa
        string mensaje;
        int recompensa = CalcularRecompensa(cartaActual, pocionEntregada, calidadEntregada, out mensaje);

        if (playerWallet != null) playerWallet.AddCoins(recompensa);

        // UI resultado
        if (txtResultadoCarta != null)
        {
            if (backResCarta != null) backResCarta.SetActive(true);
            txtResultadoCarta.text = $"{mensaje}\n\nRecompensa: {recompensa} monedas.";
        }

        // ✅ 3) marcar carta como completada y ocultar boton elegir pocion
        MarcarCartaCompletada(cartaActual);
        if (btnElegirPocion != null) btnElegirPocion.gameObject.SetActive(false);

        cartasResueltasEnRonda++;
        if (cartasResueltasEnRonda >= cartasRondaActual.Count)
            TerminarRondaYCrearOtra();
    }

    private int CalcularRecompensa(CartaData carta, string pocionEntregada, PotionQuality calidadEntregada, out string mensaje)
    {
        if (pocionEntregada != carta.pocionRequerida)
        {
            mensaje =
                $"La pocion entregada NO era la correcta.\n" +
                $"El cliente pidio: {carta.pocionRequerida}.\n" +
                $"Solo recibes 10 monedas.";
            return 10;
        }

        int basePrice = carta.recompensaBase;

        if ((int)calidadEntregada < (int)carta.calidad)
        {
            int penalizacion = Mathf.RoundToInt(basePrice * 0.2f);
            int recompensa = Mathf.Max(0, basePrice - penalizacion);

            mensaje =
                $"Entregaste la pocion correcta ({carta.pocionRequerida}), " +
                $"pero con calidad {calidadEntregada}, inferior a la requerida ({carta.calidad}).\n" +
                $"Penalizacion 20% ({penalizacion} monedas).";

            return recompensa;
        }
        else if ((int)calidadEntregada == (int)carta.calidad)
        {
            int recompensa = CalcularRecompensaConBonus(basePrice, carta.calidad);

            mensaje =
                $"Entrega correcta :)\n" +
                $"Pocion: {carta.pocionRequerida}\n" +
                $"Calidad: {calidadEntregada}";

            return recompensa;
        }
        else
        {
            mensaje =
                $"Entregaste calidad superior ({calidadEntregada}) a la requerida ({carta.calidad}).\n" +
                $"Sin penalizacion ni bono extra.";

            return basePrice;
        }
    }

    private int CalcularRecompensaConBonus(int basePrice, PotionQuality calidadRequerida)
    {
        float multiplicador = 1f;
        switch (calidadRequerida)
        {
            case PotionQuality.Estandar: multiplicador = 1f; break;
            case PotionQuality.Plata: multiplicador = 1.2f; break;
            case PotionQuality.Oro: multiplicador = 1.4f; break;
        }
        return Mathf.RoundToInt(basePrice * multiplicador);
    }

    private void CrearRondaIntro()
    {
        cartasRondaActual = new List<CartaData>();
        cartasResueltasEnRonda = 0;

        CartaData cartaValor = cartasEtapa1.Find(c => c.pocionRequerida == CartasConfig.POCION_VALOR_INFALIBLE);
        CartaData cartaRevitalizante = cartasEtapa1.Find(c => c.pocionRequerida == CartasConfig.REVITALIZANTE);
        CartaData cartaMetamorfica = cartasEtapa1.Find(c => c.pocionRequerida == CartasConfig.POCION_METAMORFICA);

        if (cartaValor != null) cartasRondaActual.Add(cartaValor);
        if (cartaRevitalizante != null) cartasRondaActual.Add(cartaRevitalizante);
        if (cartaMetamorfica != null) cartasRondaActual.Add(cartaMetamorfica);

        foreach (var carta in cartasRondaActual) poolDisponibles.Remove(carta);

        slot1.Configurar(this, cartasRondaActual.Count > 0 ? cartasRondaActual[0] : null);
        slot2.Configurar(this, cartasRondaActual.Count > 1 ? cartasRondaActual[1] : null);
        slot3.Configurar(this, cartasRondaActual.Count > 2 ? cartasRondaActual[2] : null);
    }

    private void CrearNuevaRonda()
    {
        cartasResueltasEnRonda = 0;

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

        int idxQueRegresa = Random.Range(0, cartasRondaActual.Count);

        for (int i = 0; i < cartasRondaActual.Count; i++)
        {
            var carta = cartasRondaActual[i];
            if (i == idxQueRegresa) poolDisponibles.Add(carta);
            else poolExcluidas.Add(carta);
        }

        cartasRondaActual.Clear();
        CrearNuevaRonda();
    }

    private void MarcarCartaCompletada(CartaData carta)
    {
        if (carta == null) return;

        if (!cartasCompletadas.Contains(carta.numero))
            cartasCompletadas.Add(carta.numero);

        if (estadoActual == BuzonStage.Etapa1 && TodasCartasEtapa1Completas())
        {
            estadoActual = BuzonStage.Etapa2;
            ReconstruirPoolEtapa2();
        }
        else if (estadoActual == BuzonStage.Etapa2 && TodasCartasEtapa1Completas() && TodasCartasEtapa2Completas())
        {
            estadoActual = BuzonStage.PostGame;
            ReconstruirPoolPostGame();
        }
    }

    private bool TodasCartasEtapa1Completas()
    {
        for (int i = 1; i <= 12; i++)
            if (!cartasCompletadas.Contains(i)) return false;
        return true;
    }

    private bool TodasCartasEtapa2Completas()
    {
        for (int i = 13; i <= 27; i++)
            if (!cartasCompletadas.Contains(i)) return false;
        return true;
    }

    private void ReconstruirPoolEtapa2()
    {
        poolDisponibles = new List<CartaData>();
        poolExcluidas = new List<CartaData>();

        foreach (var carta in cartasEtapa2)
            if (CartaEsPermitidaPorRecetas(carta))
                poolDisponibles.Add(carta);
    }

    private void ReconstruirPoolPostGame()
    {
        poolDisponibles = new List<CartaData>();
        poolExcluidas = new List<CartaData>();
        poolDisponibles.AddRange(cartasEtapa1);
        poolDisponibles.AddRange(cartasEtapa2);
    }

    private bool CartaEsPermitidaPorRecetas(CartaData carta)
    {
        if (carta == null) return false;

        string p = carta.pocionRequerida;
        if (p == CartasConfig.POCION_RUPTURA && !recetaRupturaDesbloqueada) return false;
        if (p == CartasConfig.POCION_AMOR && !recetaAmorDesbloqueada) return false;
        if (p == CartasConfig.BREBAJE_GUARDIAN && !recetaGuardianDesbloqueada) return false;

        return true;
    }
}
