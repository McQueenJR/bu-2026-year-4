using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EndingNewspaper : MonoBehaviour
{
    [Header("Display (Popup เต็มจอ)")]
    public GameObject displayPanel;     // Panel ใน Canvas (ปิดไว้ตอนเริ่ม)
    public Image displayImage;          // รูปหนังสือพิมพ์ใน panel
    public Sprite goodNewspaperSprite;
    public Sprite badNewspaperSprite;
    public Button skipButton;           // ปุ่มข้าม มุมขวาล่าง

    [Header("หน้าสรุป")]
    public EndingSummaryUI summaryUI;

    [Header("การคลิก")]
    [Tooltip("ติ๊กถ้าไม่ได้ใช้ WorldButton (ถ้าใช้ WorldButton ให้ต่อ OnClick → Open())")]
    public bool handleClickItself = false;

    private bool opened;

    void Start()
    {
        if (displayPanel != null)
            displayPanel.SetActive(false);

        if (skipButton != null)
            skipButton.onClick.AddListener(Skip);
    }

    void OnMouseUpAsButton()
    {
        if (!handleClickItself)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        Open();
    }

    // ต่อจาก WorldButton.OnClick
    public void Open()
    {
        if (opened)
            return;

        opened = true;

        // กันคลิกซ้ำ
        Collider2D col2D = GetComponent<Collider2D>();
        if (col2D != null) col2D.enabled = false;
        Collider col3D = GetComponent<Collider>();
        if (col3D != null) col3D.enabled = false;

        if (displayImage != null)
        {
            Sprite s = EndingSummaryData.type == EndingType.Bad
                ? badNewspaperSprite
                : goodNewspaperSprite;

            if (s != null)
                displayImage.sprite = s;
        }

        if (displayPanel != null)
            displayPanel.SetActive(true);
        else
            Debug.LogWarning("EndingNewspaper: ไม่ได้ใส่ Display Panel");
    }

    private void Skip()
    {
        if (displayPanel != null)
            displayPanel.SetActive(false);

        if (summaryUI != null)
            summaryUI.Show();
        else
            Debug.LogError("EndingNewspaper: ไม่ได้ใส่ Summary UI");
    }
}