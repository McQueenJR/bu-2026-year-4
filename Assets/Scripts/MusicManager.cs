using System.Collections;
using UnityEngine;

// คุมเพลงพื้นหลัง: เพลงตอนเล่นเกม <-> เพลงตอนหน้าสรุปจบวัน
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Source (ติ๊ก Loop ไว้ทั้งสองตัว / Play On Awake ปิด)")]
    public AudioSource gameplayBgm;
    public AudioSource endDayMusic;

    [Header("Fade")]
    [Tooltip("วินาทีที่ใช้ค่อยๆ เฟดสลับเพลง (0 = สลับทันที)")]
    public float fadeDuration = 0.5f;

    private float gameplayVolume = 1f;
    private float endDayVolume = 1f;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        Instance = this;

        // จำความดังที่ตั้งไว้ใน Inspector ไว้เป็นค่าเป้าหมายตอนเฟด
        if (gameplayBgm != null) gameplayVolume = gameplayBgm.volume;
        if (endDayMusic != null) endDayVolume = endDayMusic.volume;
    }

    private void Start()
    {
        PlayGameplayBGM();
    }

    public void PlayGameplayBGM()
    {
        SwitchTo(gameplayBgm, gameplayVolume, endDayMusic, endDayVolume);
    }

    public void PlayEndDayMusic()
    {
        SwitchTo(endDayMusic, endDayVolume, gameplayBgm, gameplayVolume);
    }

    private void SwitchTo(AudioSource on, float onVol, AudioSource off, float offVol)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(SwitchRoutine(on, onVol, off, offVol));
    }

    private IEnumerator SwitchRoutine(AudioSource on, float onVol, AudioSource off, float offVol)
    {
        float d = Mathf.Max(0f, fadeDuration);
        float offStart = off != null ? off.volume : 0f;

        if (on != null && !on.isPlaying)
        {
            on.volume = d > 0f ? 0f : onVol;
            on.Play();
        }

        float t = 0f;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / d);

            if (on != null)  on.volume  = Mathf.Lerp(0f, onVol, k);
            if (off != null) off.volume = Mathf.Lerp(offStart, 0f, k);

            yield return null;
        }

        if (on != null) on.volume = onVol;

        if (off != null)
        {
            off.Stop();
            off.volume = offVol;   // คืนความดังไว้ ครั้งหน้าเล่นแล้วดังตามที่ตั้ง
        }

        fadeRoutine = null;
    }
}