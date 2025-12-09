using UnityEngine;

public class ProbadorMinijuego : MonoBehaviour
{
    [SerializeField] private MinijuegoPresicion minijuegoPrecision;

    void Start()
    {
        Debug.Log("=== PROBADOR MINIJUEGO INICIADO ===");

        if (minijuegoPrecision == null)
        {
            Debug.LogError("CRÍTICO: No hay referencia al minijuego de precisión!");

            minijuegoPrecision = FindObjectOfType<MinijuegoPresicion>();
            if (minijuegoPrecision == null)
                Debug.LogError("No se encontró PrecisionMiniGame en la escena");
            else
                Debug.Log("Encontrado automáticamente: " + minijuegoPrecision.name);
        }
        else
        {
            Debug.Log("Referencia OK: " + minijuegoPrecision.name);
        }

        Debug.Log("Instrucciones: Presiona ESPACIO para iniciar minijuego");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Tecla ESPACIO presionada");
            IniciarMinijuegoPrueba();

        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Tecla R presionada - Reiniciando");
            ReiniciarMinijuego();
        }
    }

    void IniciarMinijuegoPrueba()
    {
        Debug.Log("=== LLAMANDO A INICIAR MINIJUEGO ===");

        if (minijuegoPrecision == null)
        {
            Debug.LogError("No se puede iniciar: referencia es null");
            return;
        }

        Debug.Log("Llamando a IniciarMinijuego...");
        minijuegoPrecision.IniciarMinijuego(ResultadoMinijuego);
    }

    void ResultadoMinijuego(bool exito)
    {
        Debug.Log($"=== RESULTADO MINIJUEGO RECIBIDO ===");
        Debug.Log($"Éxito: {exito}");

        if (exito)
        {
            Debug.Log("RESULTADO: ¡ÉXITO!");
            Debug.Log("El jugador puede continuar con la preparación");
        }
        else
        {
            Debug.Log(" RESULTADO: FALLO");
            Debug.Log("El jugador NO puede continuar");
        }
    }

    void ReiniciarMinijuego()
    {
        Debug.Log("Reiniciando minijuego...");
        if (minijuegoPrecision != null)
            minijuegoPrecision.ReiniciarMinijuego();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 100),
            "CONTROLES PRUEBA:\n" +
            "ESPACIO = Iniciar minijuego\n" +
            "R = Reiniciar\n\n" +
            "Estado: " + (minijuegoPrecision != null ? "OK" : "ERROR"));
    }
}