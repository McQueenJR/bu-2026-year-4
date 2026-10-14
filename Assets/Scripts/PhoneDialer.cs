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

        // ตรวจ PhoneManager
        if (phoneManager == null)
        {
            Debug.LogError("📞 PhoneDialer ไม่มี PhoneManager");
            return;
        }

        // ตรวจ CampManager
        if (CampManager.Instance == null)
        {
            Debug.LogError("📞 ไม่มี CampManager");
            return;
        }

        // ต้องกรอกเบอร์ก่อน
        if (string.IsNullOrEmpty(currentNumber))
        {
            displayText.text = "No Number";
            return;
        }

        // ค้นหาห้องจากเบอร์ที่ผู้เล่นกด
        CampRoom room =
            CampManager.Instance.GetRoomByPhone(currentNumber);

        // ไม่พบเบอร์นี้
        if (room == null)
        {
            displayText.text = "Wrong Number";
            Debug.Log($"📞 ไม่พบห้องของเบอร์ {currentNumber}");
            return;
        }

        // บันทึกห้องปลายทาง
        phoneManager.calledRoom = room;
        phoneManager.roomCode = room.roomCode;
        phoneManager.phoneNumber = room.phoneNumber;

        // เตรียมข้อมูลคนที่อยู่ในห้องปลายทาง
        phoneManager.RefreshPhoneCampDataForRoom(room);

        // เริ่มโทร
        isCalling = true;
        displayText.text = "Calling...";

        if (audioSource != null && callingSound != null)
            audioSource.PlayOneShot(callingSound);

        Debug.Log(
            $"📞 โทรหา Room {room.roomCode} " +
            $"เบอร์ {room.phoneNumber}"
        );

        StartCoroutine(CallCampSequence());
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
// ดึงสถานะ Camp ล่าสุดก่อนตรวจคนรับสาย
    phoneManager.RefreshPhoneCampDataForRoom(
        phoneManager.calledRoom
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