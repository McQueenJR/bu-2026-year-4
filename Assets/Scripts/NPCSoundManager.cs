using UnityEngine;

public class NPCSoundManager : MonoBehaviour
{
    public static NPCSoundManager Instance;
    public AudioSource walkAudioSource;

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
}