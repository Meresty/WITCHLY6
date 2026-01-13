
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SueroBD", menuName = "Game/Suero BD")]

public class SueroDB : ScriptableObject
{
    public List<SueroData> sueros = new List<SueroData>();
    public SueroData GetSueroByName(string sueroName)
    {
        return sueros.Find(s => s.nombre == sueroName);
    }
}