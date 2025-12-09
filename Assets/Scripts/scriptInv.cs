using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scriptInv : MonoBehaviour
{
    // Start is called before the first frame update
    void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().color = new Color(0xA6 / 255f, 0x90 / 255f, 0xCD / 255f, 1f);
    }
    void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}
