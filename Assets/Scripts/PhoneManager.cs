using UnityEngine;
using TMPro;
using System.Collections;

public class PhoneManager : MonoBehaviour
{
    public GameObject phonePanel;
    public GameObject telaphone;
    public PhoneDialer phoneDialer;   // เพิ่ม: reference ไปยัง PhoneDialer

    public void OpenPhone()
    {
        if (!GameManager.Instance.emergencyMode)
        {
            Debug.Log("ต้องปิดประตูก่อน");
            return;
        }
        
        // กันเปิดโทรศัพท์ซ้ำ ระหว่างที่ตำรวจกำลังมา
        if (GameManager.Instance.isPoliceSequenceActive)
        {
            Debug.Log("กำลังอยู่ระหว่างเรียกตำรวจ เปิดโทรศัพท์ไม่ได้ตอนนี้");
            return;
        }

        telaphone.SetActive(false);
        phonePanel.SetActive(true);
    }

    // เรียกจากปุ่มปิดที่ผู้เล่นกดเอง — บล็อกถ้ากำลังอยู่ระหว่างโทร
    public void ClosePhone()
    {
        if (phoneDialer != null && phoneDialer.IsCalling)
        {
            Debug.Log("กำลังโทรอยู่ ผู้เล่นปิดโทรศัพท์เองไม่ได้ตอนนี้");
            return;
        }
 
        ForceClosePhone();
    }
 
    // ปิดโทรศัพท์แบบบังคับ ไม่เช็คสถานะ isCalling
    // ใช้ตอนระบบสั่งปิดเอง (เช่นหลัง dialog แรกของตำรวจจบ)
    public void ForceClosePhone()
    {
        phonePanel.SetActive(false);
        telaphone.SetActive(true);
 
        if (phoneDialer != null)
            phoneDialer.Clear();
    }
}