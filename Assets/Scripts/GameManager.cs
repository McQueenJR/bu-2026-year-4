using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private bool isProcessing = false;
    public GameObject currentNPC;

    public NPCSpawner spawner;

    public Transform enterPoint;
    public Transform exitPoint;

    [Header("Day System")]
    public int currentDay = 1;
    public int currentHour = 12;

    [Header("Emergency")]
    public bool emergencyMode = false;
    
    public int npcPerDay = 8;

   
    private int currentNPCCount = 0;

    public ClockManager clockManager;
    public NPCState currentState;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("Game Start");

        currentNPCCount = 0;
        currentHour = 12;

        spawner.SpawnNPC();
    }

    public void AllowNPC()
    {
        if (currentNPC == null) return;
        if (isProcessing) return;

        isProcessing = true;

        currentNPC.GetComponent<NPCMovement>()
            .MoveTo(enterPoint.position);

        StartCoroutine(RemoveNPC());
    }

    public void RejectNPC()
    {
        if (currentNPC == null) return;
        if (isProcessing) return;

        isProcessing = true;

        currentNPC.GetComponent<NPCMovement>()
            .MoveTo(exitPoint.position);

        StartCoroutine(RemoveNPC());
    }

    System.Collections.IEnumerator RemoveNPC()
    {
        NPCMovement move = currentNPC.GetComponent<NPCMovement>();

        while (move.IsMoving())
            yield return null;

        Destroy(currentNPC);
        currentNPC = null;

        currentNPCCount++;

        clockManager.NextHour();

        yield return new WaitForSeconds(1f);

        
        isProcessing = false;

        if (currentNPCCount < npcPerDay)
        {
            spawner.SpawnNPC();
        }
        else
        {
            EndDay();
        }
    }

    public void EndDay()
    {
        Debug.Log("Day " + currentDay + " จบ");

        // ตอนนี้ยังไม่ทำ Day2
        // ไว้ทำทีหลัง
    }
    public enum NPCState
    {
        WalkingToCheckpoint, 
        WaitingDecision,     
        Inspecting,          
        Leaving              
    }
    public void GreenButton()
    {
        if (currentState != NPCState.WaitingDecision)
            return;

        currentState = NPCState.Leaving;

        currentNPC.GetComponent<NPCMovement>()
            .MoveTo(enterPoint.position);

        StartCoroutine(RemoveNPC());
    }
    public void RedButton()
    {
        if (currentState != NPCState.WaitingDecision)
            return;

        currentState = NPCState.Inspecting;

        Debug.Log("เปิดกระจก");
        Debug.Log("ส่งกระเป๋า");
        Debug.Log("เริ่มคุย");
    }
}