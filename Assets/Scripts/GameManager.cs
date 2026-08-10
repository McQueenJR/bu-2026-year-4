using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject currentNPC;
    public NPCSpawner spawner;

    public Transform enterPoint;
    public Transform exitPoint;

    [Header("Day System")]
    public int currentHour;
    public int startHour = 20;
    public int endHour = 6;

    public ClockManager clockManager;

    [Header("Emergency")]
    public bool emergencyMode = false;

    public enum NPCState
    {
        WalkingToCheckpoint,
        WaitingDecision,
        Inspecting,
        Leaving
    }

    public NPCState currentState;

    public enum ButtonChoice
    {
        Green,
        Red
    }

    public ButtonChoice currentButtonChoice = ButtonChoice.Red;

    [Header("Button Visuals")]
    public ButtonVisual greenButtonVisual;
    public ButtonVisual redButtonVisual;

    [Header("Dialog")]
    public DialogManager dialogManager;

    [Header("Bag")]
    public GameObject bagPrefab;
    public Transform spawnPointBag;
    public SlidingPanel windowPanel;

    private GameObject currentBag;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentHour = startHour;

        clockManager.SetHour(currentHour);

        SetButtonChoice(ButtonChoice.Red);

        spawner.SpawnNPC();
    }

    // =========================
    // BUTTON
    // =========================

    public void GreenButton()
    {
        SetButtonChoice(ButtonChoice.Green);

        // ถ้ากำลังตรวจอยู่
        if (currentState == NPCState.Inspecting && currentNPC != null)
        {
            ReleaseCurrentNPC();
        }
    }

    public void RedButton()
    {
        SetButtonChoice(ButtonChoice.Red);
    }

    private void SetButtonChoice(ButtonChoice choice)
    {
        currentButtonChoice = choice;

        greenButtonVisual.SetActive(
            choice == ButtonChoice.Green);

        redButtonVisual.SetActive(
            choice == ButtonChoice.Red);
    }

    // =========================
    // NPC CHECKPOINT
    // =========================

    public void NPCReachedCheckpoint(GameObject npc)
    {
        currentNPC = npc;

        currentState = NPCState.WaitingDecision;

        StartCoroutine(CheckDecisionAfterDelay());
    }

    private IEnumerator CheckDecisionAfterDelay()
    {
        // NPC หยุดรอ 2 วินาที
        yield return new WaitForSeconds(2f);

        if (currentNPC == null)
            yield break;

        // =========================
        // GREEN
        // =========================

        if (currentButtonChoice == ButtonChoice.Green)
        {
            ReleaseCurrentNPC();
        }

        // =========================
        // RED
        // =========================

        else
        {
            currentState = NPCState.Inspecting;

            Debug.Log("เริ่มตรวจสอบ NPC");

            StartNPCDialog();
        }
    }

    // =========================
    // DIALOG
    // =========================

    private void StartNPCDialog()
    {
        if (currentNPC == null)
            return;

        NPC npc = currentNPC.GetComponent<NPC>();

        if (npc == null)
        {
            Debug.LogError("NPC ไม่มี NPC.cs");
            return;
        }

        if (npc.data == null)
        {
            Debug.LogError("NPC ไม่มี NPCData");
            return;
        }

        dialogManager.StartDialog(npc.data);
    }

    public void DialogFinished()
    {
        if (currentNPC == null)
            return;

        if (currentState != NPCState.Inspecting)
            return;

        Debug.Log("Dialog จบ");

        windowPanel.SlideOut(() =>
        {
            SpawnBag();
        });
    }

    // =========================
    // BAG
    // =========================

    private void SpawnBag()
    {
        if (bagPrefab == null)
        {
            Debug.LogError("ไม่ได้ใส่ Bag Prefab");
            return;
        }

        if (spawnPointBag == null)
        {
            Debug.LogError("ไม่ได้ใส่ Spawn Point Bag");
            return;
        }

        currentBag = Instantiate(
            bagPrefab,
            spawnPointBag.position,
            Quaternion.identity
        );
    }

    private void DestroyBagAndSlideBack()
    {
        if (currentBag != null)
        {
            Destroy(currentBag);
            currentBag = null;
        }

        if (windowPanel != null)
        {
            windowPanel.SlideBack();
        }
    }

    // =========================
    // RELEASE NPC
    // =========================

    private void ReleaseCurrentNPC()
    {
        if (currentNPC == null)
            return;

        currentState = NPCState.Leaving;

        DestroyBagAndSlideBack();

        NPCMovement move =
            currentNPC.GetComponent<NPCMovement>();

        move.MoveTo(enterPoint.position);

        StartCoroutine(
            WaitForExitThenAdvanceHour(currentNPC));
    }

    // =========================
    // REJECT NPC
    // =========================

    private void RejectCurrentNPC()
    {
        if (currentNPC == null)
            return;

        currentState = NPCState.Leaving;

        DestroyBagAndSlideBack();

        NPCMovement move =
            currentNPC.GetComponent<NPCMovement>();

        move.MoveTo(exitPoint.position);

        StartCoroutine(
            WaitForExitThenAdvanceHour(currentNPC));
    }

    // =========================
    // NPC DESTROY
    // =========================

    private IEnumerator WaitForExitThenAdvanceHour(GameObject npc)
    {
        NPCMovement move =
            npc.GetComponent<NPCMovement>();

        while (move.IsMoving())
            yield return null;

        Destroy(npc);

        currentNPC = null;

        // เวลา +1 ชั่วโมง
        AdvanceHour();
    }

    // =========================
    // TIME
    // =========================

    private void AdvanceHour()
    {
        currentHour++;

        if (currentHour >= 24)
            currentHour = 0;

        clockManager.SetHour(currentHour);

        Debug.Log(
            "เวลา : " +
            currentHour.ToString("00") +
            ":00");

        if (currentHour == endHour)
        {
            EndGame();
            return;
        }

        // Spawn NPC คนใหม่
        spawner.SpawnNPC();
    }

    // =========================
    // END
    // =========================

    public void EndGame()
    {
        Debug.Log(
            "จบเกม เวลา " +
            currentHour.ToString("00") +
            ":00");
    }
}