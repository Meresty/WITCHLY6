using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public int totalScore = 0; 
    public TextMeshProUGUI Txt_Score;   


    public void AddScore(string collectible, int value)
    {
        totalScore += value;

        Debug.Log("Recolectaste: " + collectible + " +" + value + " puntos");
        Debug.Log("Puntaje total: " + totalScore);

        if (Txt_Score != null)
        {
            Txt_Score.text = "0";
            Txt_Score.text = " " + totalScore;
        }
    }

    public void ResetScore()
    {
        totalScore = 0;
        if (Txt_Score != null)
        {
            Txt_Score.text = " ";
        }
    }
}
