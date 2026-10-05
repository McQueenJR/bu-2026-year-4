using UnityEngine;
using TMPro;
using System.Collections;

public class PhoneDialer : MonoBehaviour
{
    public TMP_Text displayText;

    public PhoneManager phoneManager;

    private string currentNumber = "";

    private bool isCalling = false;
    public bool IsCalling => isCalling;

    public AudioSource audioSource;
    public AudioClip numberButtonSound;
    public AudioClip callingSound;
    public AudioClip noAnswerSound;


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

        if (audioSource != null &&
            numberButtonSound != null)
        {
            audioSource.PlayOneShot(numberButtonSound);
        }
    }


    // ==================================================
    // CALL
    // ==================================================

    public void Call()
    {
        // กำลังโทรอยู่แล้ว
        if (isCalling)
            return;

        // ไม่มี PhoneManager
        if (phoneManager == null)
        {
            Debug.LogError(
                "📞 PhoneDialer ไม่มี PhoneManager"
            );

            return;
        }

        // ไม่มีเบอร์ห้อง
        if (string.IsNullOrEmpty(phoneManager.phoneNumber))
        {
            Debug.LogWarning(
                "📞 ยังไม่มีเบอร์ห้องสำหรับ NPC คนนี้"
            );

            displayText.text = "No Number";
            return;
        }

        // ==========================================
        // เช็กว่าเบอร์ที่กดตรงกับห้องของ NPC หรือไม่
        // ==========================================

        if (currentNumber == phoneManager.phoneNumber)
        {
            isCalling = true;

            displayText.text = "Calling...";

            if (audioSource != null &&
                callingSound != null)
            {
                audioSource.PlayOneShot(callingSound);
            }

            Debug.Log(
                $"📞 โทรหา Room {phoneManager.roomCode}"
            );

            StartCoroutine(CallCampSequence());

            return;
        }

        // ==========================================
        // เบอร์ผิด
        // ==========================================

        displayText.text = "Wrong Number";

        Debug.Log(
            $"📞 เบอร์ผิด: {currentNumber}"
        );
    }


    // ==================================================
    // โทรเข้า Camp
    // ==================================================

    private IEnumerator CallCampSequence()
{
    // ==========================================
    // รอจนเสียงรอสายเล่นจบ
    // ==========================================

    if (callingSound != null)
    {
        yield return new WaitForSeconds(
            callingSound.length
        );
    }
    else
    {
        // กันกรณีไม่ได้ใส่เสียง
        yield return new WaitForSeconds(2f);
    }


    // ==========================================
    // ตรวจข้อมูลในห้อง
    // ==========================================

    Debug.Log(
        $"📞 เสียงรอสายจบแล้ว"
    );

    Debug.Log(
        $"📞 เชื่อมต่อ Room: {phoneManager.roomCode}"
    );

    Debug.Log(
        $"📞 Target: {phoneManager.targetName}"
    );

    Debug.Log(
        $"📞 คนรับสาย: {phoneManager.answeringName}"
    );

    Debug.Log(
        $"📞 คนอยู่ในห้อง: " +
        $"{phoneManager.peopleAtHomeCount} คน"
    );


    // ==========================================
    // ไม่มีคนอยู่ในห้อง
    // ==========================================

    if (phoneManager.peopleAtHomeCount == 0)
    {
        Debug.Log(
            "📞 ไม่มีคนรับสาย"
        );

        displayText.text = "No Answer";

        if (audioSource != null &&
            noAnswerSound != null)
        {
            audioSource.PlayOneShot(
                noAnswerSound
            );

            // รอเสียงตุ๊ดๆ จบ
            yield return new WaitForSeconds(
                noAnswerSound.length
            );
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // วางสาย
        FinishCalling();

        yield break;
    }


    // ==========================================
    // มีคนอยู่ในห้อง
    // ==========================================

    Debug.Log(
        $"📞 มีคนอยู่ในห้อง → " +
        $"{phoneManager.answeringName} รับสาย"
    );


    // ==========================================
    // เอาโทรศัพท์ลง
    // ==========================================

    phoneManager.phonePanel.SetActive(false);

    phoneManager.telaphone.SetActive(true);


    // ==========================================
    // เปิด Dialog ของคนรับสาย
    // ==========================================

    phoneManager.StartPhoneGreeting();
}
    
    
    // ==================================================
    // BACKSPACE
    // ==================================================

    public void Backspace()
    {
        if (isCalling)
            return;

        if (currentNumber.Length > 0)
        {
            currentNumber =
                currentNumber.Substring(
                    0,
                    currentNumber.Length - 1
                );

            displayText.text = currentNumber;
        }
    }


    // ==================================================
    // CLEAR
    // ==================================================

    public void Clear()
    {
        currentNumber = "";
        displayText.text = "";
    }
    
    public void FinishCalling()
    {
        isCalling = false;

        Debug.Log("📞 วางสายแล้ว");

        if (phoneManager != null)
        {
            phoneManager.ForceClosePhone();
        }
    }
}