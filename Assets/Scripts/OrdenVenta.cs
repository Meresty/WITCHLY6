using System.Collections.Generic;
using UnityEngine;

namespace Witchly.Mercado
{
    [CreateAssetMenu(fileName = "OrdenVentaTable", menuName = "Witchly/Mercado/OrdenVentaTable")]
    public class OrdenVentaTable : ScriptableObject
    {
        [Header("Tabla de órdenes")]
        public List<OrdenVenta> ordenes = new List<OrdenVenta>();

        [Header("Editor helpers")]
        public bool autoBakeEnEditor = true;

        private void OnValidate()
        {
            if (!autoBakeEnEditor) return;
            if (ordenes == null) return;

            for (int i = 0; i < ordenes.Count; i++)
            {
                if (ordenes[i] == null) continue;
                ordenes[i].BakeUIFields();
            }
        }
    }
}
