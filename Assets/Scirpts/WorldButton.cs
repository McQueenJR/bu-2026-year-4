using UnityEngine;
using UnityEngine.Events;

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
        transform.localScale = originalScale * hoverScale;
    }

    private void OnMouseExit()
    {
        transform.localScale = originalScale;
    }

    private void OnMouseDown()
    {
        transform.localScale = originalScale * pressScale;
        Debug.Log("Clicked!");
    }

    private void OnMouseUpAsButton()
    {
        transform.localScale = originalScale * hoverScale;

        onClick?.Invoke();
    }
}