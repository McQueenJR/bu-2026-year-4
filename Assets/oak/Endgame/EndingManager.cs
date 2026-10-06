using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// ศูนย์กลางฉากจบ GoodEnd / BadEnd
// ทุกเงื่อนไขปรับได้ใน Inspector
public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance;

    [Header("Good End")]
    [Tooltip("เช็คเงื่อนไขหลัง NPC ตัวสุดท้ายของวันที่กำหนด")]
    public int checkDay = 7;
    [Tooltip("ต้องมีชาวบ้านรอดอย่างน้อยกี่คน")]
    public int minVillagersAlive = 7;
    public string goodEndSceneName = "GoodEnd";
    [Tooltip("ถ้า GoodEnd ไม่ผ่านจะไปซีนนี้ — เว้นว่าง = ขึ้น EndDayUI ตามปกติ")]
    public string fallbackSceneName = "";

    [Header("Bad End Conditions")]
    [Tooltip("ชาวบ้านที่ยังมีชีวิต น้อยกว่าหรือเท่ากับค่านี้")]
    public int aliveVillagerThreshold = 0;
    [Tooltip("ตายจากผีฆ่าในเต็นท์ + หายสาบสูญจากปุ่มแดง ไม่เกินกี่คน")]
    public int ghostPlusVanishLimit = 999;
    [Tooltip("ตายจากการกด emergency ไม่เกินกี่คน")]
    public int emergencyKillLimit = 6;

    [Header("Bad End Sequence")]
    public GameObject badEndNPCPrefab;
    public BadEndDialogueData badEndDialogue;
    [Tooltip("ว่าง = ใช้ SpawnManager.spawnPoint")]
    public Transform badEndSpawnPoint;
    [Tooltip("ว่าง = ใช้ SpawnManager.stopPoint")]
    public Transform badEndStopPoint;
    [Tooltip("พักกี่วิ เมื่อเจอช่องว่างในบทพูด")]
    public float spacePauseDuration = 0.5f;
    public string badEndSceneName = "BadEnd";

    [Header("References")]
    public SpawnManager spawner;
    public DialogManager dialogManager;

    // =========================
    // STATE
    // =========================
    public bool IsBadEndTriggered { get; private set; }
    private bool goodEndTriggered;

    void Awake()
    {
        Instance = this;
    }

    // =====================================================
    // ★ GOOD END — เรียกจาก GameManager.EndGame()
    // =====================================================
    public bool CheckGoodEnd()
    {
        // กันเรียกซ้ำ (EndGame อาจถูกเรียกได้มากกว่า 1 ทาง)
        if (goodEndTriggered)
            return true;

        if (GameManager.Instance == null)
            return false;

        // ยังไม่ถึงวันที่ตั้งไว้ → ไม่เข้าเงื่อนไขนี้
        if (GameManager.Instance.currentDay != checkDay)
            return false;

        int alive = CampManager.Instance != null
            ? CampManager.Instance.GetAliveVillagerCount()
            : 0;

        Debug.Log(
            $"[Ending] เช็ค GoodEnd | วัน {checkDay} | " +
            $"ชาวบ้านรอด {alive}/{minVillagersAlive}"
        );

        // ---------- ผ่าน → GoodEnd ----------
        if (alive >= minVillagersAlive)
        {
            goodEndTriggered = true;
            Debug.Log("===== GOOD END =====");
            LoadScene(goodEndSceneName);
            return true;
        }

        // ---------- ไม่ผ่าน → fallback ----------
        if (!string.IsNullOrEmpty(fallbackSceneName))
        {
            goodEndTriggered = true;
            Debug.Log("===== GOOD END ไม่ผ่าน → Fallback =====");
            LoadScene(fallbackSceneName);
            return true;
        }

        // ไม่มี fallback → ให้เกมทำงานต่อปกติ (EndDayUI)
        return false;
    }

    // =====================================================
    // ★ BAD END — เรียกหลังปล่อย NPC เดินออกจากจุดตรวจ
    // =====================================================
    public bool CheckBadEnd()
    {
        if (IsBadEndTriggered)
            return true;

        if (DeathTracker.Instance == null ||
            CampManager.Instance == null)
            return false;

        int alive = CampManager.Instance.GetAliveVillagerCount();
        int ghostPlusVanish = DeathTracker.Instance.GetGhostPlusVanishCount();
        int emergency = DeathTracker.Instance.GetCount(DeathCause.EmergencyKill);

        bool pass =
            alive <= aliveVillagerThreshold &&
            ghostPlusVanish <= ghostPlusVanishLimit &&
            emergency <= emergencyKillLimit;

        Debug.Log(
            $"[Ending] เช็ค BadEnd | รอด {alive}≤{aliveVillagerThreshold} | " +
            $"ผี+หาย {ghostPlusVanish}≤{ghostPlusVanishLimit} | " +
            $"emergency {emergency}≤{emergencyKillLimit} → {pass}"
        );

        if (pass)
        {
            StartCoroutine(BadEndSequence());
        }

        return pass;
    }

    // =====================================================
    // BAD END SEQUENCE
    // หยุด spawn → spawn NPC → เดินเข้ามา → พูด → วาปซีน
    // =====================================================
    private IEnumerator BadEndSequence()
    {
        IsBadEndTriggered = true;

        // ซ่อนปุ่มตัดสินใจ
        if (GreenRedButtonManager.Instance != null)
            GreenRedButtonManager.Instance.HideDecisionButtons();

        // ★ สำคัญ: ตั้ง state ให้ไม่ใช่ WalkingToCheckpoint
        // เพื่อกัน NPCMovement เรียก NPCReachedCheckpoint → เปิด dialog ปกติ
        if (GameManager.Instance != null)
            GameManager.Instance.currentState = GameManager.NPCState.WaitingDecision;

        // ---------- กรณีไม่มี prefab / dialogue → ข้ามไปซีนเลย ----------
        if (badEndNPCPrefab == null)
        {
            Debug.LogError("EndingManager ไม่ได้ใส่ BadEnd NPC Prefab → ข้ามไปซีนเลย");
            LoadScene(badEndSceneName);
            yield break;
        }

        // ---------- 1. Spawn NPC ----------
        Transform spawnPos = badEndSpawnPoint != null
            ? badEndSpawnPoint
            : spawner.spawnPoint;

        Transform stopPos = badEndStopPoint != null
            ? badEndStopPoint
            : spawner.stopPoint;

        GameObject npcObj = Instantiate(
            badEndNPCPrefab,
            spawnPos.position,
            Quaternion.identity
        );

        Debug.Log("===== BAD END — NPC เดินเข้ามา =====");

        // ---------- 2. เดินเข้ามา ----------
        NPCMovement move = npcObj.GetComponent<NPCMovement>();

        if (move != null && stopPos != null)
        {
            move.MoveTo(stopPos.position);

            while (move.IsMoving())
                yield return null;
        }

        // ---------- 3. ตั้งปากพูด ----------
        NPCMouthAnimation mouth =
            npcObj.GetComponentInChildren<NPCMouthAnimation>();

        if (mouth != null && dialogManager != null)
            dialogManager.SetTalkingNPC(mouth);

        // ---------- 4. พูด dialogue ----------
        if (badEndDialogue == null)
        {
            Debug.LogWarning("ไม่มี BadEndDialogueData → ข้าม dialogue ไปซีนเลย");
            LoadScene(badEndSceneName);
            yield break;
        }

        dialogManager.StartEndingDialog(badEndDialogue, spacePauseDuration);

        // รอพูดจนครบ → DialogManager จะเรียก OnBadEndDialogueFinished() เอง
    }

    // เรียกจาก DialogManager เมื่อ dialogue ครบทุกบรรทัดแล้ว
    public void OnBadEndDialogueFinished()
    {
        Debug.Log("===== BAD END — โหลดซีน =====");
        LoadScene(badEndSceneName);
    }

    // =====================================================
    // LOAD SCENE
    // =====================================================
    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("EndingManager: ชื่อซีนว่าง!");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}