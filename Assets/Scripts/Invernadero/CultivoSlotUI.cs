
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CultivoSlotUI : MonoBehaviour
{
    public Image image;
    public TMP_Text timerText;
    public Button buttonCosechar;
    private Timer timer = new Timer(1);
    CultivoSlotInfo cultivoSlotInfo;

    public void Start()
    {
        image.enabled = false;
        timerText.text = "";
        buttonCosechar.gameObject.SetActive(false);
    }

    public void SetUp(Sprite newSprite, Timer timer, CultivoSlotInfo cultivoSlotInfo)
    {
        this.timer = timer;
        this.cultivoSlotInfo = cultivoSlotInfo;
        image.enabled = cultivoSlotInfo.isOccupied;
        image.sprite = newSprite;
    }

    private void Update()
    {
        if (cultivoSlotInfo == null || !cultivoSlotInfo.isOccupied)
        {
            timerText.text = "";
            image.enabled = false;
            buttonCosechar.gameObject.SetActive(false);
            return;
        }

        timerText.text = ((int)timer.TimeLeft).ToString();
        if (timer.hasFinished)
        {
            buttonCosechar.gameObject.SetActive(true);
            timerText.text = "Listo";
        }
    }

    public void Cosechar()
    {
        Debug.Log("[TODO] Cosechando planta...");
    }
}