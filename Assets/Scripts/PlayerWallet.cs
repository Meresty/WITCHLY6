using UnityEngine;
using TMPro;

namespace Witchly.Mercado
{
    public class PlayerWallet : MonoBehaviour
    {
        [Header("Monedero del jugador")]
        [SerializeField] private int monedasIniciales = 100;
        [SerializeField] private int monedasMaximas = 9999;

        public int Monedas { get; private set; }

   
        public System.Action<int> OnWalletChanged;

        private void Awake()
        {
            Monedas = Mathf.Clamp(monedasIniciales, 0, monedasMaximas);
            OnWalletChanged?.Invoke(Monedas);
        }

        public bool PuedePagar(int cantidad)
        {
            return cantidad > 0 && Monedas >= cantidad;
        }


        public bool Pagar(int cantidad)
        {
            if (cantidad <= 0)
                return false;

            if (!PuedePagar(cantidad))
            {
                Debug.LogWarning(
                    $"[PlayerWallet] No alcanzan las monedas. Tienes {Monedas}, precio {cantidad}"
                );
                return false;
            }

            Monedas -= cantidad;
            Monedas = Mathf.Clamp(Monedas, 0, monedasMaximas);
            OnWalletChanged?.Invoke(Monedas);
            return true;
        }


        public void AgregarMonedas(int cantidad)
        {
            if (cantidad <= 0)
                return;

            Monedas += cantidad;
            Monedas = Mathf.Clamp(Monedas, 0, monedasMaximas);
            OnWalletChanged?.Invoke(Monedas);
        }





        public void AddCoins(int amount)
        {
            AgregarMonedas(amount);
        }

        public bool RemoveCoins(int amount)
        {
            return Pagar(amount);
        }
    }
}
