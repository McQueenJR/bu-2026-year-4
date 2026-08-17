using UnityEngine;
using TMPro;
using System.Collections;

public class PhoneDialer : MonoBehaviour
{
    public TMP_Text displayText;

    // เพิ่ม: ใช้สั่งปิดโทรศัพท์เองหลัง dialog แรกของตำรวจจบ
    public PhoneManager phoneManager;
    
    private string currentNumber = "";
    
    // true ตั้งแต่กด Call ถูกต้อง จนกว่า dialog แรกของตำรวจจะปิดและระบบปิดโทรศัพท์ให้เองแล้ว
    private bool isCalling = false;
    public bool IsCalling => isCalling;
    

    void Start()
    {
        displayText.text = "";
    }

    public void PressNumber(string number)
    {
        // กำลังโทรอยู่ ห้ามกดเลขเพิ่ม
        if (isCalling)
            return;
        
        if (currentNumber.Length >= 8)
            return;

        currentNumber += number;
        displayText.text = currentNumber;
    }

    public void Call()
    {
        // กำลังโทรอยู่แล้ว ห้ามกด Call ซ้อน
        if (isCalling)
            return;
        
        if (currentNumber == "191")
        {
            isCalling = true;
            
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

        // รอจน Dialog แรกปิด
        yield return new WaitUntil(() =>
            !GameManager.Instance.dialogManager.IsDialogOpen()
        );
        
        // Dialog แรกจบแล้ว → ปิดโทรศัพท์ให้เองทันที (ผู้เล่นไม่ต้องกดปิดเอง)
        if (phoneManager != null)
            phoneManager.ForceClosePhone();
        
        // ปลดล็อกโทรศัพท์ ให้ผู้เล่นกลับมาใช้งานปกติได้แล้ว
        isCalling = false;

        // เริ่มกระบวนการตำรวจ
        GameManager.Instance.OnPoliceCalled();
    }

    public void Backspace()
    {
        // กำลังโทรอยู่ ห้ามลบเลข
        if (isCalling)
            return;
        
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