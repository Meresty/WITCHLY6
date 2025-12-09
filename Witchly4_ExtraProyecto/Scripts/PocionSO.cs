using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pocion", menuName = "Combinaciones/Potion")]
public class PocionSO : ScriptableObject
{
    public string pocionNombre;
    public Sprite icon; 
    public string efecto;
    public int precio;
    public ItemSO suero;
    public ItemSO ingrediente1;
    public ItemSO ingrediente2;
}