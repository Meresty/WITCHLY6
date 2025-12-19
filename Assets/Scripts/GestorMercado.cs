// Scripts/Mercado/GestorMercado.cs
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Witchly.Mercado
{
    public class GestorMercado : MonoBehaviour
    {
        [Header("Catálogo de objetos")]
        public DatosObjetoMercado[] plantas;
        public DatosObjetoMercado[] semillas;
        public DatosObjetoMercado[] sueros;

        [Header("Perfiles de vendedores (tabla Anexo)")]
        public PerfilVendedor[] vendedores;

        [Header("UI")]
        public RectTransform contenidoScrollView;   // Scroll View > Viewport > Content
        public CartaOrdenUI prefabCartaOrden;       // Prefab OrdenCompra
        public TMP_InputField buscadorInput;
        public TMP_Dropdown filtrosDropdown;

        [Header("Configuración de mercado")]
        [Range(1, 5)] public int ordenesPorObjeto = 2;
        [Tooltip("Horas reales entre refrescos de mercado")]
        public float intervaloRefrescoHoras = 2f;

        [Header("Monedero del jugador (para comprar)")]
        public PlayerWallet monedero;

        private readonly List<OrdenVenta> ordenesActuales = new();
        private float siguienteRefresco;

        private void Start()
        {
            GenerarMercadoCompleto();
            siguienteRefresco = Time.time + intervaloRefrescoHoras * 3600f;

            if (buscadorInput != null)
                buscadorInput.onValueChanged.AddListener(_ => RefrescarFiltro());

            if (filtrosDropdown != null)
                filtrosDropdown.onValueChanged.AddListener(_ => RefrescarFiltro());
        }

        private void Update()
        {
            if (Time.time >= siguienteRefresco)
            {
                GenerarMercadoCompleto();
                siguienteRefresco = Time.time + intervaloRefrescoHoras * 3600f;
            }
        }

        // ========== GENERACIÓN DE MERCADO ==========

        private void GenerarMercadoCompleto()
        {
            // Limpia UI
            foreach (Transform child in contenidoScrollView)
                Destroy(child.gameObject);

            ordenesActuales.Clear();

            GenerarOrdenesParaColeccion(plantas);
            GenerarOrdenesParaColeccion(semillas);
            GenerarOrdenesParaColeccion(sueros);

            // Crea cartas
            foreach (var orden in ordenesActuales)
                CrearCartaUI(orden);

            Debug.Log($"[Mercado] Generadas {ordenesActuales.Count} órdenes.");
        }

        private void GenerarOrdenesParaColeccion(DatosObjetoMercado[] catalogo)
        {
            if (catalogo == null || catalogo.Length == 0 || vendedores == null || vendedores.Length == 0)
                return;

            foreach (var objeto in catalogo)
            {
                for (int i = 0; i < ordenesPorObjeto; i++)
                {
                    var vendedor = vendedores[Random.Range(0, vendedores.Length)];
                    int cantidad = GenerarCantidadConProbabilidad();
                    ClasePrecio clase = ElegirClasePrecio();
                    float multiplicador = vendedor.ObtenerMultiplicador(objeto.tipo, clase);

                    int precioBaseTotal = objeto.precioBase * cantidad;   // cambia a tu campo real
                    int precioFinal = Mathf.Max(1, Mathf.RoundToInt(precioBaseTotal * multiplicador));

                    var orden = new OrdenVenta(objeto, vendedor, cantidad, precioFinal, clase);
                    ordenesActuales.Add(orden);
                }
            }
        }

        private int GenerarCantidadConProbabilidad()
        {
            float r = Random.value; // 0–1
            if (r < 0.5f) return 1;     // 50%
            if (r < 0.8f) return 2;     // +30% = 80%
            return 3;                   // 20% restante
        }

        private ClasePrecio ElegirClasePrecio()
        {
            float r = Random.value;
            if (r < 1f / 3f) return ClasePrecio.A;
            if (r < 2f / 3f) return ClasePrecio.B;
            return ClasePrecio.C;
        }

        private void CrearCartaUI(OrdenVenta orden)
        {
            var carta = Instantiate(prefabCartaOrden, contenidoScrollView);
            carta.Configurar(orden, monedero, OnOrdenComprada);
        }

        private void OnOrdenComprada(OrdenVenta orden)
        {
            // Aquí luego conectas con tu inventario para sumar la planta/suero al jugador
            Debug.Log($"[Mercado] Compraste {orden.cantidad} x {orden.NombreObjeto} a {orden.NombreVendedor} por {orden.precioTotal} monedas.");
        }

        // ========== FILTRO BÁSICO (opcional, si ya lo tienes puedes ignorar esto) ==========

        private void RefrescarFiltro()
        {
            string texto = buscadorInput != null ? buscadorInput.text.ToLowerInvariant() : string.Empty;
            int filtroIndex = filtrosDropdown != null ? filtrosDropdown.value : 0;

            foreach (Transform child in contenidoScrollView)
            {
                var carta = child.GetComponent<CartaOrdenUI>();
                if (carta == null) continue;

                bool visible = true;

                if (!string.IsNullOrEmpty(texto))
                {
                    string nombre = carta.name.ToLowerInvariant();
                    visible &= nombre.Contains(texto);
                }

                // Si tus filtros (dropdown) ya tienen lógica propia, puedes ignorar este ejemplo.
                child.gameObject.SetActive(visible);
            }
        }
    }
}
