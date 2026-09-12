using UnityEngine;
using System;

/// <summary>
/// ใช้แทน UnityEngine.UI.Toggle สำหรับวัตถุที่อยู่ใน World Space (ไม่ใช่ Canvas)
/// ต้องมี Collider2D (แนะนำ BoxCollider2D) ติดอยู่กับ GameObject นี้เพื่อให้ OnMouseDown ทำงาน
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WorldToggle : MonoBehaviour
{
    [Header("Visual")]
    public SpriteRenderer icon;      // สไปรต์ที่จะเปลี่ยนตอนติ๊ก/ไม่ติ๊ก
    public Sprite onSprite;          // สไปรต์ตอนติ๊กแล้ว (Abnormal / Normal ที่ถูกเลือก)
    public Sprite offSprite;         // สไปรต์ตอนยังไม่ติ๊ก

    [Header("State")]
    [SerializeField] private bool isOn = false;
    public bool IsOn => isOn;

    [Header("Options")]
    public bool interactable = true;

    // เทียบเท่า onValueChanged ของ Toggle เดิม
    // สมัครฟังแบบ: myToggle.onValueChanged += value => { ... };
    public event Action<bool> onValueChanged;

    private void Reset()
    {
        icon = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ApplyVisual();
    }

    private void OnMouseDown()
    {
        if (!interactable) return;

        // กันคลิกทะลุกรณีเมาส์อยู่บน UI Canvas อื่น (เช่น โทรศัพท์ในเกมเปิดอยู่)
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        SetIsOn(!isOn);
    }

    // เทียบเท่า toggle.isOn = value; (จะยิง event ถ้าค่าค่าเปลี่ยน)
    public void SetIsOn(bool value)
    {
        if (isOn == value) return;

        isOn = value;
        ApplyVisual();
        onValueChanged?.Invoke(isOn);
    }

    // เทียบเท่า SetIsOnWithoutNotify ของ Toggle เดิม (ใช้ตอน Reset ไม่อยากยิง event)
    public void SetIsOnWithoutNotify(bool value)
    {
        isOn = value;
        ApplyVisual();
    }

    private void ApplyVisual()
    {
        if (icon == null) return;
        icon.sprite = isOn ? onSprite : offSprite;
    }
}