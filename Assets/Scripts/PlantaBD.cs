
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlantaBD", menuName = "Game/Planta BD")]

public class PlantaBD : ScriptableObject
{
    public List<PlantasData> plantas;

    public PlantasData GetPlantas(PlantaTipo tipo)
    {
        return plantas.Find(p => p.plantaTipo == tipo);
    }
}