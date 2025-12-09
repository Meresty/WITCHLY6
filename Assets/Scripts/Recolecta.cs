using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Recolecta : MonoBehaviour
{
    public string Collectible;
    public int Value = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Score playerScore = collision.GetComponent<Score>();
            if (playerScore != null)
            {
                playerScore.AddScore(Collectible, Value);
                Destroy(gameObject);
            }
        }
    }
}
