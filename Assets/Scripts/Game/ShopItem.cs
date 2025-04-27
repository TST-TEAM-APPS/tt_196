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
        Purchased,   // ������ (�������� ��� ������)
        Selected,    // ������ (������� ��������� ���)
        Purchasable, // �������� � ������� (���������� �����)
        Unavailable  // ���������� � ������� (������������ �����)
    }

    public ItemState GetState(int playerCoins)
    {
        if (isSelected) return ItemState.Selected;
        if (isPurchased) return ItemState.Purchased;
        return playerCoins >= price ? ItemState.Purchasable : ItemState.Unavailable;
    }
}
