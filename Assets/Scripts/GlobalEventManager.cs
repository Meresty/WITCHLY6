using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EventoMundialManager : MonoBehaviour
{
    public static EventoMundialManager instance;

    [Header("Ventana del evento")]
    public float ventanaSegundos = 60;      // 15 min
    public int activacionesPorVentana = 3;    // 3 veces
    public float duracionActiva = 10;       // 3 min

    [Header("Escena donde se muestra el icono")]
    public string escenaPantallaInicial = "PantallaInicial";

    [Header("UI en PantallaInicial (nombres exactos)")]
    public string nombreBotonEnEscena = "BtnEventoMundial";
    public string nombreTextoTimer = "TxtEventoTimer";

    [Header("Minijuegos")]
    public string escenaMinijuegoA = "Estrellas";
    public string escenaMinijuegoB = "Meteoritos";

    private GameObject botonGO;
    private Button boton;
    private TextMeshProUGUI txtTimer;

    private bool eventoActivo = false;
    private float tiempoRestante = 0f;

    private Coroutine cicloRoutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        TryBindUI();

        if (cicloRoutine != null) StopCoroutine(cicloRoutine);
        cicloRoutine = StartCoroutine(CicloVentanas());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBindUI();
    }

    // Busca objetos aunque esten INACTIVOS (GameObject.Find NO los encuentra)
    private GameObject FindInActiveSceneByName(string exactName)
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();

        foreach (var root in roots)
        {
            var all = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in all)
            {
                if (t.name == exactName) return t.gameObject;
            }
        }
        return null;
    }

    private void TryBindUI()
    {
        // Solo usamos UI si estamos en PantallaInicial
        var scene = SceneManager.GetActiveScene();
        if (scene.name != escenaPantallaInicial)
        {
            if (botonGO != null) botonGO.SetActive(false);
            return;
        }

        var go = FindInActiveSceneByName(nombreBotonEnEscena);
        if (go == null) return;

        botonGO = go;
        boton = botonGO.GetComponent<Button>();
        if (boton == null) return;

        // Timer opcional
        var timerTr = botonGO.transform.Find(nombreTextoTimer);
        txtTimer = (timerTr != null) ? timerTr.GetComponent<TextMeshProUGUI>() : null;

        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(OnClickEvento);

        // Aplica estado actual
        botonGO.SetActive(eventoActivo);
        if (boton != null) boton.interactable = eventoActivo;

        UpdateTimerText();
    }

    private IEnumerator CicloVentanas()
    {
        while (true)
        {
            // 3 activaciones aleatorias dentro de 15 min sin empalmarse
            List<float> starts = GenerarStartTimesNoEmpalmados();

            float tiempoEnVentana = 0f;

            for (int i = 0; i < starts.Count; i++)
            {
                float wait = Mathf.Max(0f, starts[i] - tiempoEnVentana);
                if (wait > 0f)
                {
                    yield return new WaitForSeconds(wait);
                    tiempoEnVentana += wait;
                }

                // Activar evento
                ActivarEvento();

                // Se mantiene activo 3 min o hasta que el jugador lo use
                float t = 0f;
                while (t < duracionActiva && eventoActivo)
                {
                    t += Time.deltaTime;
                    tiempoRestante = Mathf.Max(0f, duracionActiva - t);
                    UpdateTimerText();
                    yield return null;
                }

                // Si no lo usaron, expira
                if (eventoActivo) DesactivarEvento();

                tiempoEnVentana += Mathf.Min(t, duracionActiva);
            }

            // Espera restante para completar la ventana de 15 min
            float restante = Mathf.Max(0f, ventanaSegundos - tiempoEnVentana);
            if (restante > 0f) yield return new WaitForSeconds(restante);
        }
    }

    private List<float> GenerarStartTimesNoEmpalmados()
    {
        float maxStart = Mathf.Max(0f, ventanaSegundos - duracionActiva);
        var times = new List<float>(activacionesPorVentana);

        int safeGuard = 0;
        while (times.Count < activacionesPorVentana && safeGuard < 500)
        {
            safeGuard++;
            float candidate = Random.Range(0f, maxStart);

            bool ok = true;
            for (int i = 0; i < times.Count; i++)
            {
                // Para no empalmar: distancia minima = duracionActiva
                if (Mathf.Abs(candidate - times[i]) < duracionActiva)
                {
                    ok = false;
                    break;
                }
            }

            if (ok) times.Add(candidate);
        }

        times.Sort();
        return times;
    }

    private void ActivarEvento()
    {
        eventoActivo = true;
        tiempoRestante = duracionActiva;

        // Si estamos en PantallaInicial y el boton existe, lo mostramos
        if (botonGO != null) botonGO.SetActive(true);
        if (boton != null) boton.interactable = true;

        UpdateTimerText();
    }

    private void DesactivarEvento()
    {
        eventoActivo = false;
        tiempoRestante = 0f;

        if (botonGO != null) botonGO.SetActive(false);
        UpdateTimerText();
    }

    private void OnClickEvento()
    {
        if (!eventoActivo) return;

        // Se consume la activacion (ya no se puede volver a usar en esta ventana)
        DesactivarEvento();

        // Minijuego aleatorio
        string escena = (Random.value < 0.5f) ? escenaMinijuegoA : escenaMinijuegoB;
        SceneManager.LoadScene(escena);
    }

    private void UpdateTimerText()
    {
        if (txtTimer == null) return;

        if (!eventoActivo)
        {
            txtTimer.text = "";
            return;
        }

        int sec = Mathf.CeilToInt(tiempoRestante);
        txtTimer.text = sec.ToString() + "s";
    }
}
