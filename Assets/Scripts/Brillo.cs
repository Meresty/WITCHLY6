using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Brillo : MonoBehaviour
{
    public float minBrillo = 0.7f;
    public float maxBrillo = 1.3f;
    public float velocidad = 2f;

    private Image sr;
    private Color colorBase;
    private float h, s, v;

    void Start()
    {
        sr = GetComponent<Image>();
        Color.RGBToHSV(colorBase, out h, out s, out v);
    }

    void Update()
    {
        float nuevoV = Mathf.Lerp(minBrillo, maxBrillo, Mathf.PingPong(Time.time * velocidad, 1));
        Color c = Color.HSVToRGB(h, s, Mathf.Clamp01(nuevoV));
        c.a = colorBase.a;
    }
}