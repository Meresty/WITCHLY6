using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UI_OrdenDeCompra : MonoBehaviour
{
    [SerializeField] int cantidadObjeto;
    [SerializeField] int costoObjeto;
    [SerializeField] Image iconoObjeto;
    [SerializeField] TextMeshProUGUI nombreVendedor;
    [SerializeField] TextMeshProUGUI nombreObjeto;
    [SerializeField] TextMeshProUGUI text_cantidadObjeto;
    [SerializeField] TextMeshProUGUI text_costoObjeto;
    
    public void ActualizarInformacion(int Cantidad, string NombreVendedor, string NombreObjeto, int CostoObjeto)
    {
        cantidadObjeto = Cantidad;
        costoObjeto = CostoObjeto;
        text_cantidadObjeto.text = Cantidad.ToString();
        text_costoObjeto.text = CostoObjeto.ToString();
        nombreVendedor.text = NombreVendedor;
        nombreObjeto.text = NombreObjeto;
    }

    public void ComprarObjeto()
    {
        if(cantidadObjeto < 0/*Agregar condicional de revisar dinero*/)
        {
            Debug.Log("Se compro objeto");
            cantidadObjeto--;
        }
        else
        {
            Debug.Log("No se compro");
        }
    }
}
