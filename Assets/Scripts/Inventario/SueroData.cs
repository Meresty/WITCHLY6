using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SueroData : ItemData
{
    public override Sprite GetBaseSprite => sueroSprite;

    [Header("Visuals")]
    public Sprite sueroSprite;

}
