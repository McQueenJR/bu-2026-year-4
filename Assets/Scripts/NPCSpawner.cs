using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject[] npcPrefabs;

    public Transform spawnPoint;
    public Transform stopPoint;

    public GameManager gameManager;

    public void SpawnNPC()
    {
        if (gameManager.currentNPC != null) return;   // มี NPC อยู่แล้ว ห้าม spawn ซ้อน

        int randomIndex = Random.Range(0, npcPrefabs.Length);

        GameObject npc = Instantiate(
            npcPrefabs[randomIndex],
            spawnPoint.position,
            Quaternion.identity);

        gameManager.currentNPC = npc;
        gameManager.currentState = GameManager.NPCState.WalkingToCheckpoint;

        npc.GetComponent<NPCMovement>().MoveTo(stopPoint.position);
    }
}