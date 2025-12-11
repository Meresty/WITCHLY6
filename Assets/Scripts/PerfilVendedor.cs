using UnityEngine;

namespace Witchly.Mercado
{
    public enum ClasePrecio { A, B, C }

    [System.Serializable]
    public struct RangoMultiplicador
    {
        public float minimo;
        public float maximo;
    }

    [CreateAssetMenu(menuName = "Witchly/Mercado/Perfil Vendedor")]
    public class PerfilVendedor : ScriptableObject
    {
        public string nombreVendedor;

        [Header("Plantas y semillas (multiplicadores)")]
        public RangoMultiplicador claseAPlantasSemillas = new RangoMultiplicador { minimo = 0.5f, maximo = 0.9f };
        public float claseBPlantasSemillas = 1f;
        public RangoMultiplicador claseCPlantasSemillas = new RangoMultiplicador { minimo = 1.1f, maximo = 1.5f };

        [Header("Sueros (multiplicadores)")]
        public RangoMultiplicador claseASueros = new RangoMultiplicador { minimo = 0.7f, maximo = 0.9f };
        public float claseBSueros = 1f;
        public RangoMultiplicador claseCSueros = new RangoMultiplicador { minimo = 1.1f, maximo = 1.2f };

        public float ObtenerMultiplicador(TipoObjetoMercado tipo, ClasePrecio clase)
        {
            bool esPlantaOSemilla = tipo == TipoObjetoMercado.Planta || tipo == TipoObjetoMercado.Semilla;

            switch (clase)
            {
                case ClasePrecio.A:
                    return esPlantaOSemilla
                        ? Random.Range(claseAPlantasSemillas.minimo, claseAPlantasSemillas.maximo)
                        : Random.Range(claseASueros.minimo, claseASueros.maximo);

                case ClasePrecio.B:
                    return esPlantaOSemilla ? claseBPlantasSemillas : claseBSueros;

                case ClasePrecio.C:
                    return esPlantaOSemilla
                        ? Random.Range(claseCPlantasSemillas.minimo, claseCPlantasSemillas.maximo)
                        : Random.Range(claseCSueros.minimo, claseCSueros.maximo);

                default:
                    return 1f;
            }
        }
    }
}
