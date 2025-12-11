using UnityEngine;

namespace Witchly.Mercado
{
    public class PlayerWallet : MonoBehaviour
    {
        [Header("Monedas actuales del jugador")]
        public int monedas = 200; // pon lo que quieras de inicio

        public bool PuedePagar(int costo)
        {
            return monedas >= costo;
        }

        public void Pagar(int costo)
        {
            if (costo < 0) return;
            monedas = Mathf.Max(0, monedas - costo);
        }

        public void AgregarMonedas(int cantidad)
        {
            if (cantidad < 0) return;
            monedas += cantidad;
        }
    }
}
