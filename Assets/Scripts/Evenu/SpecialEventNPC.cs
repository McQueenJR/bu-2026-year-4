using UnityEngine;

public class SpecialEventNPC : MonoBehaviour
{
    public SpecialEventManager.SpecialEventConfig eventConfig;

    public bool IsSpecialEvent => eventConfig != null;
}