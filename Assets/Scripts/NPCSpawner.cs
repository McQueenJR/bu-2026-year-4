using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject[] npcPrefabs;

    public Transform spawnPoint;
    public Transform stopPoint;

    public GameManager gameManager;

    public void SpawnNPC()
    {
        if (gameManager.currentNPC != null)
            return;
        
        if (npcPrefabs.Length == 0)
        {
            Debug.LogError("ยังไม่ได้ใส่ NPC Prefab ใน NPCSpawner");
            return;
        }
        
        int randomIndex = Random.Range(0, npcPrefabs.Length);

        GameObject npc = Instantiate(
            npcPrefabs[randomIndex],
            spawnPoint.position,
            Quaternion.identity
        );
        
        gameManager.currentNPC = npc;
        
        gameManager.currentState =
            GameManager.NPCState.WalkingToCheckpoint;
        
        NPCMovement movement =
            npc.GetComponent<NPCMovement>();

        if (movement == null)
        {
            Debug.LogError("NPC Prefab ไม่มี NPCMovement");
            return;
        }

        movement.MoveTo(stopPoint.position);
    }
}