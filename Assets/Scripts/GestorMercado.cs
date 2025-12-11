using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Witchly.Mercado
{
    // ================== MODELO DE ORDEN ==================
    [System.Serializable]
    public class OrdenVenta
    {
        public DatosObjetoMercado objeto;
        public PerfilVendedor vendedor;

        public int cantidad;
        public ClasePrecio clasePrecio;

        public int precioUnitario; // después de multiplicador
        public int precioTotal;    // precioUnitario * cantidad
    }

    // ================== GESTOR DE MERCADO ==================
    public class GestorMercado : MonoBehaviour
    {
        [Header("Catálogo de objetos")]
        public DatosObjetoMercado[] plantas;
        public DatosObjetoMercado[] semillas;
        public DatosObjetoMercado[] sueros;

        [Header("Cartas del vendedor Anónimo")]
        public PerfilVendedor vendedorAnonimo;
        public bool cartasAnonimo = true;
        [Range(0, 10)] public int cantidadCartasAnonimo = 3;
        public int precioCartaAnonimo = 100;

        [Header("Perfiles de vendedores (tabla Anexo)")]
        public List<PerfilVendedor> vendedores = new();

        [Header("UI")]
        public RectTransform contenidoScrollView;  // Content del Scroll
        public CartaOrdenUI prefabCartaOrden;
        public TMP_InputField buscadorInput;
        public TMP_Dropdown filtrosDropdown;

        [Header("Configuración de mercado")]
        [Min(1)] public int ordenesPorObjeto = 2;

        [Header("Monedero del jugador (para comprar)")]
        public PlayerWallet monedero; // opcional, ver script de abajo

        // Estado interno
        private readonly List<OrdenVenta> ordenesActuales = new();

        private void Start()
        {
            // Suscribimos eventos de búsqueda / filtros (los detallamos luego)
            if (buscadorInput != null)
                buscadorInput.onValueChanged.AddListener(_ => RefrescarUI());

            if (filtrosDropdown != null)
                filtrosDropdown.onValueChanged.AddListener(_ => RefrescarUI());

            GenerarOrdenesYUI();
        }

        // ======================================================
        // GENERACIÓN DE ÓRDENES (precio, cantidad, vendedor...)
        // ======================================================
        private void GenerarOrdenesYUI()
        {
            ordenesActuales.Clear();

            // Limpiar hijos actuales del Content
            if (contenidoScrollView != null)
            {
                for (int i = contenidoScrollView.childCount - 1; i >= 0; i--)
                {
                    Destroy(contenidoScrollView.GetChild(i).gameObject);
                }
            }

            // 1) Generar órdenes dinámicas para todos los objetos del catálogo
            List<DatosObjetoMercado> catalogo = ObtenerCatalogoCompleto();

            if (catalogo.Count == 0)
            {
                Debug.LogWarning("[Mercado] No hay objetos en el catálogo.");
                return;
            }

            foreach (DatosObjetoMercado obj in catalogo)
            {
                for (int i = 0; i < ordenesPorObjeto; i++)
                {
                    PerfilVendedor vendedor = ElegirVendedorAleatorio();
                    if (vendedor == null)
                    {
                        Debug.LogWarning("[Mercado] No hay vendedores configurados.");
                        continue;
                    }

                    int cantidad = ElegirCantidadConProbabilidad();      // RQF98
                    ClasePrecio clase = ElegirClasePrecioAleatoria();    // RQF99

                    // Multiplicador según tipo (planta/semilla/suero) y clase (A/B/C)  RQF95, RQF99
                    float mult = vendedor.ObtenerMultiplicador(obj.tipo, clase);

                    int precioUnitario = Mathf.Max(1, Mathf.RoundToInt(obj.precioBase * mult));
                    int precioTotal = precioUnitario * cantidad;         // RQF100

                    OrdenVenta orden = new OrdenVenta
                    {
                        objeto = obj,
                        vendedor = vendedor,
                        cantidad = cantidad,
                        clasePrecio = clase,
                        precioUnitario = precioUnitario,
                        precioTotal = precioTotal
                    };

                    ordenesActuales.Add(orden);
                }
            }

            // 2) Cartas especiales del vendedor Anónimo (precio fijo)  (texto que pusiste)
            if (cartasAnonimo && vendedorAnonimo != null && cantidadCartasAnonimo > 0)
            {
                for (int i = 0; i < cantidadCartasAnonimo; i++)
                {
                    DatosObjetoMercado objetoRandom = ElegirObjetoRandomDeCatalogo();
                    if (objetoRandom == null) continue;

                    OrdenVenta ordenAnon = new OrdenVenta
                    {
                        objeto = objetoRandom,
                        vendedor = vendedorAnonimo,
                        cantidad = 1,
                        clasePrecio = ClasePrecio.B,
                        precioUnitario = precioCartaAnonimo,
                        precioTotal = precioCartaAnonimo
                    };

                    ordenesActuales.Add(ordenAnon);
                }
            }

            // 3) Pintar en UI
            RefrescarUI();
        }

        // Crea las tarjetas de UI según las órdenes filtradas
        private void RefrescarUI()
        {
            if (contenidoScrollView == null || prefabCartaOrden == null)
                return;

            // Limpiar hijos actuales
            for (int i = contenidoScrollView.childCount - 1; i >= 0; i--)
            {
                Destroy(contenidoScrollView.GetChild(i).gameObject);
            }

            // TODO: aquí podrías aplicar filtros / búsqueda. Por ahora mostramos todo.
            foreach (OrdenVenta orden in ordenesActuales)
            {
                CartaOrdenUI carta = Instantiate(prefabCartaOrden, contenidoScrollView);
                carta.Configurar(orden, this);   // ?? ESTO ES LO IMPORTANTE
            }

        }

        // =================== UTILIDADES ===================

        private List<DatosObjetoMercado> ObtenerCatalogoCompleto()
        {
            var lista = new List<DatosObjetoMercado>();

            if (plantas != null) lista.AddRange(plantas);
            if (semillas != null) lista.AddRange(semillas);
            if (sueros != null) lista.AddRange(sueros);

            return lista;
        }

        private DatosObjetoMercado ElegirObjetoRandomDeCatalogo()
        {
            List<DatosObjetoMercado> cat = ObtenerCatalogoCompleto();
            if (cat.Count == 0) return null;
            int idx = Random.Range(0, cat.Count);
            return cat[idx];
        }

        private PerfilVendedor ElegirVendedorAleatorio()
        {
            if (vendedores == null || vendedores.Count == 0)
                return null;

            int idx = Random.Range(0, vendedores.Count);
            return vendedores[idx];
        }

        // 50% ? 1, 30% ? 2, 20% ? 3   (RQF98)
        private int ElegirCantidadConProbabilidad()
        {
            float r = Random.value; // 0-1

            if (r < 0.5f)          // 0.0 - 0.5
                return 1;
            if (r < 0.8f)          // 0.5 - 0.8
                return 2;
            return 3;              // 0.8 - 1.0
        }

        // A/B/C elegidos aleatoriamente (RQF99.1)
        private ClasePrecio ElegirClasePrecioAleatoria()
        {
            int v = Random.Range(0, 3); // 0,1,2
            return (ClasePrecio)v;
        }

        // ================== COMPRA ==================

        public void IntentarComprar(OrdenVenta orden)
        {
            if (orden == null)
                return;

            if (monedero == null)
            {
                Debug.LogWarning("[Mercado] No hay PlayerWallet asignado. Solo log de prueba.");
                Debug.Log($"[Mercado] (Simulación) Comprar {orden.cantidad} x {orden.objeto.nombreMostrado} " +
                          $"al vendedor {orden.vendedor.nombreVendedor} por {orden.precioTotal} monedas.");
                return;
            }

            if (!monedero.PuedePagar(orden.precioTotal))
            {
                Debug.Log("[Mercado] No tienes suficientes monedas.");
                // Aquí podrías disparar un popup en UI
                return;
            }

            // Descuenta monedas (RQNF101.1)
            monedero.Pagar(orden.precioTotal);

            // TODO: aquí deberías sumar el objeto al inventario del jugador (RQNF101.2)

            Debug.Log($"[Mercado] ¡Compra exitosa! {orden.cantidad} x {orden.objeto.nombreMostrado} " +
                      $"por {orden.precioTotal} monedas. Vendedor: {orden.vendedor.nombreVendedor}");
        }
    }
}
