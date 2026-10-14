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

    if (allData == null || gameManager == null)
    {
        Debug.LogError(
            "SpawnManager: ยังไม่ได้กำหนด allData หรือ gameManager"
        );
        return;
    }

    int npcToday = Random.Range(
        allData.minNPCPerDay,
        allData.maxNPCPerDay + 1
    );

    gameManager.npcPerDay = npcToday;

    List<DataPrefabNPC> allNPCs = new List<DataPrefabNPC>();

    void AddRoleNPCs(RoleGroup role)
    {
        if (role == null || role.npcs == null)
            return;

        foreach (NPCGroupChance entry in role.npcs)
        {
            if (entry.npc != null && !allNPCs.Contains(entry.npc))
                allNPCs.Add(entry.npc);
        }
    }

    AddRoleNPCs(allData.villager);
    AddRoleNPCs(allData.monk);
    AddRoleNPCs(allData.special);

    allNPCs = allNPCs
        .Where(n =>
        {
            NPCData data = GetNPCDataFromPrefabData(n);

            return data != null &&
                (CampManager.Instance == null ||
                 CampManager.Instance.IsAlive(data));
        })
        .ToList();

    if (allNPCs.Count == 0)
    {
        Debug.LogError("ไม่มี DPN ที่สามารถสร้าง NPC ได้");
        return;
    }

    int minCount = Mathf.Clamp(
        allData.minTodayList, 1, allNPCs.Count
    );

    int maxCount = Mathf.Clamp(
        allData.maxTodayList, minCount, allNPCs.Count
    );

    int requestedTodayListCount = Random.Range(
        minCount, maxCount + 1
    );

    // เตรียม Applicant และ Prefab ก่อนตรวจชื่อจริง
    Dictionary<DataPrefabNPC, HashSet<GameObject>>
        usedPrefabsPerNPC = new();

    HashSet<NPCData> usedTodayListNames = new();
    List<DataPrefabNPC> shuffledDPNs = allNPCs
        .OrderBy(_ => Random.value)
        .ToList();

    int safety = 2000;

    // เลือก Applicant ของ Today List ที่มี displayData ไม่ซ้ำ
    foreach (DataPrefabNPC source in shuffledDPNs)
    {
        if (usedTodayListNames.Count >= requestedTodayListCount)
            break;

        // ทดลองทั้งกลุ่ม todayGood/todayBad
        List<NPCSpawnGroup> groups = new List<NPCSpawnGroup>();

        if (source.todayGood != null &&
            source.todayGood.chance > 0 &&
            source.todayGood.prefabs != null)
            groups.Add(source.todayGood);

        if (source.todayBad != null &&
            source.todayBad.chance > 0 &&
            source.todayBad.prefabs != null)
            groups.Add(source.todayBad);

        groups = groups.OrderBy(_ => Random.value).ToList();

        foreach (NPCSpawnGroup group in groups)
        {
            if (safety-- <= 0)
                break;

            if (group.prefabs == null ||
                !group.prefabs.Any(p =>
                    p.prefab != null && p.chance > 0))
                continue;

            TodayApplicant applicant = new TodayApplicant
            {
                npcData = source,
                selectedGroup = group,
                isGood = group == source.todayGood,
                isInTodayList = true,
                isGhost = group.isGhost,
                hasEnteredToday = false
            };

            PrepareApplicantPrefab(applicant, usedPrefabsPerNPC);

            if (applicant.spawnPrefab == null ||
                applicant.displayData == null)
                continue;

            // ใช้ NPCData ของ Prefab จริงในการป้องกันชื่อซ้ำ
            if (usedTodayListNames.Contains(applicant.displayData))
                continue;

            usedTodayListNames.Add(applicant.displayData);
            todayApplicants.Add(applicant);

            break;
        }
    }

    // เติม NPC ที่เหลือจาก DPN ทั้งหมด
    safety = 2000;

    while (todayApplicants.Count < npcToday && safety-- > 0)
    {
        DataPrefabNPC source = allNPCs[
            Random.Range(0, allNPCs.Count)
        ];

        TodayApplicant applicant = CreateApplicant(source, false);

        if (applicant == null)
            continue;

        PrepareApplicantPrefab(applicant, usedPrefabsPerNPC);

        if (applicant.spawnPrefab == null ||
            applicant.displayData == null)
            continue;

        applicant.hasEnteredToday = false;
        todayApplicants.Add(applicant);
    }

    if (todayApplicants.Count < npcToday)
    {
        Debug.LogWarning(
            $"สร้าง NPC ได้ {todayApplicants.Count}/{npcToday} คน"
        );
    }

    // สุ่มลำดับก่อน Spawn
    todayApplicants = todayApplicants
        .OrderBy(_ => Random.value)
        .ToList();

    // สร้างเอกสาร
    int today = gameManager.currentDay;

    foreach (TodayApplicant applicant in todayApplicants)
    {
        if (applicant.displayData == null)
            continue;

        applicant.hasTempleDocument = true;

        applicant.templeDocument = TempleDocumentGenerator.Generate(
            applicant.displayData.templeDocumentData,
            today,
            true
        );
    }

    // สุ่ม NPC ใน Today List หนึ่งคนให้ไม่มีเอกสาร
    List<TodayApplicant> todayListApplicants = todayApplicants
        .Where(x => x.isInTodayList)
        .ToList();

    if (todayListApplicants.Count > 0)
    {
        TodayApplicant noDocumentApplicant =
            todayListApplicants[
                Random.Range(0, todayListApplicants.Count)
            ];

        noDocumentApplicant.hasTempleDocument = false;
        noDocumentApplicant.templeDocument = null;

        if (CampManager.Instance != null &&
            noDocumentApplicant.displayData != null)
        {
            CampManager.Instance.SetDocumentInCamp(
                noDocumentApplicant.displayData,
                false
            );
        }
    }

    // ส่งรายชื่อไม่ซ้ำเข้า UI
    List<NPCData> todayListData = todayListApplicants
        .Where(x => x.displayData != null)
        .Where(x => x.isGhost ||
            CampManager.Instance == null ||
            !CampManager.Instance.IsHomeToday(x.displayData))
        .Select(x => x.displayData)
        .Distinct()
        .ToList();

    if (todayListManager != null)
        todayListManager.GenerateTodayList(todayListData);

    Debug.Log(
        $"NPC ทั้งวัน: {todayApplicants.Count}/{npcToday}, " +
        $"Today List: {todayListData.Count}/{requestedTodayListCount}"
    );

    if (CampManager.Instance != null)
        CampManager.Instance.PrintAllCamps();
}

  

    // =========================================================
    // Spawn NPC ทีละคน
    // =========================================================
    public void SpawnNextNPC()
    {
        // ★ BadEnd ถูก trigger → หยุด spawn ปกติทันที
        if (EndingManager.Instance != null &&
            EndingManager.Instance.IsBadEndTriggered)
        {
            Debug.Log("===== BAD END ถูก trigger → หยุด Spawn =====");
            return;
        }
        
        if (gameManager.currentNPC != null)
            return;

        if (currentApplicantIndex >= todayApplicants.Count)
        {
            Debug.Log("===== NPC วันนี้หมดแล้ว → จบวัน =====");

            gameManager.EndGame();

            return;
        }

        TodayApplicant applicant = null;

// ข้าม NPC ที่อยู่ Camp วันนี้
        while (currentApplicantIndex < todayApplicants.Count)
        {
            TodayApplicant nextApplicant = todayApplicants[currentApplicantIndex];
            currentApplicantIndex++;
            if (nextApplicant.hasEnteredToday)
            {
                Debug.Log($"{nextApplicant.displayData.npcName} เข้าวัดแล้ววันนี้ → ข้าม");
                continue;
            }

            
            // NPC จริงที่อยู่ Camp วันนี้ ไม่ต้อง Spawn
            if (!nextApplicant.isGhost &&
                nextApplicant.displayData != null &&
                CampManager.Instance != null &&
                CampManager.Instance.IsHomeToday(nextApplicant.displayData))
            {
                Debug.Log(
                    $"{nextApplicant.displayData.npcName} " +
                    "อยู่ Camp วันนี้ → ข้ามการ Spawn NPC จริง"
                );

                continue;
            }

            applicant = nextApplicant;
            break;
        }

// ไม่มี NPC ที่ต้องตรวจแล้ว
        if (applicant == null)
        {
            Debug.Log("===== NPC ที่ต้องตรวจวันนี้หมดแล้ว → จบวัน =====");

            gameManager.EndGame();

            return;
        }

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
            
            // สุ่มเลือกข้อความ checklist ของแต่ละหัวข้อไว้ล่วงหน้า ตั้งแต่ตอนสปาวน์
            if (npcScript.data != null)
            {
                npcScript.data.InitializeChecklistQuestions();
            }
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
        List<RoleGroup> roles = new();

        if (allData.villager.npcs.Any(n => n.npc != null))
            roles.Add(allData.villager);

        if (allData.monk.npcs.Any(n => n.npc != null))
            roles.Add(allData.monk);

        if (allData.special.npcs.Any(n => n.npc != null))
            roles.Add(allData.special);

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
    // เลือก DataPrefabNPC ภายใน Role
    // =========================================================
    private DataPrefabNPC ChooseNPC(RoleGroup role)
    {
        if (role.npcs == null || role.npcs.Count == 0)
            return null;

        // เอาเฉพาะ NPC ที่ยังมีชีวิต
        List<NPCGroupChance> aliveNPCs = new List<NPCGroupChance>();

        foreach (NPCGroupChance entry in role.npcs)
        {
            if (entry.npc == null)
                continue;

            NPCData npcData = GetNPCDataFromPrefabData(entry.npc);

            // ถ้าหา NPCData ไม่เจอ ให้ข้าม
            if (npcData == null)
                continue;

            // NPC ตายแล้ว → ห้ามสุ่ม
            if (CampManager.Instance != null &&
                !CampManager.Instance.IsAlive(npcData))
            {
                Debug.Log(
                    $"☠️ {npcData.npcName} ตายแล้ว → ไม่สุ่ม Spawn"
                );

                continue;
            }

            aliveNPCs.Add(entry);
        }

        if (aliveNPCs.Count == 0)
            return null;

        int totalWeight = aliveNPCs.Sum(n => n.chance);

        if (totalWeight <= 0)
            return null;

        int random = Random.Range(0, totalWeight);
        int current = 0;

        foreach (NPCGroupChance entry in aliveNPCs)
        {
            current += entry.chance;

            if (random < current)
                return entry.npc;
        }

        return aliveNPCs.Last().npc;
    }
    
    private NPCData GetNPCDataFromPrefabData(DataPrefabNPC data)
    {
        if (data == null)
            return null;

        NPCSpawnGroup[] groups =
        {
            data.todayGood,
            data.todayBad,
            data.normalGood,
            data.normalBad
        };

        foreach (NPCSpawnGroup group in groups)
        {
            if (group == null || group.prefabs == null)
                continue;

            foreach (NPCPrefabChance prefabChance in group.prefabs)
            {
                if (prefabChance.prefab == null)
                    continue;

                NPC npc = prefabChance.prefab.GetComponent<NPC>();

                if (npc != null && npc.data != null)
                    return npc.data;
            }
        }

        return null;
    }

    // =========================================================
    // สร้าง Applicant
    // =========================================================
    private TodayApplicant CreateApplicant(
        DataPrefabNPC npc,
        bool isInTodayList)
    {
        if (npc == null)
            return null;

        NPCSpawnGroup selectedGroup = ChooseSpawnGroup(
            npc,
            isInTodayList
        );

        if (selectedGroup == null)
            return null;

        return new TodayApplicant
        {
            npcData = npc,
            selectedGroup = selectedGroup,
            isGood = selectedGroup == npc.todayGood ||
                     selectedGroup == npc.normalGood,
            isInTodayList = isInTodayList,
            isGhost = selectedGroup.isGhost
        };
    }
    
    private NPCSpawnGroup ChooseSpawnGroup(
        DataPrefabNPC npc,
        bool isInTodayList)
    {
        if (npc == null)
            return null;

        NPCSpawnGroup goodGroup = isInTodayList
            ? npc.todayGood
            : npc.normalGood;

        NPCSpawnGroup badGroup = isInTodayList
            ? npc.todayBad
            : npc.normalBad;

        List<NPCSpawnGroup> groups = new List<NPCSpawnGroup>();

        if (goodGroup != null &&
            goodGroup.chance > 0 &&
            goodGroup.prefabs != null &&
            goodGroup.prefabs.Any(p => p.prefab != null && p.chance > 0))
        {
            groups.Add(goodGroup);
        }

        if (badGroup != null &&
            badGroup.chance > 0 &&
            badGroup.prefabs != null &&
            badGroup.prefabs.Any(p => p.prefab != null && p.chance > 0))
        {
            groups.Add(badGroup);
        }

        if (groups.Count == 0)
            return null;

        int totalWeight = groups.Sum(g => g.chance);
        int random = Random.Range(0, totalWeight);
        int current = 0;

        foreach (NPCSpawnGroup group in groups)
        {
            current += group.chance;

            if (random < current)
                return group;
        }

        return groups[groups.Count - 1];
    }

    // =========================================================
    // เตรียม Prefab ของ Applicant
    // =========================================================
   
    private void PrepareApplicantPrefab(
        TodayApplicant applicant,
        Dictionary<DataPrefabNPC, HashSet<GameObject>> usedPrefabsPerNPC)
    {
        NPCSpawnGroup group = applicant.selectedGroup;

        if (group == null)
        {
            Debug.LogError(
                $"Applicant ของ {applicant.npcData.name} ไม่มี Selected Group"
            );
            return;
        }

        // กำหนดว่า Applicant เป็น Ghost หรือไม่
        applicant.isGhost = group.isGhost;

        // ใช้ DataPrefabNPC เป็นตัวระบุ NPC ต้นทาง
        DataPrefabNPC sourceNPC = applicant.npcData;

        if (!usedPrefabsPerNPC.TryGetValue(
                sourceNPC, out HashSet<GameObject> usedPrefabs))
        {
            usedPrefabs = new HashSet<GameObject>();
            usedPrefabsPerNPC[sourceNPC] = usedPrefabs;
        }

        applicant.spawnPrefab = GetRandomPrefab(group, usedPrefabs);

        if (applicant.spawnPrefab == null)
        {
            Debug.LogError(
                $"{sourceNPC.name} ไม่มี Prefab ที่สามารถใช้ได้"
            );
            return;
        }

        usedPrefabs.Add(applicant.spawnPrefab);

        NPC npc = applicant.spawnPrefab.GetComponent<NPC>();

        if (npc == null)
        {
            Debug.LogError(
                $"{applicant.spawnPrefab.name} ไม่มี Script NPC"
            );
            applicant.spawnPrefab = null;
            return;
        }

        if (npc.data == null)
        {
            Debug.LogError(
                $"{applicant.spawnPrefab.name} ยังไม่ได้ใส่ NPCData ใน Script NPC"
            );
            applicant.spawnPrefab = null;
            return;
        }

        applicant.displayData = npc.data;

        // Ghost ปลอมตัวเป็น NPC ที่กำหนดไว้ใน Prefab
        if (applicant.isGhost)
        {
            applicant.ghostTarget = applicant.displayData;

            Debug.Log(
                $"👻 Ghost Target = {applicant.ghostTarget.npcName}"
            );
        }
        else
        {
            applicant.ghostTarget = null;
        }
    }


    // =========================================================
    // สุ่ม Prefab ตาม %
    // =========================================================
    private GameObject GetRandomPrefab(NPCSpawnGroup group, HashSet<GameObject> excludedPrefabs)
    {
        if (group == null || group.prefabs == null ||
            group.prefabs.Count == 0)
            return null;

        List<NPCPrefabChance> available = group.prefabs
            .Where(p => p.prefab != null &&
                        p.chance > 0 &&
                        !excludedPrefabs.Contains(p.prefab))
            .ToList();

        // ใช้ครบทุก Prefab ในกลุ่มแล้ว จึงอนุญาตให้ใช้ซ้ำ
        if (available.Count == 0)
        {
            available = group.prefabs
                .Where(p => p.prefab != null && p.chance > 0)
                .ToList();
        }

        int totalWeight = available.Sum(p => p.chance);

        if (totalWeight <= 0)
            return null;

        int random = Random.Range(0, totalWeight);
        int current = 0;

        foreach (NPCPrefabChance prefab in available)
        {
            current += prefab.chance;

            if (random < current)
                return prefab.prefab;
        }

        return available[available.Count - 1].prefab;
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