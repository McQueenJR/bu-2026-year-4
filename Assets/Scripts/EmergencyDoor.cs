using UnityEngine;

public class EmergencyDoor : MonoBehaviour
{
    public Transform door;

    public Vector3 openPosition;
    public Vector3 closePosition;

    public float speed = 5f;

    private bool isClosing = false;
    private bool isOpening = false;

    void Update()
    {
        if (isClosing)
        {
            door.position = Vector3.MoveTowards(
                door.position,
                closePosition,
                speed * Time.deltaTime);

            if (Vector3.Distance(door.position, closePosition) < 0.01f)
            {
                isClosing = false;
            }
        }

        if (isOpening)
        {
            door.position = Vector3.MoveTowards(
                door.position,
                openPosition,
                speed * Time.deltaTime);

            if (Vector3.Distance(door.position, openPosition) < 0.01f)
            {
                isOpening = false;
            }
        }
    }

    public void CloseDoor()
    {
        isClosing = true;
        isOpening = false;
    }

    public void OpenDoor()
    {
        isOpening = true;
        isClosing = false;
    }
}