using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainUi : MonoBehaviour
{
    [SerializeField] 
    private int _coins;
    [SerializeField]
    TextMeshPro uiCoins;

    public int Coins
    {
        get
        {
            Debug.Log("Hola");
            return _coins;
        }
        set
        {
            Debug.Log("Modifique coins");
            uiCoins.text = value.ToString();
            _coins = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Coins++;
        }
    }
}
