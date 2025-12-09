using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventButtonUI : MonoBehaviour
{
    public Button eventButton;
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI timerText;

    public Action onClicked;
    public Action onExpired;

    private float endTime;

    public void Initialize(int durationSeconds)
    {
        endTime = Time.time + durationSeconds;
        if (labelText) labelText.text = "Evento Mundial";
        if (eventButton) eventButton.onClick.AddListener(OnClick);

        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        while (Time.time < endTime)
        {
            float remaining = endTime - Time.time;
            TimeSpan t = TimeSpan.FromSeconds(remaining);
            if (timerText)
                timerText.text = $"{t.Minutes:D2}:{t.Seconds:D2}";
            yield return new WaitForSeconds(1f);
        }

        onExpired?.Invoke();
        Destroy(gameObject);
    }

    private void OnClick()
    {
        onClicked?.Invoke();
        Destroy(gameObject);
    }
}
