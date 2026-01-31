using UnityEngine;

public class CerrarPanel : MonoBehaviour
{
    public GameObject panelToClose;

    public void Close()
    {
        if (panelToClose != null)
            panelToClose.SetActive(false);
    }
}
