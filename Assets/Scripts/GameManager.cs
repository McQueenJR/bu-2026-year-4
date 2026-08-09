using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject currentNPC;
    public NPCSpawner spawner;

    public Transform enterPoint;   // จุดเดินไปตอนกดเขียว (ผ่านการตรวจ)
    public Transform exitPoint;    // จุดเดินไปตอนตรวจไม่ผ่าน (ยังไม่มีเงื่อนไขเรียกใช้)

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

    public enum ButtonChoice { Green, Red }
    public ButtonChoice currentButtonChoice = ButtonChoice.Red;

    [Header("Button Visuals")]
    public ButtonVisual greenButtonVisual;
    public ButtonVisual redButtonVisual;

    [Header("Bag Spawn")]
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

    public void GreenButton()
    {
        SetButtonChoice(ButtonChoice.Green);

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

        greenButtonVisual.SetActive(choice == ButtonChoice.Green);
        redButtonVisual.SetActive(choice == ButtonChoice.Red);
    }

    // NPCMovement เรียกตอนเดินถึงจุดกลางจอ (stopPoint)
    public void NPCReachedCheckpoint(GameObject npc)
    {
        currentNPC = npc;
        currentState = NPCState.WaitingDecision;
        StartCoroutine(CheckDecisionAfterDelay());
    }

    private IEnumerator CheckDecisionAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        if (currentButtonChoice == ButtonChoice.Green)
        {
            ReleaseCurrentNPC();
        }
        else
        {
            currentState = NPCState.Inspecting;
            Debug.Log("NPC หยุดรอการตรวจสอบ (ปุ่มแดง active) — รอผู้เล่นกดเขียวเพื่อปล่อย");

            windowPanel.SlideOut(() => SpawnBag());
        }
    }

    private void SpawnBag()
    {
        if (bagPrefab == null || spawnPointBag == null) return;

        currentBag = Instantiate(bagPrefab, spawnPointBag.position, Quaternion.identity);
    }

    private void DestroyBagAndSlideBack()
    {
        if (currentBag != null)
        {
            Destroy(currentBag);
            currentBag = null;

            windowPanel.SlideBack();
        }
    }

    // ผ่านการตรวจ (กดเขียว) → เดินไป enterPoint
    private void ReleaseCurrentNPC()
    {
        if (currentNPC == null) return;

        currentState = NPCState.Leaving;

        DestroyBagAndSlideBack();

        currentNPC.GetComponent<NPCMovement>().MoveTo(enterPoint.position);
        StartCoroutine(WaitForExitThenAdvanceHour(currentNPC));
    }

    // TODO: เรียกจุดนี้เมื่อมีเงื่อนไข "ตรวจไม่ผ่าน" — ยังไม่ได้ผูกจากที่ไหน
    private void RejectCurrentNPC()
    {
        if (currentNPC == null) return;

        currentState = NPCState.Leaving;

        DestroyBagAndSlideBack();

        currentNPC.GetComponent<NPCMovement>().MoveTo(exitPoint.position);
        StartCoroutine(WaitForExitThenAdvanceHour(currentNPC));
    }

    private IEnumerator WaitForExitThenAdvanceHour(GameObject npc)
    {
        NPCMovement move = npc.GetComponent<NPCMovement>();

        while (move.IsMoving())
            yield return null;

        Destroy(npc);
        currentNPC = null;

        AdvanceHour();
    }

    private void AdvanceHour()
    {
        currentHour = (currentHour + 1) % 24;
        clockManager.SetHour(currentHour);

        if (currentHour == endHour)
        {
            EndGame();
            return;
        }

        spawner.SpawnNPC();
    }

    public void EndGame()
    {
        Debug.Log("จบเกม (ถึงเวลา " + endHour.ToString("00") + ":00)");
        // TODO: ใส่ logic จบเกมจริง
    }
}