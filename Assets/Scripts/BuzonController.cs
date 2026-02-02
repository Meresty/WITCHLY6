using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Witchly.Mercado;

public class BuzonController : MonoBehaviour
{
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
    public GameObject backResCarta;

    [Header("Panel de pociones (MenuPociones)")]
    public GameObject panelPociones;

    [Header("Botones del panel detalle")]
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
        if (backResCarta != null) backResCarta.SetActive(false);
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

        // IMPORTANTISIMO: al iniciar refresca gates (por si ya tenias pociones creadas)
        RefreshPotionGates();

        if (btnSiguienteRondaDebug != null)
        {
            btnSiguienteRondaDebug.onClick.RemoveAllListeners();
            btnSiguienteRondaDebug.onClick.AddListener(TerminarRondaYCrearOtra);
        }
    }

    private void ResolverWallet()
    {
        if (playerWallet != null) return;

        if (WalletFirebase.Instance != null)
        {
            playerWallet = WalletFirebase.Instance;
            return;
        }

        playerWallet = FindObjectOfType<WalletFirebase>();

        if (playerWallet == null)
        {
            var go = new GameObject("WalletFirebase");
            playerWallet = go.AddComponent<WalletFirebase>();
        }
    }

    private void ConfigurarBotonesPanel()
    {
        if (btnCerrar != null)
        {
            btnCerrar.onClick.RemoveAllListeners();
            btnCerrar.onClick.AddListener(() =>
            {
                if (panelDetalle != null) panelDetalle.SetActive(false);
                if (backResCarta != null) backResCarta.SetActive(false);
            });
        }

        if (btnElegirPocion != null)
        {
            btnElegirPocion.onClick.RemoveAllListeners();
            btnElegirPocion.onClick.AddListener(OnElegirPocion);
        }
    }

    // ======= LO QUE TE FALTABA =======
    private void RefreshPotionGates()
    {
        if (panelPociones == null) return;

        foreach (var gate in panelPociones.GetComponentsInChildren<BuzonPotionGate>(true))
            gate.Refresh();
    }

    public void MostrarCartaEnPanel(CartaData carta)
    {
        if (carta == null) return;

        cartaActual = carta;
        cartasResueltasEnRonda = Mathf.Clamp(cartasResueltasEnRonda, 0, cartasRondaActual.Count);

        if (panelDetalle != null) panelDetalle.SetActive(true);
        if (backResCarta != null) backResCarta.SetActive(false);

        if (txtTituloCarta != null) txtTituloCarta.text = carta.titulo;
        if (txtCuerpoCarta != null) txtCuerpoCarta.text = carta.textoCarta;
        if (txtCalidadCarta != null) txtCalidadCarta.text = $"Calidad requerida: {carta.calidad}";
        if (txtResultadoCarta != null) txtResultadoCarta.text = "";
    }

    private void OnElegirPocion()
    {
        if (cartaActual == null) return;
        if (panelPociones == null) return;

        // 1) Prende panel
        panelPociones.SetActive(true);

        // 2) Refresca gates (deshabilita/activa segun BuzonProgress)
        RefreshPotionGates();

        Debug.Log($"[Buzon] Abrir inventario para carta #{cartaActual.numero} ({cartaActual.pocionRequerida}).");
    }

    // Este metodo lo llamas desde los botones de pociones (tu script ElegirPocion o OnClick)
    public void GetNamePocion(string name)
    {
        // Por ahora dejas Estandar como lo tienes
        ProcesarEntrega(name, PotionQuality.Estandar);
    }

    public void ProcesarEntrega(string pocionEntregada, PotionQuality calidadEntregada)
    {
        if (cartaActual == null)
        {
            Debug.LogWarning("[Buzon] No hay carta activa al procesar entrega.");
            return;
        }

        if (panelPociones != null) panelPociones.SetActive(false);

        string mensaje;
        int recompensa = CalcularRecompensa(cartaActual, pocionEntregada, calidadEntregada, out mensaje);

        if (playerWallet != null) playerWallet.AddCoins(recompensa);

        if (txtResultadoCarta != null)
        {
            if (backResCarta != null) backResCarta.SetActive(true);
            txtResultadoCarta.text = $"{mensaje}\n\nRecompensa: {recompensa} monedas.";
        }

        MarcarCartaCompletada(cartaActual);
        cartasResueltasEnRonda++;

        if (cartasResueltasEnRonda >= cartasRondaActual.Count)
            TerminarRondaYCrearOtra();
    }

    private int CalcularRecompensa(
        CartaData carta,
        string pocionEntregada,
        PotionQuality calidadEntregada,
        out string mensaje)
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
                $"Penalizacion del 20% ({penalizacion} monedas).";

            return recompensa;
        }

        if ((int)calidadEntregada == (int)carta.calidad)
        {
            int recompensa = CalcularRecompensaConBonus(basePrice, carta.calidad);

            mensaje =
                $"Entrega correcta!\n" +
                $"Pocion: {carta.pocionRequerida}\n" +
                $"Calidad: {calidadEntregada}";

            return recompensa;
        }

        // Calidad superior: sin penalizacion, sin bono extra
        mensaje =
            $"Entregaste calidad superior ({calidadEntregada}) a la requerida ({carta.calidad}).\n" +
            $"No hay penalizacion, pero tampoco bono extra.";

        return basePrice;
    }

    private int CalcularRecompensaConBonus(int basePrice, PotionQuality calidadRequerida)
    {
        float mult = 1f;
        switch (calidadRequerida)
        {
            case PotionQuality.Estandar: mult = 1f; break;
            case PotionQuality.Plata: mult = 1.2f; break;
            case PotionQuality.Oro: mult = 1.4f; break;
        }
        return Mathf.RoundToInt(basePrice * mult);
    }

    private void CrearRondaIntro()
    {
        cartasRondaActual = new List<CartaData>();
        cartasResueltasEnRonda = 0;

        var cartaValor = cartasEtapa1.Find(c => c.pocionRequerida == CartasConfig.POCION_VALOR_INFALIBLE);
        var cartaRevi = cartasEtapa1.Find(c => c.pocionRequerida == CartasConfig.REVITALIZANTE);
        var cartaMeta = cartasEtapa1.Find(c => c.pocionRequerida == CartasConfig.POCION_METAMORFICA);

        if (cartaValor != null) cartasRondaActual.Add(cartaValor);
        if (cartaRevi != null) cartasRondaActual.Add(cartaRevi);
        if (cartaMeta != null) cartasRondaActual.Add(cartaMeta);

        foreach (var carta in cartasRondaActual)
            poolDisponibles.Remove(carta);

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
        else if (estadoActual == BuzonStage.Etapa2 &&
                 TodasCartasEtapa1Completas() &&
                 TodasCartasEtapa2Completas())
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

    public void DesbloquearRecetaRuptura()
    {
        recetaRupturaDesbloqueada = true;
        if (estadoActual == BuzonStage.Etapa2) ReconstruirPoolEtapa2();
    }

    public void DesbloquearRecetaAmor()
    {
        recetaAmorDesbloqueada = true;
        if (estadoActual == BuzonStage.Etapa2) ReconstruirPoolEtapa2();
    }

    public void DesbloquearRecetaGuardian()
    {
        recetaGuardianDesbloqueada = true;
        if (estadoActual == BuzonStage.Etapa2) ReconstruirPoolEtapa2();
    }
}
