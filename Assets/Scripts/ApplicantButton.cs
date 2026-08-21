using UnityEngine;

public class ApplicantButton : MonoBehaviour
{
    [Header("ใส่ NPCData ของคนนี้ (เว้นว่างได้)")]
    public NPCData npcData;

    public void OpenPhoto()
    {
        // ถ้าไม่มี Data ก็ไม่ทำอะไร
        if (npcData == null)
        {
            Debug.Log("ช่องนี้ยังไม่มีข้อมูล");
            return;
        }

        // ถ้าไม่มี Prefab รูป ก็ไม่ทำอะไร
        if (npcData.applicantPhotoPrefab == null)
        {
            Debug.Log("NPC นี้ยังไม่มีรูป");
            return;
        }

        ApplicantPhotoUI.Instance.ShowPhoto(npcData);
    }
}