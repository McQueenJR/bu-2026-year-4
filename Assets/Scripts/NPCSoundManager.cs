using UnityEngine;

public class NPCSoundManager : MonoBehaviour
{
    public static NPCSoundManager Instance;

    [Header("WALK")]
    public AudioSource walkAudioSource;
    
    [Header("DOCUMENT")]
    public AudioSource documentOpenSource;
    public AudioSource documentCloseSource;

    private int walkingCount = 0;

    private bool muteWalk = false;
    void Awake()
    {
        Instance = this;
    }

    public void PlayWalk()
    {
        walkingCount++;
        
        // ถ้าถูกปิดเสียงอยู่ ไม่ต้องเล่นเสียง แต่ยังนับ count ไว้ตามปกติ
        if (muteWalk) return;

        if (walkAudioSource != null && !walkAudioSource.isPlaying)
            walkAudioSource.Play();
    }

    public void StopWalk()
    {
        walkingCount = Mathf.Max(0, walkingCount - 1);

        if (walkingCount == 0 && walkAudioSource != null)
            walkAudioSource.Stop();
    }
    
    public void SetMuteWalk(bool mute)
    {
        muteWalk = mute;

        if (mute && walkAudioSource != null)
            walkAudioSource.Stop();
    }
    
    public void PlayDocumentOpen()
    {
        if (documentOpenSource != null)
            documentOpenSource.Play();
    }

    public void PlayDocumentClose()
    {
        if (documentCloseSource != null)
            documentCloseSource.Play();
    }
    
}