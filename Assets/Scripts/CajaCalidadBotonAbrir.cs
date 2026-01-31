using UnityEngine;

public class CajaCalidadOpenButton : MonoBehaviour
{
    [Header("Caja asociada (Caja1..Caja4)")]
    public CajaCalidad caja;

    [Header("Panel grande (overlay)")]
    public GameObject panelCajaUI;

    [Header("Componente CajaCalidadUI dentro del panel")]
    public CajaCalidadUI cajaUI;

    public void AbrirCaja()
    {
        if (panelCajaUI == null || cajaUI == null || caja == null)
        {
            Debug.LogError("[CajaCalidadOpenButton] Faltan referencias en el inspector.");
            return;
        }

        panelCajaUI.SetActive(true);
        cajaUI.gameObject.SetActive(true);

        cajaUI.SetCaja(caja);
    }
}
