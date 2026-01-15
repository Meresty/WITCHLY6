
using System;
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

        image.enabled = true;
        timerText.text = FormatTime(timer.TimeLeft);
        if (timer.hasFinished)
        {
            buttonCosechar.gameObject.SetActive(true);
            timerText.text = "Listo";
        }
        else
        {
            buttonCosechar.gameObject.SetActive(false);
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time - minutes * 60);
        return string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    public void Cosechar()
    {
        InvernaderoManager.Instance.CosecharPlanta(cultivoSlotInfo);
    }
}