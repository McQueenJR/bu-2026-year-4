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
    public GameObject lastSpawnedPrefab;
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

    [Header("กฎบังคับโจร")]
    [Tooltip("ทุกๆ กี่ตัว ต้องมีโจรอย่างน้อย 1 ตัว")]
    public int checkWindow = 4;

    [Tooltip("จำนวนโจรขั้นต่ำในแต่ละช่วง (window)")]
    public int minRobberInWindow = 1;

    // เก็บ history ของ NPCType ที่สปาวไปแล้วในวันนี้ (เรียงตามลำดับ)
    private List<NPCType> spawnHistory = new List<NPCType>();

    // =========================
    // MAIN SPAWN
    // =========================
    public void SpawnNPC()
    {
        if (gameManager.currentNPC != null)
            return;

        if (roles == null || roles.Length == 0)
        {
            Debug.LogError("ยังไม่ได้ตั้งค่า Role ใน SpawnManager");
            return;
        }

        NPCRole chosenRole = ChooseRole();

        if (chosenRole == null)
        {
            Debug.LogError("เลือก Role ไม่ได้ (เช็คว่าใส่ prefab และตั้ง % แล้วหรือยัง)");
            return;
        }

        GameObject prefab = ChoosePrefabFromRole(chosenRole);

        if (prefab == null)
        {
            Debug.LogError("ไม่มี prefab ใน role: " + chosenRole.roleName);
            return;
        }

        SpawnPrefab(prefab, chosenRole);
    }

    // =========================
    // เลือก ROLE
    // =========================
    private NPCRole ChooseRole()
    {
        // 1) เช็คกฎบังคับโจรก่อน
        if (MustForceRobber())
        {
            NPCRole robberRole = roles.FirstOrDefault(
                r => r.npcType == NPCType.Robber &&
                     r.prefabs != null &&
                     r.prefabs.Length > 0);

            if (robberRole != null)
                return robberRole;

            Debug.LogWarning("ควรบังคับ Robber แต่ไม่มี Role Robber ที่ตั้ง prefab ไว้");
        }

        // 2) สุ่มแบบ Weighted ตาม % ที่ตั้งไว้
        var validRoles = roles
            .Where(r => r.prefabs != null && r.prefabs.Length > 0)
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

    // เช็คว่ารอบนี้ต้องบังคับ Robber หรือไม่
    // กฎ: ทุกๆ checkWindow ตัว ต้องมี Robber อย่างน้อย minRobberInWindow ตัว
    private bool MustForceRobber()
    {
        if (checkWindow <= 0)
            return false;

        int nextSpawnNumber = spawnHistory.Count + 1;

        // เช็คเฉพาะตอนที่กำลังจะครบรอบ (ตัวที่ 4, 8, 12, ...)
        if (nextSpawnNumber % checkWindow != 0)
            return false;

        int windowStart = spawnHistory.Count - (checkWindow - 1);
        if (windowStart < 0) windowStart = 0;

        var window = spawnHistory.Skip(windowStart).Take(checkWindow - 1);

        int robberCountInWindow = window.Count(t => t == NPCType.Robber);

        return robberCountInWindow < minRobberInWindow;
    }

    // =========================
    // เลือก PREFAB ใน ROLE
    // =========================
    private GameObject ChoosePrefabFromRole(NPCRole role)
    {
        List<GameObject> available = role.prefabs
            .Where(p => p != null)
            .ToList();

        // ตัด prefab ตัวล่าสุดออก กันไม่ให้ออกซ้ำติดกัน (ถ้ามีตัวเลือกมากกว่า 1)
        if (role.lastSpawnedPrefab != null && available.Count > 1)
        {
            available.Remove(role.lastSpawnedPrefab);
        }

        if (available.Count == 0)
            return null;

        int index = Random.Range(0, available.Count);
        return available[index];
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

        role.lastSpawnedPrefab = prefab;
        spawnHistory.Add(role.npcType);

        gameManager.currentNPC = npc;
        gameManager.currentState = GameManager.NPCState.WalkingToCheckpoint;

        NPCMovement movement = npc.GetComponent<NPCMovement>();

        if (movement == null)
        {
            Debug.LogError("NPC Prefab ไม่มี NPCMovement");
            return;
        }

        movement.MoveTo(stopPoint.position);
    }

    // =========================
    // เรียกตอนขึ้นวันใหม่ เพื่อรีเซ็ต history
    // =========================
    public void ResetHistory()
    {
        spawnHistory.Clear();

        foreach (var role in roles)
            role.lastSpawnedPrefab = null;
    }
}