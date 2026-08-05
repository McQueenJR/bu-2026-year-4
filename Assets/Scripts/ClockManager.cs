using TMPro;
using UnityEngine;

public class ClockManager : MonoBehaviour
{
    public TMP_Text timeText;

    public int currentHour = 12;

    void Start()
    {
        UpdateClock();
    }

    public void NextHour()
    {
        currentHour++;

        UpdateClock();

        Debug.Log(currentHour + ":00");
    }

    void UpdateClock()
    {
        timeText.text = currentHour.ToString("00") + ":00";
    }
}