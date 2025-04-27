using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public string id;
    public string displayName;
    public int price;
    public Sprite icon;
    public GameObject backgroundPrefab;
    public bool isUnlockedByDefault;

    [HideInInspector] public bool isPurchased;
    [HideInInspector] public bool isSelected;

    public enum ItemState
    {
        Purchased,   // Куплен (доступен для выбора)
        Selected,    // Выбран (текущий выбранный фон)
        Purchasable, // Возможен к покупке (достаточно денег)
        Unavailable  // Недоступен к покупке (недостаточно денег)
    }

    public ItemState GetState(int playerCoins)
    {
        if (isSelected) return ItemState.Selected;
        if (isPurchased) return ItemState.Purchased;
        return playerCoins >= price ? ItemState.Purchasable : ItemState.Unavailable;
    }
}
