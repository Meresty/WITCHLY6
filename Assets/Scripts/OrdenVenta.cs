// Scripts/Mercado/OrdenVenta.cs
using UnityEngine;

namespace Witchly.Mercado
{
    [System.Serializable]
    public class OrdenVenta
    {
        // Referencias de datos (por si luego las necesitas)
        public DatosObjetoMercado datosObjeto;
        public PerfilVendedor perfilVendedor;

        // Datos numéricos de la orden
        public int cantidad;
        public int precioTotal;
        public ClasePrecio clasePrecio;

        // Datos ya “masticados” para la UI
        public Sprite iconoObjeto;       // por ahora lo dejamos null
        public string nombreObjeto;      // texto que verá el jugador
        public string nombreVendedor;    // texto que verá el jugador

        // --------------------------------------------------------------------
        //  Constructor que usa GestorMercado
        // --------------------------------------------------------------------
        public OrdenVenta(
            DatosObjetoMercado datosObjeto,
            PerfilVendedor perfilVendedor,
            int cantidad,
            int precioTotal,
            ClasePrecio clasePrecio)
        {
            this.datosObjeto = datosObjeto;
            this.perfilVendedor = perfilVendedor;
            this.cantidad = cantidad;
            this.precioTotal = precioTotal;
            this.clasePrecio = clasePrecio;

            // ===== Nombre del objeto =====
            if (datosObjeto != null)
            {
                // Si DatosObjetoMercado es ScriptableObject / MonoBehaviour,
                // usamos el nombre del asset/objeto en Unity.
                var unityObj = datosObjeto as Object;
                nombreObjeto = unityObj != null
                    ? unityObj.name
                    : datosObjeto.ToString();
            }
            else
            {
                nombreObjeto = string.Empty;
            }

            // ===== Nombre del vendedor =====
            if (perfilVendedor != null)
            {
                var unityVend = perfilVendedor as Object;
                nombreVendedor = unityVend != null
                    ? unityVend.name
                    : perfilVendedor.ToString();
            }
            else
            {
                nombreVendedor = string.Empty;
            }

            // Icono: de momento null. Más adelante,
            // si quieres, rellenamos esto leyendo tu sprite real.
            iconoObjeto = null;
        }

        // --------------------------------------------------------------------
        //  Propiedades en PascalCase (para GestorMercado) + alias usados en la UI
        // --------------------------------------------------------------------
        public string NombreObjeto => nombreObjeto;
        public string NombreVendedor => nombreVendedor;
        public int Cantidad => cantidad;
        public int PrecioTotal => precioTotal;

        // Alias en minúsculas que usa CartaOrdenUI
        public Sprite iconoObjetoSprite => iconoObjeto;   // por si necesitas otro nombre
    }
}
