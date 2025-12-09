using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Flotar : MonoBehaviour
{
    [Header("Configuración de Flotación")]
    public float amplitud = 0.5f;   
    public float frecuencia = 1f;   
    public bool flotarEnY = true;

    private Vector3 posicionInicial;

    void Start()
    {
     
        posicionInicial = transform.position;
    }

    void Update()
    {
        float movimiento = Mathf.Sin(Time.time * frecuencia) * amplitud;

        if (flotarEnY)
        {
            transform.position = new Vector3(
                posicionInicial.x,
                posicionInicial.y + movimiento,
                posicionInicial.z
            );
        }
        else
        {
            transform.position = new Vector3(
                posicionInicial.x + movimiento,
                posicionInicial.y,
                posicionInicial.z
            );
        }
    }
}
