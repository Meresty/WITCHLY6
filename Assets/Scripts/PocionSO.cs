using UnityEngine;

[CreateAssetMenu(fileName = "Nueva Pocion", menuName = "Inventario/Pocion")]
public class PocionSO : ScriptableObject
{
    public string pocionNombre;
    public Sprite icon;
    public string efecto;
    public ItemSO suero;
    public ItemSO ingrediente1;
    public ItemSO ingrediente2;


    public float precioBase = 100f;
    [System.NonSerialized]
    public float precioFinal;
    public ItemSO itemPocion;
}
