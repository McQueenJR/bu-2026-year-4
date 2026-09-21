using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpawnManager : MonoBehaviour
{
    [Header("NPC Database")]
    public AllDataPrefabNPC allData;

    [Header("Spawn Point")]
    public Transform spawnPoint;
    public Transform stopPoint;

    [Header("References")]
    public GameManager gameManager;
    public TodayListManager todayListManager;

    [Header("Special Rule")]
    [Tooltip("ทุกกี่คนต้องมี Special อย่างน้อย 1 คน")]
    public int specialWindow = 4;

    [Tooltip("เปิด/ปิดกฎบังคับ Special")]
    public bool forceSpecial = true;

    // NPC ทั้งหมดของวันนี้
    public List<TodayApplicant> todayApplicants = new();

    private int currentApplicantIndex = 0;

    // =========================================================
    // เริ่มวันใหม่
    // =========================================================
    public void GenerateTodayApplicants()
{
    todayApplicants.Clear();
    currentApplicantIndex = 0;

    // =====================================================
    // จำนวน NPC ของวันนี้
    // =====================================================
    int npcToday = Random.Range(
        allData.minNPCPerDay,
        allData.maxNPCPerDay + 1
    );

    // ส่งจำนวน NPC ให้ GameManager ใช้เช็ก End Day
    gameManager.npcPerDay = npcToday;

    Debug.Log("NPC วันนี้ทั้งหมด = " + npcToday);

    // =====================================================
    // จำนวนคนใน Today List
    // =====================================================
    int todayListCount = Random.Range(
        allData.minTodayList,
        Mathf.Min(allData.maxTodayList + 1, npcToday + 1)
    );

    HashSet<DataPrefabNPC> usedNPC = new HashSet<DataPrefabNPC>();
    int safety = 300;

    // =====================================================
    // สุ่ม NPC ทั้งวัน
    // =====================================================
    while (todayApplicants.Count < npcToday && safety-- > 0)
    {
        RoleGroup role = NeedSpecial(todayApplicants.Count)
            ? allData.special
            : ChooseRole();

        DataPrefabNPC npc = ChooseNPC(role);

        if (npc == null || usedNPC.Contains(npc))
            continue;

        usedNPC.Add(npc);

        TodayApplicant applicant = CreateApplicant(npc);

        if (applicant == null)
            continue;

        todayApplicants.Add(applicant);
    }

    if (safety <= 0)
    {
        Debug.LogWarning("NPC ไม่พอสำหรับวันนี้");
    }

    // =====================================================
    // สุ่มว่าใครอยู่ Today List
    // =====================================================
    List<int> indexes = Enumerable.Range(0, todayApplicants.Count).ToList();

    for (int i = 0; i < indexes.Count; i++)
    {
        int random = Random.Range(i, indexes.Count);
        (indexes[i], indexes[random]) = (indexes[random], indexes[i]);
    }

    for (int i = 0; i < todayListCount && i < indexes.Count; i++)
    {
        todayApplicants[indexes[i]].isInTodayList = true;
    }

    // =====================================================
    // เตรียม Prefab ของแต่ละ NPC
    // (ตอนนี้รู้แล้วว่าใครอยู่ Today List)
    // =====================================================
    foreach (TodayApplicant applicant in todayApplicants)
    {
        PrepareApplicantPrefab(applicant);
    }

    // =====================================================
    // แจก Camp ให้ NPC ทุกคน
    // =====================================================
    if (CampManager.Instance != null)
    {
        CampManager.Instance.AssignCamp(todayApplicants);
    }

    // =====================================================
    // ส่งข้อมูลเข้า Today List
    // =====================================================
    List<NPCData> todayListData = new List<NPCData>();

    foreach (TodayApplicant applicant in todayApplicants)
    {
        if (applicant.isInTodayList && applicant.displayData != null)
        {
            todayListData.Add(applicant.displayData);
        }
    }

    if (todayListManager != null)
    {
        todayListManager.GenerateTodayList(todayListData);
    }

    // =====================================================
    // Debug
    // =====================================================
    Debug.Log("NPC ทั้งวัน = " + todayApplicants.Count);

    if (CampManager.Instance != null)
    {
        CampManager.Instance.PrintAllCamps();
    }
}

    // =========================================================
    // Spawn NPC ทีละคน
    // =========================================================
    public void SpawnNextNPC()
    {
        if (gameManager.currentNPC != null)
            return;

        if (currentApplicantIndex >= todayApplicants.Count)
        {
            Debug.Log("NPC วันนี้หมดแล้ว");
            return;
        }

        TodayApplicant applicant = todayApplicants[currentApplicantIndex];
        currentApplicantIndex++;

// Spawn NPC
        GameObject npc = Instantiate(
            applicant.spawnPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

// ===== ส่งข้อมูลเข้า NPC =====
        NPC npcScript = npc.GetComponent<NPC>();

        if (npcScript != null)
        {
            // เก็บ Applicant ทั้งตัว
            npcScript.applicant = applicant;

            // เก็บ NPCData ที่ใช้วันนี้
            npcScript.data = applicant.displayData;
        }

// เก็บ NPC ปัจจุบันใน GameManager
        gameManager.currentNPC = npc;
        gameManager.currentState = GameManager.NPCState.WalkingToCheckpoint;

// ให้เดินไป Stop Point
        NPCMovement movement = npc.GetComponent<NPCMovement>();

        if (movement != null)
        {
            movement.MoveTo(stopPoint.position);
        }
    }

    // =========================================================
    // เลือก Role ตาม %
    // =========================================================
    private RoleGroup ChooseRole()
    {
        List<RoleGroup> roles = new()
        {
            allData.villager,
            allData.monk,
            allData.special
        };

        int totalWeight = roles.Sum(r => r.spawnChance);

        int random = Random.Range(0, totalWeight);

        int current = 0;

        foreach (RoleGroup role in roles)
        {
            current += role.spawnChance;

            if (random < current)
                return role;
        }

        return roles.Last();
    }

    // =========================================================
    // ทุก ๆ 4 คน ต้องมี Special
    // =========================================================
    private bool NeedSpecial(int currentIndex)
    {
        if (!forceSpecial)
            return false;

        if ((currentIndex + 1) % specialWindow != 0)
            return false;

        int start = Mathf.Max(0, currentIndex - (specialWindow - 1));

        for (int i = start; i < currentIndex; i++)
        {
            if (todayApplicants[i].npcData.roleType == NPCType.Special)
                return false;
        }

        return true;
    }

    // =========================================================
    // เลือก DataPrefabNPC ภายใน Role
    // =========================================================
    private DataPrefabNPC ChooseNPC(RoleGroup role)
    {
        if (role.npcs == null || role.npcs.Count == 0)
            return null;

        int totalWeight = role.npcs.Sum(n => n.chance);

        if (totalWeight <= 0)
            return null;

        int random = Random.Range(0, totalWeight);
        int current = 0;

        foreach (NPCGroupChance npc in role.npcs)
        {
            current += npc.chance;

            if (random < current)
                return npc.npc;
        }

        return role.npcs.Last().npc;
    }

    // =========================================================
    // สร้าง Applicant
    // =========================================================
    private TodayApplicant CreateApplicant(DataPrefabNPC npc)
    {
        bool isGood = Random.Range(0, 100) < npc.todayGood.chance;

        return new TodayApplicant
        {
            npcData = npc,
            isGood = isGood,
            isInTodayList = false
        };
    }

    // =========================================================
    // เตรียม Prefab ของ Applicant
    // =========================================================
    private void PrepareApplicantPrefab(TodayApplicant applicant)
    {
        NPCSpawnGroup group = applicant.isInTodayList
            ? (applicant.isGood ? applicant.npcData.todayGood : applicant.npcData.todayBad)
            : (applicant.isGood ? applicant.npcData.normalGood : applicant.npcData.normalBad);

        applicant.spawnPrefab = GetRandomPrefab(group);

        if (applicant.spawnPrefab == null)
        {
            Debug.LogError($"{applicant.npcData.name} ไม่มี Prefab ในกลุ่มนี้");
            return;
        }

        NPC npc = applicant.spawnPrefab.GetComponent<NPC>();

        if (npc == null)
        {
            Debug.LogError($"{applicant.spawnPrefab.name} ไม่มี Script NPC");
            return;
        }

        if (npc.data == null)
        {
            Debug.LogError($"{applicant.spawnPrefab.name} ยังไม่ได้ใส่ NPCData ใน Script NPC");
            return;
        }

        applicant.displayData = npc.data;
    }

    // =========================================================
    // สุ่ม Prefab ตาม %
    // =========================================================
    private GameObject GetRandomPrefab(NPCSpawnGroup group)
    {
        if (group.prefabs == null || group.prefabs.Count == 0)
            return null;

        int totalWeight = group.prefabs.Sum(p => p.chance);

        if (totalWeight <= 0)
            return null;

        int random = Random.Range(0, totalWeight);

        int current = 0;

        foreach (NPCPrefabChance prefab in group.prefabs)
        {
            current += prefab.chance;

            if (random < current)
                return prefab.prefab;
        }

        return group.prefabs.Last().prefab;
    }

    // =========================================================
    // รีเซ็ตวันใหม่
    // =========================================================
    public void ResetToday()
    {
        todayApplicants.Clear();
        currentApplicantIndex = 0;
    }
}