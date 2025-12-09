using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject Meteorito;
    public float tiempoEntreSpawn = 1.5f;
    public float ySpawn = 200f;

    private Canvas canvas;
    private float screenWidth;

    void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No esta el Canvas");
            return;
        }

        screenWidth = canvas.GetComponent<RectTransform>().rect.width * canvas.transform.localScale.x;

        StartCoroutine(SpawnMeteoritos());
    }

    IEnumerator SpawnMeteoritos()
    {
        while (true)
        {
            float xRandom = Random.Range(-screenWidth / 2, screenWidth / 2);
            Vector3 spawnPos = new Vector3(xRandom, ySpawn, 0);

            GameObject nuevoMeteorito = Instantiate(Meteorito, canvas.transform);
            nuevoMeteorito.transform.localPosition = spawnPos;

            Debug.Log($"Posición: {spawnPos}, Ancho pantalla: {screenWidth}");

            yield return new WaitForSeconds(tiempoEntreSpawn);
        }
    }
}