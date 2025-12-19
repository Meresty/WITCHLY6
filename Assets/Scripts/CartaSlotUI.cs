using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CartaSlotUI : MonoBehaviour
{
    [Header("Referencias de UI del sobre")]
    public Button botonCarta;     // El Button del propio sobre
    public TMP_Text textoTitulo;  // El TMP hijo (TituloCarta1, etc.)

    private CartaData carta;              // La carta que representa este sobre
    private BuzonController buzonOwner;   // Referencia al controlador

    /// <summary>
    /// Configura este sobre con una carta específica.
    /// </summary>
    public void Configurar(BuzonController owner, CartaData cartaData)
    {
        buzonOwner = owner;
        carta = cartaData;

        if (carta != null)
        {
            if (textoTitulo != null)
                textoTitulo.text = carta.titulo;
            else
                Debug.LogWarning($"[CartaSlotUI] {name} no tiene textoTitulo asignado.");

            if (botonCarta != null)
            {
                botonCarta.interactable = true;
                botonCarta.onClick.RemoveAllListeners();
                botonCarta.onClick.AddListener(OnClickCarta);
            }
            else
            {
                Debug.LogWarning($"[CartaSlotUI] {name} no tiene botonCarta asignado.");
            }
        }
        else
        {
            // Slot vacío (por si luego quieres tener menos cartas que sobres)
            if (textoTitulo != null)
                textoTitulo.text = "";

            if (botonCarta != null)
            {
                botonCarta.interactable = false;
                botonCarta.onClick.RemoveAllListeners();
            }
        }
    }

    private void OnClickCarta()
    {
        if (buzonOwner == null || carta == null)
        {
            Debug.LogWarning($"[CartaSlotUI] {name} hizo click pero no tiene buzon/carta asignados.");
            return;
        }

        buzonOwner.MostrarCartaEnPanel(carta);
    }
}
