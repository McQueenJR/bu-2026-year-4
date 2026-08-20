using UnityEngine;

public class NPCMouthAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int Talking = Animator.StringToHash("Talking");

    public void StartTalking()
    {
        if (animator == null)
            return;

        animator.SetBool(Talking, true);
    }

    public void StopTalking()
    {
        if (animator == null)
            return;

        animator.SetBool(Talking, false);
    }
}