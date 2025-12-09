using UnityEngine;
using UnityEngine.EventSystems;

public class SigueMouse : MonoBehaviour
{
    public float speed = 1;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
       


        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(mousePos);


        Vector3 newPosition = transform.position;
        newPosition.x = mouseWorldPos.x; 

        transform.position = Vector3.Lerp(transform.position, newPosition, speed * Time.deltaTime);
    }
}