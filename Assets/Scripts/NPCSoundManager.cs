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

    void Awake()
    {
        Instance = this;
    }

    public void PlayWalk()
    {
        walkingCount++;

        if (walkAudioSource != null && !walkAudioSource.isPlaying)
            walkAudioSource.Play();
    }

    public void StopWalk()
    {
        walkingCount = Mathf.Max(0, walkingCount - 1);

        if (walkingCount == 0 && walkAudioSource != null)
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