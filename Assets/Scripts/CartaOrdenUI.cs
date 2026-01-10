using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Witchly.Mercado
{
    public class CartaOrdenUI : MonoBehaviour
    {
        [Header("Referencias UI")]
        public Image iconoObjeto;
        public TMP_Text textoNombreObjeto;
        public TMP_Text textoNombreVendedor;
        public TMP_Text textoCantidad;
        public TMP_Text textoPrecio;
        public Button botonComprar;

        private OrdenVenta orden;
        private PlayerWallet monedero;
        private System.Action<OrdenVenta> callbackComprada;

        public void Configurar(OrdenVenta orden, PlayerWallet monedero, System.Action<OrdenVenta> callbackComprada)
        {
            this.orden = orden;
            this.monedero = monedero;
            this.callbackComprada = callbackComprada;

            // ----- Rellenar UI -----
            if (iconoObjeto != null)
                iconoObjeto.sprite = orden.iconoObjeto;

            if (textoNombreObjeto != null)
                textoNombreObjeto.text = orden.nombreObjeto;

            if (textoNombreVendedor != null)
                textoNombreVendedor.text = orden.nombreVendedor;

            if (textoCantidad != null)
                textoCantidad.text = $"x{orden.cantidad}";

            if (textoPrecio != null)
                textoPrecio.text = orden.precioTotal.ToString();


            if (botonComprar != null)
            {
                botonComprar.onClick.RemoveAllListeners();
                botonComprar.onClick.AddListener(Comprar);
            }

            Debug.Log($"[CartaOrdenUI] Configurada carta: {orden.cantidad} x {orden.nombreObjeto} - {orden.precioTotal} monedas");
        }

        private void Comprar()
        {
            if (orden == null || monedero == null)
            {
                Debug.LogWarning("[CartaOrdenUI] No hay orden o monedero asignado al intentar comprar.");
                return;
            }

            if (!monedero.PuedePagar(orden.precioTotal))
            {
                Debug.Log("[Mercado] No tienes suficientes monedas.");
                return;
            }

            monedero.Pagar(orden.precioTotal);
            callbackComprada?.Invoke(orden);
        }
    }
}
