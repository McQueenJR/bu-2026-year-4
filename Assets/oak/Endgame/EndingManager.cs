using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// ศูนย์กลางฉากจบ GoodEnd / BadEnd
public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance;

    [Header("Good End")]
    [Tooltip("วันสุดท้าย — จบวันนี้แล้วตัดสินฉากจบ (ตั้ง 7)")]
    public int checkDay = 7;
    [Tooltip("ต้องมีชาวบ้านรอดอย่างน้อยกี่คน ถึงจะ Good End")]
    public int minVillagersAlive = 7;
    public string goodEndSceneName = "GoodEnd";
    [Tooltip("สำรองสำหรับฉากจบอนาคต — ตอนนี้ Bad End มาก่อนเสมอ ไม่ได้ใช้")]
    public string fallbackSceneName = "";

    [Header("Bad End Conditions (สำรองสำหรับฉากจบอนาคต ไม่บล็อก Bad End แล้ว)")]
    public int aliveVillagerThreshold = 0;
    public int ghostPlusVanishLimit = 999;
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

    // ตัดสินฉากจบแล้ว (กันยิงซ้อน)
    private bool endingLocked;
    // สั่งโหลดซีนจบแล้ว
    private bool sceneLoading;

    void Awake()
    {
        Instance = this;
    }

    // =====================================================
    // ★ จบวันสุดท้าย — เรียกจาก GameManager.EndGame()
    // คืน true = ฉากจบถูกจัดการแล้ว (GameManager ห้ามขึ้น EndDayUI)
    // =====================================================
    public bool TryResolveFinalDay()
    {
        if (endingLocked)
            return true;

        if (GameManager.Instance == null ||
            GameManager.Instance.currentDay < checkDay)
            return false;

        int day = GameManager.Instance.currentDay;

        // Resolve ผีและหายสาบสูญก่อน แล้วค่อยตัดสิน
        if (CampManager.Instance != null)
            CampManager.Instance.ResolveGhosts();

        if (DeathTracker.Instance != null)
            DeathTracker.Instance.RollPendingVanish(day);

        DecideEnding();
        return true;
    }

    // =====================================================
    // ★ ตัดสินฉากจบ — เมธอดเดียว (เติมฉากใหม่ภายหลังที่นี่)
    // =====================================================
    private void DecideEnding()
    {
        int alive = CampManager.Instance != null
            ? CampManager.Instance.GetAliveVillagerCount()
            : 0;

        Debug.Log($"[Ending] ตัดสินฉากจบ | ชาวบ้านรอด {alive} (Good ≥ {minVillagersAlive})");

        // ช่องว่าง (รอด 1–6 คน) ตกไป Bad End
        if (alive >= minVillagersAlive)
        {
            endingLocked = true;
            Debug.Log("===== GOOD END =====");
            LoadEndingScene(EndingType.Good, goodEndSceneName);
        }
        else
        {
            StartBadEnd();
        }
    }

    // =====================================================
    // ★ BAD END ก่อนวันสุดท้าย — ชาวบ้านรอด ≤ 0
    // เรียกหลังปล่อย NPC / หลัง PoliceSequence / หลัง RollPendingVanish
    // =====================================================
    public bool CheckBadEnd()
    {
        if (endingLocked)
            return true;

        if (CampManager.Instance == null)
            return false;

        int alive = CampManager.Instance.GetAliveVillagerCount();

        if (alive > 0)
            return false;

        Debug.Log("[Ending] ชาวบ้านตายหมด → BAD END");
        StartBadEnd();
        return true;
    }

    private void StartBadEnd()
    {
        if (endingLocked && IsBadEndTriggered)
            return;

        endingLocked = true;
        StartCoroutine(BadEndSequence());
    }

    // =====================================================
    // BAD END SEQUENCE
    // =====================================================
    private IEnumerator BadEndSequence()
    {
        IsBadEndTriggered = true;

        if (GreenRedButtonManager.Instance != null)
            GreenRedButtonManager.Instance.HideDecisionButtons();

        // กัน NPCMovement เรียก NPCReachedCheckpoint
        if (GameManager.Instance != null)
            GameManager.Instance.currentState = GameManager.NPCState.WaitingDecision;

        if (badEndNPCPrefab == null)
        {
            Debug.LogError("EndingManager ไม่ได้ใส่ BadEnd NPC Prefab → ข้ามไปซีนเลย");
            LoadEndingScene(EndingType.Bad, badEndSceneName);
            yield break;
        }

        Transform spawnPos = badEndSpawnPoint != null
            ? badEndSpawnPoint : spawner.spawnPoint;

        Transform stopPos = badEndStopPoint != null
            ? badEndStopPoint : spawner.stopPoint;

        GameObject npcObj = Instantiate(
            badEndNPCPrefab, spawnPos.position, Quaternion.identity);

        Debug.Log("===== BAD END — NPC เดินเข้ามา =====");

        NPCMovement move = npcObj.GetComponent<NPCMovement>();

        if (move != null && stopPos != null)
        {
            move.MoveTo(stopPos.position);

            while (move.IsMoving())
                yield return null;
        }

        NPCMouthAnimation mouth =
            npcObj.GetComponentInChildren<NPCMouthAnimation>();

        if (mouth != null && dialogManager != null)
            dialogManager.SetTalkingNPC(mouth);

        if (badEndDialogue == null)
        {
            Debug.LogWarning("ไม่มี BadEndDialogueData → ข้าม dialogue ไปซีนเลย");
            LoadEndingScene(EndingType.Bad, badEndSceneName);
            yield break;
        }

        dialogManager.StartEndingDialog(badEndDialogue, spacePauseDuration);
        // DialogManager จะเรียก OnBadEndDialogueFinished() เมื่อพูดครบ
    }

    // เรียกจาก DialogManager เมื่อ dialogue ครบทุกบรรทัด
    public void OnBadEndDialogueFinished()
    {
        Debug.Log("===== BAD END — โหลดซีน =====");
        LoadEndingScene(EndingType.Bad, badEndSceneName);
    }

    // =====================================================
    // SNAPSHOT + LOAD SCENE
    // =====================================================
    private void SaveSnapshot(EndingType type)
    {
        EndingSummaryData.Clear();
        EndingSummaryData.HasData = true;
        EndingSummaryData.type = type;

        int day = GameManager.Instance != null
            ? GameManager.Instance.currentDay : 0;

        EndingSummaryData.endDay = day;

        CampaignStatsTracker s = CampaignStatsTracker.Instance;

        if (s != null)
        {
            // ปิดบัญชีวันสุดท้าย (ถ้ายังไม่ปิด)
            // Bad End กลางวัน = ใช้เท่าที่ตัดสินเสร็จจนถึงตอนนั้น
            s.FinalizeDay(day);

            EndingSummaryData.ghostsEliminated = s.GhostsEliminated;
            EndingSummaryData.ghostsEntered = s.GhostsEntered;
            EndingSummaryData.ghostsRejected = s.GhostsRejected;
            EndingSummaryData.villagersWronglyJudged = s.VillagersWronglyJudged;
            EndingSummaryData.villagersRejected = s.VillagersRejected;

            EndingSummaryData.scoreEarned = s.CampaignEarned;
            EndingSummaryData.scoreMax = s.CampaignMax;
            EndingSummaryData.rankText =
                ScoreRankUtility.GetRank(s.CampaignEarned, s.CampaignMax);
        }
        else
        {
            Debug.LogWarning("[Ending] ไม่มี CampaignStatsTracker ในซีน → สถิติเป็น 0 แรงค์เป็น \"-\"");
        }

        if (DeathTracker.Instance != null)
        {
            EndingSummaryData.villagersKilledByGhost =
                DeathTracker.Instance.GetCount(DeathCause.GhostTentKill);
        }
    }

    private void LoadEndingScene(EndingType type, string sceneName)
    {
        if (sceneLoading)
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("EndingManager: ชื่อซีนว่าง!");
            return;
        }

        sceneLoading = true;
        SaveSnapshot(type);
        SceneManager.LoadScene(sceneName);
    }
}