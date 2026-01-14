
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlantaBD", menuName = "Game/Planta BD")]

public class PlantaBD : ScriptableObject
{
    public List<PlantData> plantas;

    public PlantData GetPlantas(PlantaTipo tipo)
    {
        return plantas.Find(p => p.plantaTipo == tipo);
    }
}