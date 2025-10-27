using UnityEngine;

public enum ItemType
{
    None,
    Ore, Ingot, Sword,
    Log, Plank, Bow
}

public class Item : MonoBehaviour
{
    public ItemType type;
}
