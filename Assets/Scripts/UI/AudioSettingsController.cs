using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    [Header("UI Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    private const string BGM_KEY = "BGMVolume";
    private const string SFX_KEY = "SFXVolume";
    private const string BGM_PREF = "BGMVolumePref";
    private const string SFX_PREF = "SFXVolumePref";

    void Start()
    {
        // โหลดค่าที่เคยบันทึกไว้ (ถ้ายังไม่เคยตั้งค่า จะใช้ 0.75 เป็นค่าเริ่มต้น)
        float savedBGM = PlayerPrefs.GetFloat(BGM_PREF, 0.75f);
        float savedSFX = PlayerPrefs.GetFloat(SFX_PREF, 0.75f);

        bgmSlider.value = savedBGM;
        sfxSlider.value = savedSFX;

        SetBGMVolume(savedBGM);
        SetSFXVolume(savedSFX);

        // ผูก event ตอนลาก slider
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetBGMVolume(float sliderValue)
    {
        // sliderValue อยู่ในช่วง 0.0001 - 1 (ห้ามเป็น 0 เป๊ะ เพราะ Log10(0) หาค่าไม่ได้)
        float dB = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
        mainMixer.SetFloat(BGM_KEY, dB);
        PlayerPrefs.SetFloat(BGM_PREF, sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float dB = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
        mainMixer.SetFloat(SFX_KEY, dB);
        PlayerPrefs.SetFloat(SFX_PREF, sliderValue);
    }
}