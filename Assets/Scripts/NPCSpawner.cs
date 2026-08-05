using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject[] npcPrefabs;

    public Transform spawnPoint;
    public Transform stopPoint;

    public GameManager gameManager;

    public void SpawnNPC()
    {
        int randomIndex = Random.Range(0, npcPrefabs.Length);

        GameObject npc = Instantiate(
            npcPrefabs[randomIndex],
            spawnPoint.position,
            Quaternion.identity);

        npc.GetComponent<NPCMovement>().MoveTo(stopPoint.position);

        gameManager.currentNPC = npc;
    }
}