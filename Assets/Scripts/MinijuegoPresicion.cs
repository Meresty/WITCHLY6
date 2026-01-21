using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinijuegoPresicion : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject miniJuegoPanel;
    [SerializeField] private GameObject uiPresicion;
    [SerializeField] private CalderoLogic uiCaldero;
    [SerializeField] private RectTransform flecha;
    [SerializeField] private Image imgBarra;
    [SerializeField] private Button btnParar;
    [SerializeField] private TextMeshProUGUI txtInstruccion;
    [SerializeField] private TextMeshProUGUI txtResultado;

    [Header("Configuración Minijuego")]
    [SerializeField] private float velFlecha = 300f;
    [SerializeField] private float zonaSeguraPorc = 0.2f;
    [SerializeField] private float ancho = 0.4f;

    [Header("Mensajes")]
    [SerializeField] private string mensajeFallido = "Fallaste, no puedes continuar con el proceso de la poción";
    [SerializeField] private string mensajeExito = "¡Bien! Puedes continuar";
    [SerializeField] private string mensajeInstrucciones = "Detén la flecha en la zona verde";

    [Header("Escena")]
    [SerializeField] private string nombreEscenaCaldero = "Caldero";
    [SerializeField] private string nombreEscenaInicial = "PantallaInicial";

    [Header("Bloqueo en Fallo")] 
    [SerializeField] private GameObject panelBloqueo;
    [SerializeField] private TextMeshProUGUI txtClickParaSalir;

    int step = 0;
    ItemInfo itemInRisk;
    private bool flechaEnMovimiento = false;
    private bool minijuegoActivo = false;
    private float direccion = 1f;
    private float limiteIzquierdo, limiteDerecho;
    private float zonaSeguraInicio, zonaSeguraFin;
    private System.Action<bool> alCompletarMinijuego;
   
    private bool modoEscena = false;

    void Start()
    {
        Debug.Log("Ancho real barra: " + imgBarra.rectTransform.rect.width);
    }
    private void OnEnable()
    {
        btnParar.onClick.AddListener(PararFlecha);
    }
    private void OnDisable()
    {
        btnParar.onClick.RemoveListener(PararFlecha);
    }

    void Update()
    {
        if (!flechaEnMovimiento || !minijuegoActivo) return;
        MoverFlecha();
    }

    void MoverFlecha()
    {
        float nuevaX = flecha.anchoredPosition.x + (velFlecha * direccion * Time.deltaTime);

        if (nuevaX >= limiteDerecho)
        {
            nuevaX = limiteDerecho;
            direccion = -1f;
        }
        else if (nuevaX <= limiteIzquierdo)
        {
            nuevaX = limiteIzquierdo;
            direccion = 1f;
        }

        flecha.anchoredPosition = new Vector2(nuevaX, flecha.anchoredPosition.y);
    }

    public void IniciarMinijuego(int _step, ItemInfo _item)
    {
        panelBloqueo.SetActive(false);
        step = _step;
        itemInRisk = _item;
        minijuegoActivo = true;
        flechaEnMovimiento = true;

        uiPresicion.SetActive(true);

        CalcularLimites();
        PosicionarFlechaInicial();

        txtInstruccion.text = mensajeInstrucciones;
        txtResultado.text = "";
        txtResultado.color = Color.white;

        btnParar.interactable = true;
    }

    void CalcularLimites()
    {
        float anchoBarra = imgBarra.rectTransform.rect.width;

        limiteIzquierdo = imgBarra.rectTransform.anchoredPosition.x - imgBarra.rectTransform.rect.width / ancho;
        limiteDerecho = imgBarra.rectTransform.anchoredPosition.x + imgBarra.rectTransform.rect.width / ancho;

        float anchoZonaSegura = anchoBarra * zonaSeguraPorc;

        zonaSeguraInicio = imgBarra.rectTransform.anchoredPosition.x - (anchoZonaSegura / 2f);
        zonaSeguraFin = imgBarra.rectTransform.anchoredPosition.x + (anchoZonaSegura / 2f);

        Debug.Log($"Zona segura: {zonaSeguraInicio} a {zonaSeguraFin} (ancho: {anchoZonaSegura})");
    }


    void PosicionarFlechaInicial()
    {
        float posicionInicial = Random.Range(0, 2) == 0 ? limiteIzquierdo : limiteDerecho;
        flecha.anchoredPosition = new Vector2(posicionInicial, flecha.anchoredPosition.y);

        direccion = Random.Range(0, 2) == 0 ? 1f : -1f;
    }

    void PararFlecha()
    {
        if (!minijuegoActivo) return;

        flechaEnMovimiento = false;
        btnParar.interactable = false;

        VerificarPosicionFlecha();
    }


    void VerificarPosicionFlecha()
    {
        float posicionX = flecha.anchoredPosition.x;
        bool enZonaSegura = posicionX >= zonaSeguraInicio && posicionX <= zonaSeguraFin;

        Debug.Log($"Flecha en posición: {posicionX}, Zona segura: {zonaSeguraInicio}-{zonaSeguraFin}, Resultado: {enZonaSegura}");

        if (enZonaSegura)
        {
            OnExitoMinijuego();
        }
        else
        {
            OnFalloMinijuego();
        }
    }


    void OnExitoMinijuego()
    {
        txtResultado.text = mensajeExito;
        txtResultado.color = Color.green;
        step = 0;
        itemInRisk = null;
        StartCoroutine(AnimacionExito());
        StartCoroutine(CerrarMinijuegoDespuesDeDelay(true));
        uiCaldero.ProcesarResultadoMinijuego(true);
    }


    void OnFalloMinijuego()
    {
        txtResultado.text = mensajeFallido;
        txtResultado.color = Color.red;
        switch (step)
        {
            case 0:
                InventorySystem.Instance.RemoveSerum(itemInRisk.itemNombre,1);
                break;
            case 1:
                InventorySystem.Instance.RemovePlant(itemInRisk.plantaTipo,itemInRisk.calidad, 1);
                break;
            case 2:
                InventorySystem.Instance.RemovePlant(itemInRisk.plantaTipo, itemInRisk.calidad, 1);
                break;
        }
        step = 0;
        itemInRisk = null;

        StartCoroutine(AnimacionFallo());
        StartCoroutine(BloquearPantallaYEsperarClick());
        uiCaldero.ProcesarResultadoMinijuego(false);
    }


    IEnumerator BloquearPantallaYEsperarClick()
    {
        yield return new WaitForSeconds(2f);

        if (panelBloqueo != null)
            panelBloqueo.SetActive(true);
        if (txtClickParaSalir != null)
        {
            txtClickParaSalir.text = "Click para volver al menú";
            StartCoroutine(Parpadear(txtClickParaSalir));

        }


        bool esperandoClick = true;
        while (esperandoClick)
        {
            if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
            {
                esperandoClick = false;
            }
            yield return null;
        }

        RegresarAEscenaInicial();
    }







    void RegresarAEscenaInicial()
    {
        Debug.Log("Regresando a escena inicial por fallo en minijuego");

        uiPresicion.SetActive(false);
    }






    IEnumerator AnimacionExito()
    {
        Vector3 escalaOriginal = flecha.localScale;
        float duracion = 0.5f;
        float tiempo = 0;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;
            float escala = 1 + Mathf.Sin(progreso * Mathf.PI) * 0.3f;
            flecha.localScale = escalaOriginal * escala;
            yield return null;
        }

        flecha.localScale = escalaOriginal;
    }






    IEnumerator AnimacionFallo()
    {
        Vector2 posicionOriginal = flecha.anchoredPosition;
        float duracion = 0.5f;
        float tiempo = 0;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;
            float temblor = Mathf.Sin(progreso * 30f) * 5f;
            flecha.anchoredPosition = posicionOriginal + new Vector2(temblor, 0);
            yield return null;
        }

        flecha.anchoredPosition = posicionOriginal;
    }







    IEnumerator CerrarMinijuegoDespuesDeDelay(bool exito)
    {
        yield return new WaitForSeconds(2f);

        miniJuegoPanel.SetActive(false);
        minijuegoActivo = false;
    }






    void FinalizarMinijuego(bool exito)
    {
        Debug.Log($"Finalizando minijuego. Éxito: {exito}");

        PlayerPrefs.SetInt("MinijuegoExito", exito ? 1 : 0);
        PlayerPrefs.Save();

        uiPresicion.SetActive(false);
    }






    public void ReiniciarMinijuego()
    {
        minijuegoActivo = false;
        flechaEnMovimiento = false;
        miniJuegoPanel.SetActive(false);
    }







    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !minijuegoActivo) return;

        Vector3 posicionBarra = imgBarra.transform.position;
        float anchoBarra = imgBarra.rectTransform.rect.width * imgBarra.transform.lossyScale.x;
        float anchoZonaSegura = anchoBarra * zonaSeguraPorc;
        Vector3 centroZonaSegura = posicionBarra;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(centroZonaSegura, new Vector3(anchoZonaSegura, 50, 0));
    }




    IEnumerator Parpadear(TextMeshProUGUI texto)
    {
        if (texto == null) yield break;

        float duracionCiclo = 3f; 

        while (texto.gameObject.activeSelf) 
        {

            float tiempo = 0;
            while (tiempo < duracionCiclo / 2)
            {
                tiempo += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0.3f, tiempo / (duracionCiclo / 2));
                texto.color = new Color(texto.color.r, texto.color.g, texto.color.b, alpha);
                yield return null;
            }

            tiempo = 0;
            while (tiempo < duracionCiclo / 2)
            {
                tiempo += Time.deltaTime;
                float alpha = Mathf.Lerp(0.3f, 1f, tiempo / (duracionCiclo / 2));
                texto.color = new Color(texto.color.r, texto.color.g, texto.color.b, alpha);
                yield return null;
            }
        }
    }
}