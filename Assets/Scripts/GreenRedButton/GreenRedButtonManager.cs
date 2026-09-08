using UnityEngine;

public class GreenRedButtonManager : MonoBehaviour
{
    public static GreenRedButtonManager Instance;

    [Header("Button Sounds")]
    public AudioSource greenButtonSound;
    public AudioSource redButtonSound;
    public AudioSource showButtonsSound;

    [Header("Decision Buttons Panel")]
    public GameObject decisionButtonsPanel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        HideDecisionButtons();
    }

    // =========================
    // BUTTON
    // =========================

    public void GreenButton()
    {
        if (GameManager.Instance.emergencyMode)
        {
            Debug.Log("อยู่ในโหมดฉุกเฉิน กดปุ่มเขียวไม่ได้");
            return;
        }

        if (GameManager.Instance.currentNPC == null)
            return;

        if (GameManager.Instance.currentState == GameManager.NPCState.Leaving)
            return;

        NPC npc = GameManager.Instance.currentNPC.GetComponent<NPC>();
        if (npc == null || npc.data == null)
        {
            Debug.LogError("NPC ไม่มีข้อมูลสำหรับ Green Dialog");
            return;
        }
        

        if (greenButtonSound != null)
            greenButtonSound.Play();

        HideDecisionButtons();
        GameManager.Instance.SetCurrentNPCMouthTalking();
        GameManager.Instance.dialogManager.StartGreenDialog(npc.data);
        
    }

    public void RedButton()
    {
        if (GameManager.Instance.currentNPC == null)
            return;

        if (GameManager.Instance.currentState == GameManager.NPCState.Leaving)
            return;
        
        NPC npc = GameManager.Instance.currentNPC.GetComponent<NPC>();
        if (npc == null || npc.data == null)
        {
            Debug.LogError("NPC ไม่มีข้อมูลสำหรับ Red Dialog");
            return;
        }
        

        if (redButtonSound != null)
            redButtonSound.Play();

        HideDecisionButtons();
        GameManager.Instance.SetCurrentNPCMouthTalking(); 
        GameManager.Instance.dialogManager.StartRedDialog(npc.data);
    }
    

    // =========================
    // SHOW / HIDE
    // =========================

    public void ShowDecisionButtons()
    {
        if (decisionButtonsPanel != null)
            decisionButtonsPanel.SetActive(true);
        
        if (showButtonsSound != null)
            showButtonsSound.Play();
    }

    public void HideDecisionButtons()
    {
        if (decisionButtonsPanel != null)
            decisionButtonsPanel.SetActive(false);
    }
}