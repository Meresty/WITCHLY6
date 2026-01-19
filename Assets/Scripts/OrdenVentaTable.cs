
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Witchly.Mercado
{
    [CreateAssetMenu(menuName = "Witchly/Mercado/OrdenVentaTable", fileName = "OrdenVentaTable")]
    public class OrdenVentaTable : ScriptableObject
    {
        public enum TipoObjeto
        {
            Planta,
            Suero
        }

        [Serializable]
        public class Row
        {
            public string nombreVendedor;
            public string nombreObjeto;
            public TipoObjeto tipo;

            public int cantidad;
            public int precioUnitario;
            public int precioTotal;

            public Sprite icono; 
        }

        [Header("Referencias a tu Base de Datos")]
        public ScriptableObject plantaBD;  
        public ScriptableObject sueroBD;   

        [Header("Vendedores (texto simple)")]
        public List<string> vendedores = new List<string>() { "Vendedor A", "Vendedor B" };

        [Header("Config de generación")]
        [Min(1)] public int minCantidad = 1;
        [Min(1)] public int maxCantidad = 5;

        [Tooltip("Si tu BD tiene varios precios y quieres forzar cuál usar, deja estos nombres en orden de prioridad.")]
        public List<string> camposPrecioPreferidosVenta = new List<string>()
        {
            "precioVentaEstandar",
            "precioVenta",
            "precio",
            "PrecioVentaEstandar",
            "PrecioVenta",
            "Precio"
        };

        [Tooltip("Si no encuentra precio en la BD, usará este valor.")]
        public int precioFallback = 1;

        [Header("Tabla")]
        public List<Row> rows = new List<Row>();

       

        [ContextMenu("Rebuild From DB (Plantas + Sueros)")]
        public void RebuildFromDB()
        {
            rows.Clear();

            
            BuildRowsFromDB(plantaBD, TipoObjeto.Planta, "plantas", "Plantas");

            
            BuildRowsFromDB(sueroBD, TipoObjeto.Suero, "sueros", "Sueros");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif

            Debug.Log($"[OrdenVentaTable] Rebuild listo. Rows: {rows.Count}");
        }

        [ContextMenu("Print Rows To Console")]
        public void PrintRows()
        {
            if (rows == null || rows.Count == 0)
            {
                Debug.LogWarning("[OrdenVentaTable] No hay rows. Usa 'Rebuild From DB' primero.");
                return;
            }

            Debug.Log("=== ORDENVENTATABLE ===");
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                Debug.Log($"{i + 1}) Vendedor: {r.nombreVendedor} | Item: {r.nombreObjeto} | Cant: {r.cantidad} | Unit: {r.precioUnitario} | Total: {r.precioTotal}");
            }
        }

       

        private void BuildRowsFromDB(ScriptableObject db, TipoObjeto tipo, params string[] listMemberCandidates)
        {
            if (db == null)
            {
                Debug.LogWarning($"[OrdenVentaTable] DB null para tipo {tipo}. No se generó nada.");
                return;
            }

            var enumerable = GetEnumerableMember(db, listMemberCandidates);
            if (enumerable == null)
            {
                Debug.LogWarning($"[OrdenVentaTable] No pude encontrar lista en {db.name} con miembros: {string.Join(", ", listMemberCandidates)}");
                return;
            }

            int vendIndex = 0;

            foreach (var elem in enumerable)
            {
                if (elem == null) continue;

                // nombre  item
                string nombreObj = GetNombreElemento(elem);
                if (string.IsNullOrWhiteSpace(nombreObj))
                    nombreObj = "(SinNombre)";

                // precio 
                int precioUnit = ResolvePrecio(elem, camposPrecioPreferidosVenta, precioFallback);

                // cantidad 
                int cantidad = UnityEngine.Random.Range(minCantidad, maxCantidad + 1);
                int total = precioUnit * cantidad;

                string vend = PickVendedor(vendIndex);
                vendIndex++;

                rows.Add(new Row
                {
                    nombreVendedor = vend,
                    nombreObjeto = nombreObj,
                    tipo = tipo,
                    cantidad = cantidad,
                    precioUnitario = precioUnit,
                    precioTotal = total,
                    icono = TryGetSprite(elem)
                });
            }
        }

        private string PickVendedor(int i)
        {
            if (vendedores == null || vendedores.Count == 0) return "SinVendedor";
            return vendedores[i % vendedores.Count];
        }

        private IEnumerable GetEnumerableMember(object obj, params string[] memberNames)
        {
            if (obj == null) return null;

            var t = obj.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (var name in memberNames)
            {
               
                var fi = t.GetField(name, flags);
                if (fi != null)
                {
                    var val = fi.GetValue(obj);
                    if (val is IEnumerable en) return en;
                }

                
                var pi = t.GetProperty(name, flags);
                if (pi != null)
                {
                    var val = pi.GetValue(obj);
                    if (val is IEnumerable en) return en;
                }
            }

            return null;
        }

        private string GetNombreElemento(object elem)
        {
            
            if (elem is UnityEngine.Object uo)
                return uo.name;

            
            string n = GetStringMember(elem, "nombre") ??
                       GetStringMember(elem, "Nombre") ??
                       GetStringMember(elem, "itemNombre") ??
                       GetStringMember(elem, "ItemNombre");

            return n;
        }

        private Sprite TryGetSprite(object elem)
        {
            
            return GetSpriteMember(elem, "icon") ??
                   GetSpriteMember(elem, "Icon") ??
                   GetSpriteMember(elem, "icono") ??
                   GetSpriteMember(elem, "Icono");
        }

        private int ResolvePrecio(object elem, List<string> preferidos, int fallback)
        {
            if (elem == null) return fallback;

            
            if (preferidos != null)
            {
                for (int i = 0; i < preferidos.Count; i++)
                {
                    var name = preferidos[i];
                    var v = GetIntMember(elem, name);
                    if (v.HasValue) return v.Value;

                    
                    var f = GetFloatMember(elem, name);
                    if (f.HasValue) return Mathf.RoundToInt(f.Value);
                }
            }

            return fallback;
        }

        
        private string GetStringMember(object obj, string member)
        {
            var t = obj.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var f = t.GetField(member, flags);
            if (f != null && f.FieldType == typeof(string)) return (string)f.GetValue(obj);

            var p = t.GetProperty(member, flags);
            if (p != null && p.PropertyType == typeof(string)) return (string)p.GetValue(obj);

            return null;
        }

        private int? GetIntMember(object obj, string member)
        {
            var t = obj.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var f = t.GetField(member, flags);
            if (f != null && f.FieldType == typeof(int)) return (int)f.GetValue(obj);

            var p = t.GetProperty(member, flags);
            if (p != null && p.PropertyType == typeof(int)) return (int)p.GetValue(obj);

            return null;
        }

        private float? GetFloatMember(object obj, string member)
        {
            var t = obj.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var f = t.GetField(member, flags);
            if (f != null && f.FieldType == typeof(float)) return (float)f.GetValue(obj);

            var p = t.GetProperty(member, flags);
            if (p != null && p.PropertyType == typeof(float)) return (float)p.GetValue(obj);

            return null;
        }

        private Sprite GetSpriteMember(object obj, string member)
        {
            var t = obj.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var f = t.GetField(member, flags);
            if (f != null && typeof(Sprite).IsAssignableFrom(f.FieldType)) return (Sprite)f.GetValue(obj);

            var p = t.GetProperty(member, flags);
            if (p != null && typeof(Sprite).IsAssignableFrom(p.PropertyType)) return (Sprite)p.GetValue(obj);

            return null;
        }
    }
}
