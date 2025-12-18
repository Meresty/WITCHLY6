using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Witchly.Mercado;   // <-- añade esto

namespace Witchly.Mercado
{
    public class PlayerWallet : MonoBehaviour
    {
        [Header("Monedas actuales del jugador")]
        public int monedas = 200; // pon lo que quieras de inicio

        // ---------- MÉTODOS EXISTENTES ----------

        public bool PuedePagar(int costo)
        {
            return monedas >= costo;
        }

        public void Pagar(int costo)
        {
            if (costo < 0) return;
            monedas = Mathf.Max(0, monedas - costo);
            // Aquí podrías disparar actualización de UI si tienes una
            // ActualizarUI();
        }

        public void AgregarMonedas(int cantidad)
        {
            if (cantidad < 0) return;
            monedas += cantidad;
            // Aquí también podrías actualizar UI
            // ActualizarUI();
        }

        // ---------- MÉTODOS NUEVOS (wrapper en inglés) ----------

        /// <summary>
        /// Wrapper para que otros sistemas (como el Buzón) puedan
        /// sumar monedas usando "AddCoins".
        /// </summary>
        public void AddCoins(int amount)
        {
            AgregarMonedas(amount);
        }

        /// <summary>
        /// Wrapper opcional por si algún sistema quiere restar usando inglés.
        /// </summary>
        public void RemoveCoins(int amount)
        {
            Pagar(amount);
        }
    }
}
