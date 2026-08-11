using UnityEngine;
using TMPro;
using System.Collections;

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
        if (currentNumber == "191")
        {
            displayText.text = "Calling...";
            Debug.Log("Calling Police");

            StartCoroutine(CallPoliceSequence());
        }
        else
        {
            displayText.text = "Wrong Number";
        }
    }

    private IEnumerator CallPoliceSequence()
    {
        yield return new WaitForSeconds(2f);

        // แสดง Dialog ตอนโทร 191
        GameManager.Instance.StartPoliceCallDialog();

        // รอจน Dialog จบ
        yield return new WaitUntil(() =>
            !GameManager.Instance.dialogManager.IsDialogOpen()
        );

        // หลัง Dialog จบ → เริ่มกระบวนการตำรวจ
        GameManager.Instance.OnPoliceCalled();
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