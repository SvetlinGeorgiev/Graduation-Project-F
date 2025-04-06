using UnityEngine;

public enum ItemType
{
    HealthPotion,
}

[System.Serializable]
public class Item
{
    public string itemName;
    public ItemType itemType;
    public int amount;

    public void Use(GameObject player)
    {
        switch (itemType)
        {
            case ItemType.HealthPotion:
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.RestoreHealth(amount);
                }
                break;
        }
    }
}
