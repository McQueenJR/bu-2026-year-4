using UnityEngine;

public class GreenRedButtonManager : MonoBehaviour
{
    public static GreenRedButtonManager Instance;
    

    [Header("Button Visuals")]
    public ButtonVisual greenButtonVisual;
    public ButtonVisual redButtonVisual;

    [Header("Button Sounds")]
    public AudioSource buttonSound;
    public AudioClip greenButtonSound;
    public AudioClip redButtonSound;

    [Header("Decision Buttons Panel")]
    public GameObject decisionButtonsPanel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        greenButtonVisual.SetActive(false);
        redButtonVisual.SetActive(false);
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

        greenButtonVisual.SetActive(true);
        redButtonVisual.SetActive(false);

        if (buttonSound != null && greenButtonSound != null)
            buttonSound.PlayOneShot(greenButtonSound);

        HideDecisionButtons();

        GameManager.Instance.ReleaseCurrentNPC();
    }

    public void RedButton()
    {
        if (GameManager.Instance.currentNPC == null)
            return;

        if (GameManager.Instance.currentState == GameManager.NPCState.Leaving)
            return;

        greenButtonVisual.SetActive(false);
        redButtonVisual.SetActive(true);

        if (buttonSound != null && redButtonSound != null)
            buttonSound.PlayOneShot(redButtonSound);

        HideDecisionButtons();

        GameManager.Instance.RejectCurrentNPC();
    }
    

    // =========================
    // SHOW / HIDE
    // =========================

    public void ShowDecisionButtons()
    {
        if (decisionButtonsPanel != null)
            decisionButtonsPanel.SetActive(true);
    }

    public void HideDecisionButtons()
    {
        if (decisionButtonsPanel != null)
            decisionButtonsPanel.SetActive(false);
    }
}