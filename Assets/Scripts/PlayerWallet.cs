using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Witchly.Mercado;   // <-- añade esto

namespace Witchly.Mercado
{
    public class PlayerWallet : MonoBehaviour
    {
        [Header("Monedero del jugador")]
        [SerializeField] private int monedasIniciales = 100;
        [SerializeField] private int monedasMaximas = 9999;

        // ---------- MÉTODOS EXISTENTES ----------

        public bool PuedePagar(int costo)
        public int Monedas { get; private set; }

        // Evento opcional por si luego quieres actualizar HUD, etc.
        public System.Action<int> OnWalletChanged;

        private void Awake()
        {
            Monedas = Mathf.Clamp(monedasIniciales, 0, monedasMaximas);
        }

        /// <summary>
        /// ¿El jugador tiene suficientes monedas para pagar esta cantidad?
        /// </summary>
        public bool PuedePagar(int cantidad)
        {
            return Monedas >= cantidad;
        }

        /// <summary>
        /// Cobra la cantidad indicada (si alcanza).
        /// </summary>
        public void Pagar(int cantidad)
        {
            if (costo < 0) return;
            monedas = Mathf.Max(0, monedas - costo);
            // Aquí podrías disparar actualización de UI si tienes una
            // ActualizarUI();
            if (cantidad <= 0)
                return;

            if (!PuedePagar(cantidad))
            {
                Debug.LogWarning($"[PlayerWallet] No alcanzan las monedas. Tienes {Monedas}, precio {cantidad}");
                return;
            }

            Monedas -= cantidad;
            Monedas = Mathf.Clamp(Monedas, 0, monedasMaximas);
            OnWalletChanged?.Invoke(Monedas);
        }

        /// <summary>
        /// Para cuando quieras darle monedas al jugador (recompensas, etc.).
        /// </summary>
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
            if (cantidad <= 0)
                return;

            Monedas += cantidad;
            Monedas = Mathf.Clamp(Monedas, 0, monedasMaximas);
            OnWalletChanged?.Invoke(Monedas);
        }
    }
}
