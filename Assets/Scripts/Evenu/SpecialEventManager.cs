
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum SpecialEventType
{
    Pob,
    Pret
}
public class SpecialEventManager : MonoBehaviour
{
    [Header("Camera Return")]
    public bool returnCameraAfterEvent = true;
    [Serializable]
    public class SpecialEventConfig
    {
        [Header("Event Info")]
        public string eventName;
        public bool enableEvent = true;
        public SpecialEventType eventType;
        
        [Header("Spawn Settings")]
        public GameObject npcPrefab;

        [Range(0f, 100f)]
        public float spawnChance = 10f;

        [Min(0)]
        public int minCount = 0;

        [Min(0)]
        public int maxCount = 1;

        [Header("Arrival Animation")]
        public string animationTrigger = "LightOn";

        [Header("Camera Event")]
        public Transform faceTarget;
        [Min(0.1f)]
        public float zoomDuration = 2f;
        [Min(0.1f)]
        public float zoomDistance = 0.5f;
        [Min(0f)]
        public float maxShakeStrength = 0.12f;
        [Min(0f)]
        public float maxShakeSpeed = 35f;

        [Header("Sound")]
        public AudioClip eventSound;
    }
    
    

    [Header("Exit Animation")]
    public string exitAnimationTrigger = "StandUp";

    [Header("Walking Camera Shake")]
    public bool shakeCameraWhileWalking = false;
    [Min(0f)] public float walkingShakeStrength = 0.04f;
    [Min(0f)] public float walkingShakeSpeed = 12f;
    
    
    [Header("References")]
    public DialogManager dialogManager;
    [Header("Special Events")]
    public List<SpecialEventConfig> specialEvents =
        new List<SpecialEventConfig>();
    
    [Header("Camera Event")]
    public Camera mainCamera;
    public float shakeDuration = 2f;
    public float returnDuration = 0.5f;

    public SpecialEventConfig GetEventConfig(GameObject npc)
    {
        if (npc == null)
            return null;

        foreach (SpecialEventConfig config in specialEvents)
        {
            if (config == null || !config.enableEvent ||
                config.npcPrefab == null)
                continue;

            if (npc.name == config.npcPrefab.name ||
                npc.name.StartsWith(config.npcPrefab.name + " ("))
            {
                return config;
            }
        }

        return null;
    }

    public List<SpecialEventConfig> GetEnabledEvents()
    {
        return specialEvents.FindAll(
            config => config != null &&
                      config.enableEvent &&
                      config.npcPrefab != null
        );
    }

    public int GetRandomCount(SpecialEventConfig config)
    {
        if (config == null)
            return 0;

        int min = Mathf.Max(0, config.minCount);
        int max = Mathf.Max(min, config.maxCount);

        return UnityEngine.Random.Range(min, max + 1);
    }

    public bool RollSpawnChance(SpecialEventConfig config)
    {
        if (config == null || !config.enableEvent)
            return false;

        return UnityEngine.Random.Range(0f, 100f)
               < config.spawnChance;
    }
    
    private readonly Dictionary<SpecialEventConfig, int> dailySpawnCounts
        = new Dictionary<SpecialEventConfig, int>();

    public void ResetDailyCounts()
    {
        dailySpawnCounts.Clear();
    }

    public bool TryAssignSpecialEvent(
        GameObject spawnedNPC,
        GameObject spawnedPrefab)
    {
        if (spawnedNPC == null || spawnedPrefab == null)
            return false;

        foreach (SpecialEventConfig config in specialEvents)
        {
            if (config == null ||
                !config.enableEvent ||
                config.npcPrefab == null)
                continue;

            // ต้องตรงกับ Prefab ที่ SpawnManager เลือกมา
            if (spawnedPrefab != config.npcPrefab)
                continue;

            dailySpawnCounts.TryGetValue(config, out int currentCount);

            // ไม่ให้เกินจำนวนสูงสุดต่อวัน
            if (currentCount >= Mathf.Max(0, config.maxCount))
                return false;

            // สุ่มโอกาสเกิด Event
            float roll = UnityEngine.Random.Range(0f, 100f);

            if (roll >= Mathf.Clamp(config.spawnChance, 0f, 100f))
            {
                return false;
            }

            SpecialEventNPC marker =
                spawnedNPC.GetComponent<SpecialEventNPC>();

            if (marker == null)
                marker = spawnedNPC.AddComponent<SpecialEventNPC>();

            marker.eventConfig = config;
            dailySpawnCounts[config] = currentCount + 1;

            Debug.Log(
                $"[SpecialEvent] {config.eventName} " +
                $"({dailySpawnCounts[config]}/{config.maxCount})"
            );

            return true;
        }

        return false;
    }
    public void StartSpecialEvent(
        GameObject npc,
        SpecialEventConfig config)
    {
        if (npc == null || config == null)
            return;

        NPC npcScript = npc.GetComponent<NPC>();

        if (npcScript == null || npcScript.data == null)
        {
            Debug.LogError(
                "[SpecialEvent] NPC or NPCData is missing!"
            );
            return;
        }

        Animator animator = npc.GetComponentInChildren<Animator>();

        // สั่ง Animation เมื่อ NPC ถึง stopPoint
        if (animator != null &&
            !string.IsNullOrEmpty(config.animationTrigger))
        {
            animator.SetTrigger(config.animationTrigger);
        }
        else if (animator == null)
        {
            Debug.LogWarning(
                $"[SpecialEvent] Animator not found: {config.eventName}"
            );
        }

        // เล่นเสียงอีเวนต์
        if (config.eventSound != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.PlayOneShot(config.eventSound);
        }

        Debug.Log(
                $"[SpecialEvent] Started: {config.eventName} " +
                $"({config.eventType})"
            );
        
        // เปิดบทสนทนาของ NPC
        if (dialogManager != null)
        {
            dialogManager.StartDialog(npcScript.data);
        }
        else
        {
            Debug.LogError(
                "[SpecialEvent] DialogManager is not assigned!"
            );
        }
    }
    
   
    public void OnSpecialDialogFinished(
        GameObject npc,
        SpecialEventConfig config)
    {
        if (npc == null || config == null) return;

        Debug.Log($"[SpecialEvent] Dialog finished: {config.eventName}");

        if (config.eventType == SpecialEventType.Pob)
        {
            if (cameraEventCoroutine != null)
                StopCoroutine(cameraEventCoroutine);

            cameraEventCoroutine = StartCoroutine(
                ZoomAndShakeCamera(config)
            );
        }
        else if (config.eventType == SpecialEventType.Pret)
        {
            Debug.Log("[SpecialEvent] Pret dialog finished");
        }
    }

    
    
private Coroutine cameraEventCoroutine;

private System.Collections.IEnumerator ZoomAndShakeCamera(
    SpecialEventConfig config)
{
    if (mainCamera == null || config == null || config.faceTarget == null)
    {
        Debug.LogWarning(
            "[SpecialEvent] Main Camera หรือ Face Target ยังไม่ได้กำหนด"
        );
        yield break;
    }

    Transform cam = mainCamera.transform;

    Vector3 originalPosition = cam.position;
    Quaternion originalRotation = cam.rotation;

    Vector3 direction =
        (config.faceTarget.position - originalPosition).normalized;

    Vector3 targetPosition =
        config.faceTarget.position - direction * config.zoomDistance;

    Quaternion targetRotation = Quaternion.LookRotation(
        config.faceTarget.position - targetPosition
    );

    float duration = Mathf.Max(0.1f, config.zoomDuration);
    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        Vector3 zoomPosition = Vector3.Lerp(
            originalPosition, targetPosition, smoothT
        );

        Quaternion zoomRotation = Quaternion.Slerp(
            originalRotation, targetRotation, smoothT
        );

        float shakeStrength = config.maxShakeStrength * t;
        float shakeSpeed = Mathf.Lerp(1f, config.maxShakeSpeed, t);

        Vector3 shakeOffset = new Vector3(
            Mathf.Sin(Time.time * shakeSpeed) * shakeStrength,
            Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeStrength,
            0f
        );

        cam.position = zoomPosition + shakeOffset;
        cam.rotation = zoomRotation;

        yield return null;
    }

    if (returnCameraAfterEvent)
    {
        elapsed = 0f;

        Vector3 currentPosition = cam.position;
        Quaternion currentRotation = cam.rotation;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            cam.position = Vector3.Lerp(
                currentPosition, originalPosition, smoothT
            );

            cam.rotation = Quaternion.Slerp(
                currentRotation, originalRotation, smoothT
            );

            yield return null;
        }
    }

    cam.position = originalPosition;
    cam.rotation = originalRotation;

    cameraEventCoroutine = null;

    Debug.Log("[SpecialEvent] Camera event finished");
}

}
