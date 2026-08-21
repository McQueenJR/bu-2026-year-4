using UnityEngine;

public class NPCMouthAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int Talking =
        Animator.StringToHash("Talking");

    public void StartTalking()
    {
        if (animator == null)
        {
            Debug.LogError(
                "NPCMouthAnimation: ไม่ได้ใส่ Animator ให้ " +
                gameObject.name
            );
            return;
        }

        animator.SetBool(Talking, true);

        Debug.Log(
            "START TALKING : " +
            gameObject.name
        );
    }

    public void StopTalking()
    {
        if (animator == null)
            return;

        animator.SetBool(Talking, false);

        Debug.Log(
            "STOP TALKING : " +
            gameObject.name
        );
    }
}