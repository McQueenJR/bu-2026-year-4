using TMPro;
using UnityEngine;

public class ClockManager : MonoBehaviour
{
    public TMP_Text timeText;

    public void SetHour(int hour)
    {
        timeText.text = hour.ToString("00") + ":00";
    }
}