using UnityEngine;
using TMPro;

public class PhoneDialer : MonoBehaviour
{
    public TMP_Text displayText;

    private string currentNumber = "";

    void Start()
    {
        displayText.text = "";
    }

    public void PressNumber(string number)
    {
        
        if (currentNumber.Length >= 8)
            return;

        currentNumber += number;
        displayText.text = currentNumber;
    }

    public void Call()
    {
        if(currentNumber == "191")
        {
            displayText.text = "Calling...";
            Debug.Log("Calling Polist");
        }
        else
        {
            displayText.text = "Wrong Number";
        }
    }
    public void Backspace()
    {
        if (currentNumber.Length > 0)
        {
            currentNumber = currentNumber.Substring(0, currentNumber.Length - 1);
            displayText.text = currentNumber;
        }
    }

    public void Clear()
    {
        currentNumber = "";
        displayText.text = "";
    }
}