using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SistemaEnergia : MonoBehaviour
{
    [Header("UI")]
    public Slider energiaSlider;
    public TMP_Text energiaTexto;

    [Header("Valores")]
    public int energiaMax = 100; 
    public int energiaActual;

    void Start()
    {
        energiaActual = energiaMax;
        ActualizarUI();
    }
    public void GastarEnergia(int cantidad)
    {
        energiaActual -= cantidad;
        energiaActual = Mathf.Clamp(energiaActual, 0, energiaMax); 
        ActualizarUI();
    }

    void ActualizarUI()
    {
        energiaSlider.value = (float)energiaActual / energiaMax;
        energiaTexto.text = energiaActual + "%";
    }

    public bool PuedePlantar()
    {
        return energiaActual > 1; 
    }
}