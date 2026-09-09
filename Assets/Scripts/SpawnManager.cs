using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class NPCRole
{
    [Header("ชื่อ Role (แค่ให้ดูใน Inspector)")]
    public string roleName;

    [Header("ประเภท NPC ของ Role นี้")]
    public NPCType npcType;

    [Header("โอกาสสปาว (%) - ปรับได้")]
    [Range(0f, 100f)]
    public float spawnChance = 50f;

    [Header("Prefab ที่อยู่ใน Role นี้")]
    public GameObject[] prefabs;

    // จำ prefab ตัวล่าสุดที่สปาวไปของ role นี้ กันไม่ให้ออกซ้ำติดกัน
    [System.NonSerialized]
    public HashSet<GameObject> usedPrefabsToday = new HashSet<GameObject>();
    
    
}

public class SpawnManager : MonoBehaviour
{
    [Header("Roles")]
    public NPCRole[] roles;

    [Header("จุดสปาว")]
    public Transform spawnPoint;
    public Transform stopPoint;

    [Header("อ้างอิง GameManager")]
    public GameManager gameManager;

    [Header("Today List")]
    public TodayListManager todayListManager;

// เก็บ Prefab NPC ที่จะเกิดในวันนี้
    public List<GameObject> todayApplicants = new List<GameObject>();

    private int currentApplicantIndex = 0;
    

    // =========================
    // MAIN SPAWN
    // =========================
    public void SpawnNPC()
    {
        if (gameManager.currentNPC != null)
            return;

        // ⭐ เพิ่มส่วนนี้
        // Spawn ตาม Today List ก่อน
        if (todayApplicants.Count > 0)
        {
            if (currentApplicantIndex >= todayApplicants.Count)
            {
                Debug.Log("NPC วันนี้ Spawn ครบแล้ว");
                return;
            }

            GameObject prefabToday = todayApplicants[currentApplicantIndex];

            Debug.Log("Spawn NPC : " + prefabToday.name);

            currentApplicantIndex++;

            SpawnPrefab(prefabToday, null);
            return;
        }

        NPCRole chosenRole = ChooseRole();

        if (chosenRole == null)
        {
            Debug.LogError("เลือก Role ไม่ได้");
            return;
        }

        GameObject prefab = ChoosePrefabFromRole(chosenRole);

        if (prefab == null)
        {
            Debug.LogError("ไม่มี prefab");
            return;
        }

        SpawnPrefab(prefab, chosenRole);
    }

    // =========================
    // เลือก ROLE
    // =========================
    private NPCRole ChooseRole()
    {
        //สุ่มแบบ Weighted ตาม % ที่ตั้งไว้
        var validRoles = roles
            .Where(r => r.prefabs != null && r.prefabs.Length > 0)
            .Where(r => r.prefabs.Any(p => p != null && !r.usedPrefabsToday.Contains(p))) 
            .ToList();

        float totalWeight = validRoles.Sum(r => r.spawnChance);

        if (totalWeight <= 0f)
            return validRoles.FirstOrDefault();

        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var role in validRoles)
        {
            cumulative += role.spawnChance;

            if (rand <= cumulative)
                return role;
        }

        return validRoles.LastOrDefault();
    }


    // =========================
    // เลือก PREFAB ใน ROLE
    // =========================
    private GameObject ChoosePrefabFromRole(NPCRole role)
    {
        List<GameObject> available = role.prefabs
            .Where(p => p != null && !role.usedPrefabsToday.Contains(p))
            .ToList();


        if (available.Count == 0)
            return null;

        int index = Random.Range(0, available.Count);
        GameObject chosen = available[index];
        role.usedPrefabsToday.Add(chosen);
        return chosen;
        
    }

    // =========================
    // SPAWN จริง
    // =========================
    private void SpawnPrefab(GameObject prefab, NPCRole role)
    {
        GameObject npc = Instantiate(
            prefab,
            spawnPoint.position,
            Quaternion.identity
        );
        
        gameManager.currentState = GameManager.NPCState.WalkingToCheckpoint;

        NPCMovement movement = npc.GetComponent<NPCMovement>();

        if (movement == null)
        {
            Debug.LogError(prefab.name + " ไม่มี NPCMovement");
            return;
        }

        movement.MoveTo(stopPoint.position);
    }
    
    public void GenerateTodayApplicants()
    {
        todayApplicants.Clear();
        currentApplicantIndex = 0;
        foreach (var role in roles)
            role.usedPrefabsToday.Clear();

        // ⭐ เก็บเป็นคู่ (prefab, data) เพื่อ shuffle พร้อมกันแบบไม่หลุด sync
        List<(GameObject prefab, NPCData data)> todayPairs = new List<(GameObject, NPCData)>();

        int safety = 0;
        int maxSafety = gameManager.npcPerDay * 20;

        while (todayPairs.Count < gameManager.npcPerDay && safety < maxSafety)
        {
            safety++;

            NPCRole role = ChooseRole();
            if (role == null)
                break;

            GameObject prefab = ChoosePrefabFromRole(role);
            if (prefab == null)
                continue;

            NPC npc = prefab.GetComponent<NPC>();
            NPCData data = (npc != null) ? npc.data : null;

            todayPairs.Add((prefab, data));
        }

        // ⭐ สุ่มลำดับการมาอีกที (Fisher–Yates shuffle)
        for (int i = todayPairs.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (todayPairs[i], todayPairs[j]) = (todayPairs[j], todayPairs[i]);
        }

        // แยกกลับเป็น 2 list ตามลำดับใหม่ที่ shuffle แล้ว
        List<NPCData> todayData = new List<NPCData>();
        foreach (var pair in todayPairs)
        {
            todayApplicants.Add(pair.prefab);
            if (pair.data != null)
            {
                todayData.Add(pair.data);
                Debug.Log("Today NPC : " + pair.data.npcName);
            }
        }

        Debug.Log("สุ่ม NPC วันนี้ทั้งหมด = " + todayData.Count);

        if (todayListManager != null)
        {
            todayListManager.GenerateTodayList(todayData);
        }
        else
        {
            Debug.LogError("TodayListManager ยังไม่ได้ใส่ใน SpawnManager");
        }
    }
    
    
}