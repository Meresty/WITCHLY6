using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class EventoMundialManager : MonoBehaviour
{
    public static EventoMundialManager instance;

    [Header("Configuración del evento")]
    public GameObject botonEventoPrefab; // Prefab del botón del evento (se instancia en el inventario)
    public float duracionEvento = 300f; // 5 minutos = 300 segundos
    public int maxEventosPorDia = 3;

    private int eventosHoy = 0;
    private bool eventoActivo = false;
    private bool botonVisible = false;
    private float tiempoRestante;

    private GameObject botonInstanciado;
    private TextMeshProUGUI textoContador;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(GenerarEventosAleatorios());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator GenerarEventosAleatorios()
    {
        while (true)
        {
            if (eventosHoy < maxEventosPorDia && !eventoActivo)
            {

                float espera = Random.Range(10f, 30f); //Para test: 10 a 30 segundos
                yield return new WaitForSeconds(espera);

   
                eventoActivo = true;
                eventosHoy++;
                NotificarEvento();
            }
            yield return null;
        }
    }

    void NotificarEvento()
    {
        Debug.Log("¡Evento Mundial Disponible!");
        // Aquí puedes poner una UI global tipo popup si quieres (por ahora solo log)

        PlayerPrefs.SetInt("EventoActivo", 1);
    }

    public void MostrarBotonEvento(GameObject contenedorUI)
    {
        if (!eventoActivo || botonVisible) return;

        botonInstanciado = Instantiate(botonEventoPrefab, contenedorUI.transform);
        textoContador = botonInstanciado.GetComponentInChildren<TextMeshProUGUI>();
        botonVisible = true;

        botonInstanciado.GetComponent<Button>().onClick.AddListener(() => IniciarEvento());

        tiempoRestante = duracionEvento;
        StartCoroutine(ContadorRegresivo());
    }

    IEnumerator ContadorRegresivo()
    {
        while (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            if (textoContador != null)
                textoContador.text = Mathf.CeilToInt(tiempoRestante).ToString() + "s";

            yield return null;
        }

        FinalizarEvento(false);
    }

    void IniciarEvento()
    {
        if (!eventoActivo) return;

        string[] escenas = { "Estrellas", "Meteoritos" };
        string escenaAleatoria = escenas[Random.Range(0, escenas.Length)];

        Debug.Log("Lanzando evento ? " + escenaAleatoria);
        SceneManager.LoadScene(escenaAleatoria);
    }

    public void FinalizarEvento(bool completado)
    {
        eventoActivo = false;
        botonVisible = false;

        if (botonInstanciado != null)
            Destroy(botonInstanciado);

        PlayerPrefs.SetInt("EventoActivo", 0);

        if (completado)
            Debug.Log("Evento completado con éxito");
        else
            Debug.Log("El evento expiró");
    }
}
