using UnityEngine;
using TMPro;

public class NotaUI : MonoBehaviour
{
    [Header("Textos de la Nota")]
    public TextMeshProUGUI NombrePocion;
    public TextMeshProUGUI paso1;
    public TextMeshProUGUI paso2;
    public TextMeshProUGUI paso3;

    public void MostrarReceta(PocionSO pocion)
    {
        if (pocion == null)
        {
            LimpiarNota();
            return;
        }

        if (NombrePocion != null)
            NombrePocion.text = pocion.pocionNombre;


        if (paso1 != null && pocion.suero != null)
            paso1.text = pocion.suero.itemNombre;

        if (paso2 != null && pocion.ingrediente1 != null)
            paso2.text = pocion.ingrediente1.itemNombre;

        if (paso3 != null && pocion.ingrediente2 != null)
            paso3.text = pocion.ingrediente2.itemNombre;
    }

    public void LimpiarNota()
    {
        if (NombrePocion != null)
            NombrePocion.text = "???";

        if (paso1 != null)
            paso1.text = "???";

        if (paso2 != null)
            paso2.text = "???";

        if (paso3 != null)
            paso3.text = "???";
    }
}