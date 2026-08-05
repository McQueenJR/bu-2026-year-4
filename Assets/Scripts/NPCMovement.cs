using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public float speed = 2f;

    private Vector3 target;
    private bool moving = false;

    public void MoveTo(Vector3 destination)
    {
        target = destination;
        moving = true;
    }

    void Update()
    {
        if (!moving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            moving = false;
        }
    }

    public bool IsMoving()
    {
        return moving;
    }
}