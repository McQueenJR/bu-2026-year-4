using TMPro;
using UnityEngine;
using System.Collections;

public class DialogManager : MonoBehaviour
{
    public GameObject dialogPanel;

    public TMP_Text nameText;
    public TMP_Text dialogText;

    public BagManager bagManager;
    // NPC ที่กำลังพูดอยู่
    private NPCMouthAnimation currentMouth;
    
    private string[] dialogs;
    private int currentIndex;




    // =====================================================
    // TYPEWRITER
    // =====================================================

    [Header("Typewriter")]
    [SerializeField] private float typeSpeed = 0.03f;

    [Header("Dialog Auto Close")]
    [SerializeField] private float autoNextDelay = 2f;

    private Coroutine typewriterCoroutine;
    private Coroutine autoNextCoroutine;

    // true = กำลังพิมพ์ข้อความอยู่
    private bool isTyping = false;


    // =====================================================
    // DIALOG SOUND
    // =====================================================

    [Header("Dialog Sound")]
    [SerializeField] private AudioSource voiceAudioSource;

    // เสียงพูดของตัวละคร
    [SerializeField] private AudioClip voiceClip;

    // ระดับเสียง
    [SerializeField, Range(0f, 1f)]
    private float voiceVolume = 1f;


    // =====================================================
    // DIALOG POSITION
    // =====================================================

    // ใช้ตรวจสอบว่าข้อความปัจจุบันเป็นข้อความแรกหรือไม่
    private bool IsFirstDialog()
    {
        return currentIndex == 0;
    }


    // ใช้ตรวจสอบว่าข้อความปัจจุบันเป็นข้อความสุดท้ายหรือไม่
    private bool IsLastDialog()
    {
        return dialogs != null &&
               dialogs.Length > 0 &&
               currentIndex == dialogs.Length - 1;
    }


    // =====================================================
    // DIALOG TYPE
    // =====================================================

    private enum DialogType
    {
        Normal,
        Green,
        Emergency,
        Simple,
        Checklist
    }

    private DialogType currentDialogType;


    // =====================================================
    // NPC MOUTH
    // =====================================================

    // ใช้กำหนดว่า NPC ตัวไหนกำลังพูด
    public void SetTalkingNPC(NPCMouthAnimation mouth)
    {
        // ปิดปาก NPC ตัวเก่าก่อน
        if (currentMouth != null)
        {
            currentMouth.StopTalking();
        }

        currentMouth = mouth;

        // เปิดปาก NPC ตัวใหม่
        if (currentMouth != null)
        {
            currentMouth.StartTalking();
        }
    }


    // หยุดปาก NPC ที่กำลังพูด
    private void StopTalkingNPC()
    {
        if (currentMouth != null)
        {
            currentMouth.StopTalking();
            currentMouth = null;
        }
    }


    // =====================================================
    // NORMAL
    // =====================================================

    public void StartDialog(NPCData data)
    {
        if (data == null)
            return;

        currentDialogType = DialogType.Normal;

        dialogs = data.dialogs;
        currentIndex = 0;

        nameText.text = data.npcName;

        dialogPanel.SetActive(true);

        ShowCurrentDialog();
    }


    // =====================================================
    // GREEN
    // =====================================================

    public void StartGreenDialog(NPCData data)
    {
        if (data == null)
            return;

        currentDialogType = DialogType.Green;

        dialogs = data.greenDialogs;
        currentIndex = 0;

        nameText.text = data.npcName;

        dialogPanel.SetActive(true);

        ShowCurrentDialog();
    }


    // =====================================================
    // EMERGENCY
    // =====================================================

    public void StartEmergencyDialog(NPCData data)
    {
        if (data == null)
            return;

        if (data.emergencyDialogs == null ||
            data.emergencyDialogs.Length == 0)
        {
            Debug.LogWarning(
                "NPC " +
                data.npcName +
                " ไม่มี Emergency Dialog"
            );

            return;
        }

        currentDialogType = DialogType.Emergency;

        dialogs = data.emergencyDialogs;
        currentIndex = 0;

        nameText.text = data.npcName;

        dialogPanel.SetActive(true);

        ShowCurrentDialog();
    }


    // =====================================================
    // SIMPLE
    // =====================================================

    public void StartSimpleDialog(
        string speaker,
        string[] messages)
    {
        if (messages == null ||
            messages.Length == 0)
            return;

        currentDialogType = DialogType.Simple;

        dialogs = messages;
        currentIndex = 0;

        nameText.text = speaker;

        dialogPanel.SetActive(true);

        ShowCurrentDialog();
    }


    // =====================================================
    // CHECKLIST
    // =====================================================

    // ใช้สำหรับคำถาม Checklist
    // ไม่ต้องมี answer
    public void StartChecklistDialog(
        string speaker,
        string question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            Debug.LogWarning(
                "Checklist ไม่มีข้อความ"
            );

            return;
        }

        currentDialogType = DialogType.Checklist;

        dialogs = new string[]
        {
            question
        };

        currentIndex = 0;

        nameText.text = speaker;

        dialogPanel.SetActive(true);

        ShowCurrentDialog();
    }


    // =====================================================
    // SHOW CURRENT
    // =====================================================

    private void ShowCurrentDialog()
    {
        if (dialogs == null)
            return;

        if (dialogs.Length == 0)
            return;

        if (currentIndex < 0 ||
            currentIndex >= dialogs.Length)
            return;

        // หยุด Coroutine เก่าก่อน
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (autoNextCoroutine != null)
        {
            StopCoroutine(autoNextCoroutine);
            autoNextCoroutine = null;
        }

        isTyping = true;

        // ตรวจว่าข้อความนี้เป็นข้อความแรกหรือไม่
        bool isFirst = IsFirstDialog();

        // ตรวจว่าข้อความนี้เป็นข้อความสุดท้ายหรือไม่
        bool isLast = IsLastDialog();

        // ส่งข้อมูลไปใช้กับระบบเสียง / Animation
        OnDialogStarted(isFirst, isLast);

        // เริ่ม Typewriter
        typewriterCoroutine = StartCoroutine(
            TypeText(dialogs[currentIndex])
        );
    }


    // =====================================================
    // TYPEWRITER EFFECT
    // =====================================================

    private IEnumerator TypeText(string text)
    {
        dialogText.text = "";

        foreach (char letter in text)
        {
            dialogText.text += letter;

            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        typewriterCoroutine = null;

        // ข้อความพิมพ์ครบแล้ว
        OnDialogFinishedTyping();

        // รอ 2 วินาที แล้วไปข้อความถัดไป
        autoNextCoroutine = StartCoroutine(
            AutoNextDialog()
        );
    }


    // =====================================================
    // SKIP
    // =====================================================

    public void SkipDialog()
    {
        // ถ้ากำลัง Typewriter
        // ให้ข้ามทันทีและแสดงข้อความทั้งหมด
        if (isTyping)
        {
            SkipTypewriter();
            return;
        }
    }


    // =====================================================
    // SKIP TYPEWRITER
    // =====================================================

    private void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (dialogs != null &&
            currentIndex >= 0 &&
            currentIndex < dialogs.Length)
        {
            dialogText.text = dialogs[currentIndex];
        }

        isTyping = false;

        // ข้อความถูกแสดงครบแล้ว
        OnDialogFinishedTyping();

        // เริ่มนับ 2 วินาที
        if (autoNextCoroutine != null)
        {
            StopCoroutine(autoNextCoroutine);
        }

        autoNextCoroutine = StartCoroutine(
            AutoNextDialog()
        );
    }


    // =====================================================
    // AUTO NEXT
    // =====================================================

    private IEnumerator AutoNextDialog()
    {
        yield return new WaitForSeconds(autoNextDelay);

        currentIndex++;

        if (currentIndex >= dialogs.Length)
        {
            EndDialog();
            yield break;
        }

        ShowCurrentDialog();
    }


    // =====================================================
    // DIALOG START EVENT
    // =====================================================

    private void OnDialogStarted(
        bool isFirst,
        bool isLast)
    {
        Debug.Log(
            "Dialog Started | " +
            "First: " + isFirst +
            " | Last: " + isLast
        );

        // =================================================
        // เริ่มเสียงพูด
        // =================================================

        StartVoice();

        // =================================================
        // เริ่ม Animation ปาก
        // =================================================

        if (currentMouth != null)
        {
            currentMouth.StartTalking();
        }

        // =================================================
        // อนาคตสามารถใส่ระบบอื่นตรงนี้
        // =================================================
        //
        // if (isFirst)
        // {
        //     // เริ่มเสียงเฉพาะตอนข้อความแรก
        // }
        //
        // if (isLast)
        // {
        //     // เตรียม Animation ตอนข้อความสุดท้าย
        // }
    }


    // =====================================================
    // DIALOG FINISHED TYPING EVENT
    // =====================================================

    private void OnDialogFinishedTyping()
    {
        bool isFirst = IsFirstDialog();
        bool isLast = IsLastDialog();

        Debug.Log(
            "Dialog Typed Complete | " +
            "First: " + isFirst +
            " | Last: " + isLast
        );

        // =================================================
        // หยุดเสียง
        // =================================================

        StopVoice();

        // =================================================
        // หยุด Animation ปาก
        // =================================================

        if (currentMouth != null)
        {
            currentMouth.StopTalking();
        }

        // =================================================
        // อนาคตสามารถใส่ระบบปากตรงนี้
        // =================================================
    }


    // =====================================================
    // END
    // =====================================================

    private void EndDialog()
    {
        // หยุดเสียง
        StopVoice();

        // หยุด Animation ปาก
        StopTalkingNPC();

        dialogPanel.SetActive(false);

        switch (currentDialogType)
        {
            case DialogType.Normal:

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DialogFinished();
                }

                break;


            case DialogType.Green:

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GreenDialogFinished();
                }

                break;


            case DialogType.Emergency:

                // Emergency จบ
                break;


            case DialogType.Simple:

                // Simple จบ
                break;


            case DialogType.Checklist:

                // บอก ChecklistManager ว่า
                // Dialog ข้อนี้จบแล้ว
                if (ChecklistManager.Instance != null)
                {
                    ChecklistManager.Instance
                        .ChecklistDialogFinished();
                }

                break;
        }
    }


    // =====================================================
    // CHECK
    // =====================================================

    public bool IsDialogOpen()
    {
        return dialogPanel != null &&
               dialogPanel.activeSelf;
    }


    // =====================================================
    // PLAY VOICE
    // =====================================================

    private void StartVoice()
    {
        if (voiceAudioSource == null)
            return;

        if (voiceClip == null)
            return;

        voiceAudioSource.clip = voiceClip;
        voiceAudioSource.volume = voiceVolume;

        // ให้เสียงวนระหว่างที่ Dialog กำลังแสดง
        voiceAudioSource.loop = true;

        voiceAudioSource.Play();
    }


    // =====================================================
    // STOP VOICE
    // =====================================================

    private void StopVoice()
    {
        if (voiceAudioSource == null)
            return;

        voiceAudioSource.Stop();
    }
}