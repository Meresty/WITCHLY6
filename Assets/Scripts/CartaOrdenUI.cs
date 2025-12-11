using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Witchly.Mercado
{
    public class CartaOrdenUI : MonoBehaviour
    {
        [Header("Referencias UI")]
        public Image iconoObjeto;
        public TextMeshProUGUI textoNombreObjeto;
        public TextMeshProUGUI textoNombreVendedor;
        public TextMeshProUGUI textoCantidad;
        public TextMeshProUGUI textoPrecio;
        public Button botonComprar;

        private OrdenVenta orden;
        private GestorMercado gestor;

        public void Configurar(OrdenVenta orden, GestorMercado gestor)
        {
            this.orden = orden;
            this.gestor = gestor;

            // ===== NOMBRE OBJETO =====
            string nombreObjeto = "-";
            if (orden.objeto != null)
            {
                if (!string.IsNullOrEmpty(orden.objeto.nombreMostrado))
                    nombreObjeto = orden.objeto.nombreMostrado;
                else
                    nombreObjeto = orden.objeto.name; // nombre del asset
            }

            // ===== NOMBRE VENDEDOR =====
            string nombreVendedor = "-";
            if (orden.vendedor != null)
            {
                if (!string.IsNullOrEmpty(orden.vendedor.nombreVendedor))
                    nombreVendedor = orden.vendedor.nombreVendedor;
                else
                    nombreVendedor = orden.vendedor.name; // nombre del asset
            }

            // ===== ICONO =====
            if (iconoObjeto != null)
                iconoObjeto.sprite = orden.objeto != null ? orden.objeto.icono : null;

            // ===== TEXTOS =====
            if (textoNombreObjeto != null)
                textoNombreObjeto.text = nombreObjeto;

            if (textoNombreVendedor != null)
                textoNombreVendedor.text = nombreVendedor;

            if (textoCantidad != null)
                textoCantidad.text = $"x{orden.cantidad}";

            if (textoPrecio != null)
                textoPrecio.text = orden.precioTotal.ToString("0");

            // ===== BOTÓN =====
            if (botonComprar != null)
            {
                botonComprar.onClick.RemoveAllListeners();
                botonComprar.onClick.AddListener(() => gestor.IntentarComprar(orden));
            }

            Debug.Log($"[CartaOrdenUI] Configurada carta: {orden.cantidad} x {nombreObjeto} - {nombreVendedor} - {orden.precioTotal} monedas");
        }
    }
}
