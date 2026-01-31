using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CajaCalidadPanel : MonoBehaviour
{
    [Header("Slots UI")]
    public Transform slotEntrada1;
    public Transform slotEntrada2;
    public Transform slotSalida;
    public GameObject iconoPlantaPrefab;

    [Header("UI")]
    public TextMeshProUGUI textoTimer;
    public Button botonMejorar;

    private CajaCalidad cajaActual;

    private PlantaTipo? tipoSeleccionado = null;
    private PlantaCalidad? calidadSeleccionada = null;
    private int cantidadSeleccionada = 0;

    public void Abrir(CajaCalidad caja)
    {
        cajaActual = caja;
        LimpiarSlots();
        gameObject.SetActive(true);
        ActualizarUI();
    }

    public void Cerrar()
    {
        LimpiarSlots();
        cajaActual = null;
        gameObject.SetActive(false);
    }

    // LLAMADO DESDE EL INVENTARIO (igual que Caldero)
    public void AddPlantFromInventory(PlantaTipo tipo, PlantaCalidad calidad)
    {
        if (cantidadSeleccionada >= 2) return;

        if (cantidadSeleccionada == 0)
        {
            tipoSeleccionado = tipo;
            calidadSeleccionada = calidad;
        }
        else
        {
            if (tipo != tipoSeleccionado || calidad != calidadSeleccionada)
            {
                Debug.Log("Las plantas deben ser del mismo tipo y calidad");
                return;
            }
        }

        cantidadSeleccionada++;
        CrearIcono(slotEntrada1, slotEntrada2);
        ActualizarUI();
    }

    void CrearIcono(Transform s1, Transform s2)
    {
        Transform target = cantidadSeleccionada == 1 ? s1 : s2;
        Instantiate(iconoPlantaPrefab, target);
    }

    public void OnClickMejorar()
    {
        if (cajaActual == null) return;

        if (cantidadSeleccionada < 2)
        {
            Debug.Log("Necesitas 2 plantas");
            return;
        }

        if (cajaActual.TrySetInput(tipoSeleccionado.Value, calidadSeleccionada.Value, out string msg))
        {
            cajaActual.StartUpgrade(out msg);
            LimpiarSlots();
        }
    }

    void ActualizarUI()
    {
        botonMejorar.interactable = cantidadSeleccionada == 2;
    }

    void LimpiarSlots()
    {
        foreach (Transform t in slotEntrada1) Destroy(t.gameObject);
        foreach (Transform t in slotEntrada2) Destroy(t.gameObject);
        foreach (Transform t in slotSalida) Destroy(t.gameObject);

        tipoSeleccionado = null;
        calidadSeleccionada = null;
        cantidadSeleccionada = 0;
    }

    void Update()
    {
        if (cajaActual != null && cajaActual.IsRunning())
        {
            textoTimer.text = cajaActual.GetRemainingTime().ToString(@"mm\:ss");
        }
    }
}
