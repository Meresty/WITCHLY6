
using UnityEngine;

namespace Witchly.Mercado
{
    public class OrdenVentaTablePrinter : MonoBehaviour
    {
        public OrdenVentaTable table;
        public bool rebuildOnStart = true;
        public bool printOnStart = true;

        private void Start()
        {
            if (table == null)
            {
                Debug.LogError("[OrdenVentaTablePrinter] No asignaste 'table' en el Inspector.");
                return;
            }

            if (rebuildOnStart)
                table.RebuildFromDB();

            if (printOnStart)
                table.PrintRows();
        }

        [ContextMenu("Rebuild + Print Now")]
        public void RebuildAndPrint()
        {
            if (table == null) return;
            table.RebuildFromDB();
            table.PrintRows();
        }
    }
}
