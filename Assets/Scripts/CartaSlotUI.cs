using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CartaSlotUI : MonoBehaviour
{
    [Header("Referencias de UI del sobre")]
    public Button botonCarta;     
    public TMP_Text textoTitulo;  

    private CartaData carta;            
    private BuzonController buzonOwner;  


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
