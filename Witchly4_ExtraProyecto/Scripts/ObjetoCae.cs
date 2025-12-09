using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjetoCae : MonoBehaviour
{
    public float VelMin = 2f;
    public float VelMax = 5f;
    private float VelCaida;

    private bool yaAtrapado = false; 

    public string Collectible = "Meteorito";
    public int Value = 1;

    void Start()
    {
        VelCaida = Random.Range(VelMin, VelMax);
    }

    void Update()
    {
        transform.Translate(Vector3.down * VelCaida * Time.deltaTime);

        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !yaAtrapado) 
        {
            yaAtrapado = true;
            Score playerScore = collision.GetComponent<Score>();
            if (playerScore != null)
            {
                playerScore.AddScore(Collectible, Value);
                Destroy(gameObject);
            }
        }
    }
}