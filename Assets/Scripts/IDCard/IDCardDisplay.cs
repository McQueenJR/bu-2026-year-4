using UnityEngine;

public class IDCardDisplay : MonoBehaviour
{
    public GameObject displayPrefab;   // prefab ที่จะโชว์ตอนคลิก (ใส่ต่างกันในแต่ละ ID Card prefab)

    public void ShowCard()
    {
        IDCardPopup.Instance.Show(displayPrefab);
    }
}