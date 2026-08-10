using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;

    public Sprite itemImage;

    [TextArea(2, 5)]
    public string description;
}