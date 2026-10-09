using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class WorldButton : MonoBehaviour
{
    [Header("Animation")]
    public float hoverScale = 1.05f;
    public float pressScale = 0.95f;

    [Header("Event")]
    public UnityEvent onClick;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void OnMouseEnter()
    {
        if (InteractionLock.IsLocked)
            return;

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        transform.localScale = originalScale * hoverScale;
    }

    private void OnMouseExit()
    {
        transform.localScale = originalScale;
    }

    private void OnMouseDown()
    {
        if (InteractionLock.IsLocked)
            return;

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        transform.localScale = originalScale * pressScale;
        Debug.Log("Clicked!");
    }

    private void OnMouseUpAsButton()
    {
        if (InteractionLock.IsLocked)
            return;

        if (EventSystem.current.IsPointerOverGameObject()) 
            return;

        transform.localScale = originalScale * hoverScale;

        onClick?.Invoke();
    }
}