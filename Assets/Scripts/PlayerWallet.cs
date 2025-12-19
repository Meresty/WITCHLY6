using UnityEngine;

namespace Witchly.Mercado
{
    public class PlayerWallet : MonoBehaviour
    {
        [Header("Monedero del jugador")]
        [SerializeField] private int monedasIniciales = 100;
        [SerializeField] private int monedasMaximas = 9999;

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
            if (cantidad <= 0)
                return;

            Monedas += cantidad;
            Monedas = Mathf.Clamp(Monedas, 0, monedasMaximas);
            OnWalletChanged?.Invoke(Monedas);
        }
    }
}
