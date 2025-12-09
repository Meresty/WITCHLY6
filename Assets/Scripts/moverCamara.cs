using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moverCamara : MonoBehaviour
{

    public float speed = 5f;
    public float edgeSize = 50f;

    public float minX = -10f;
    public float maxX = 10f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.mousePosition.x;
        float screenWidth = Screen.width;
        Vector3 pos = transform.position;

      
        if (mouseX < edgeSize)
        {
            pos.x -= speed * Time.deltaTime;
        }

        
        if (mouseX > screenWidth - edgeSize)
        {
            pos.x += speed * Time.deltaTime;
        }

       
        pos.x = Mathf.Clamp(pos.x, minX, maxX);

        transform.position = pos;
    }
}