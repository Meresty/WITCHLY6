
using UnityEngine;

[CreateAssetMenu(fileName = "PlantaBD", menuName = "Game/Planta BD")]

public class PlantaBD : ScriptableObject
{
    public PlantasInfo[] plantas;

    public PlantasInfo GetPlantas(PlantaTipo tipo)
    {
        foreach (var planta in plantas)
        {
            if (planta.plantaTipo == tipo)
                return planta;
        }
        return null;
    }
}