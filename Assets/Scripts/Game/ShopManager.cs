using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Items")]
    [SerializeField] private List<ShopItem> shopItems = new List<ShopItem>();

    [Header("Shop UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Image backgroundPreview;
    [SerializeField] private List<TextMeshProUGUI> itemNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject priceContainer;
    [SerializeField] private List<Button> actionButton;
    [SerializeField] private List<GameObject> statusContainer;

    [Header("Shop Animation")]
    [SerializeField] private float fadeTime = 0.3f;
    [SerializeField] private float scaleTime = 0.5f;

    [Header("Storage Keys")]
    [SerializeField] private string selectedItemIdKey = "SelectedBackgroundId";
    [SerializeField] private string purchasedItemsKey = "PurchasedBackgrounds";

    private int currentItemIndex = 0;
    private GameManager gameManager;
    private BackgroundManager backgroundManager;
    private AudioManager audioManager;

    public event Action<ShopItem> OnItemPurchased;
    public event Action<ShopItem> OnItemSelected;
    public event Action<int> OnCoinsUpdated;

    private void Awake()
    {
        // Initialize UI references
        if (shopPanel == null) Debug.LogError("Shop panel reference is missing");

        // Set up button listeners
        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
        if (leftArrowButton != null) leftArrowButton.onClick.AddListener(NavigatePrevious);
        if (rightArrowButton != null) rightArrowButton.onClick.AddListener(NavigateNext);
        if (actionButton != null)
        {
            foreach (var item in actionButton)
            {
                item.onClick.AddListener(OnActionButtonClicked);
            }
        }

        // Load purchased items
        LoadPurchasedItems();
    }

    private void Start()
    {
        // Find required managers
        gameManager = FindObjectOfType<GameManager>();
        backgroundManager = FindObjectOfType<BackgroundManager>();
        audioManager = FindObjectOfType<AudioManager>();

        if (gameManager == null || backgroundManager == null)
        {
            Debug.LogError("ShopManager requires GameManager and BackgroundManager in the scene!");
            return;
        }

        // Set initial selected background
        string selectedId = PlayerPrefs.GetString(selectedItemIdKey, string.Empty);
        if (!string.IsNullOrEmpty(selectedId))
        {
            int index = shopItems.FindIndex(i => i.id == selectedId);
            if (index >= 0 && shopItems[index].isPurchased)
            {
                SelectItem(index);
            }
            else
            {
                // Default to first item if saved selection is invalid
                SelectItem(0);
            }
        }
        else
        {
            // Default to first item
            SelectItem(0);
        }

        // Initially hide shop panel
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    public void OpenShop()
    {
        if (shopPanel == null)
            return;

        // Show shop panel
        shopPanel.SetActive(true);

        // Update coins display
        UpdateCoinsText();

        // Refresh UI for current item
        RefreshCurrentItemUI();

        // Animate panel appearance
        shopPanel.transform.localScale = Vector3.zero;
        shopPanel.transform.DOScale(Vector3.one, scaleTime).SetEase(Ease.OutBack).SetUpdate(true);

        CanvasGroup canvasGroup = shopPanel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, fadeTime).SetUpdate(true);
        }

        // Play sound effect
        if (audioManager != null)
        {
            audioManager.PlaySound("MenuClick");
        }

        Debug.Log("Shop opened");
    }

    public void CloseShop()
    {
        if (shopPanel == null)
            return;

        // Animate panel disappearance
        shopPanel.transform.DOScale(Vector3.zero, scaleTime).SetEase(Ease.InBack).SetUpdate(true)
            .OnComplete(() => {
                shopPanel.SetActive(false);

                // Resume game if it was running
                if (gameManager != null && gameManager.IsGameRunning)
                {
                    Time.timeScale = 1f;
                }
            });

        CanvasGroup canvasGroup = shopPanel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0, fadeTime).SetUpdate(true);
        }

        // Play sound effect
        if (audioManager != null)
        {
            audioManager.PlaySound("MenuClick");
        }

        Debug.Log("Shop closed");
    }

    private void NavigatePrevious()
    {
        if (shopItems.Count == 0)
            return;

        currentItemIndex--;
        if (currentItemIndex < 0)
            currentItemIndex = shopItems.Count - 1;

        RefreshCurrentItemUI();

        // Play sound effect
        if (audioManager != null)
        {
            audioManager.PlaySound("MenuClick");
        }
    }

    private void NavigateNext()
    {
        if (shopItems.Count == 0)
            return;

        currentItemIndex++;
        if (currentItemIndex >= shopItems.Count)
            currentItemIndex = 0;

        RefreshCurrentItemUI();

        // Play sound effect
        if (audioManager != null)
        {
            audioManager.PlaySound("MenuClick");
        }
    }

    private void OnActionButtonClicked()
    {
        if (shopItems.Count == 0 || currentItemIndex < 0 || currentItemIndex >= shopItems.Count)
            return;

        ShopItem currentItem = shopItems[currentItemIndex];

        // Play sound effect
        if (audioManager != null)
        {
            audioManager.PlaySound("ButtonClick");
        }

        if (currentItem.isPurchased)
        {
            // Item already purchased, select it
            SelectItem(currentItemIndex);
        }
        else
        {
            // Try to purchase the item
            bool purchased = PurchaseItem(currentItemIndex);
            if (purchased)
            {
                // Automatically select after purchase
                SelectItem(currentItemIndex);

                // Play purchase animation
                if (backgroundPreview != null)
                {
                    backgroundPreview.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
                }
            }
        }
    }

    private bool PurchaseItem(int index)
    {
        if (index < 0 || index >= shopItems.Count)
            return false;

        ShopItem item = shopItems[index];
        if (item.isPurchased)
            return false;

        int currentCoins = gameManager.GetTotalCoins();
        if (currentCoins < item.price)
            return false;

        // Deduct coins
        gameManager.RemoveTotalCoins(item.price);

        // Mark as purchased
        item.isPurchased = true;
        SavePurchasedItems();

        // Update UI
        UpdateCoinsText();
        RefreshCurrentItemUI();

        // Notify listeners
        OnItemPurchased?.Invoke(item);
        OnCoinsUpdated?.Invoke(gameManager.GetTotalCoins());

        Debug.Log($"Item purchased: {item.displayName} for {item.price} coins");
        return true;
    }

    private void SelectItem(int index)
    {
        if (index < 0 || index >= shopItems.Count)
            return;

        ShopItem item = shopItems[index];
        if (!item.isPurchased)
            return;

        // Deselect currently selected item
        foreach (var shopItem in shopItems)
        {
            shopItem.isSelected = false;
        }

        // Select new item
        item.isSelected = true;
        PlayerPrefs.SetString(selectedItemIdKey, item.id);
        PlayerPrefs.Save();

        // Update the background manager with the selected background prefab
        if (backgroundManager != null)
        {
            backgroundManager.SetActivePrefab(item.backgroundPrefab);
        }

        // Update UI
        RefreshCurrentItemUI();

        // Notify listeners
        OnItemSelected?.Invoke(item);

        Debug.Log($"Item selected: {item.displayName}");
    }

    private void RefreshCurrentItemUI()
    {
        if (shopItems.Count == 0 || currentItemIndex < 0 || currentItemIndex >= shopItems.Count)
            return;

        ShopItem currentItem = shopItems[currentItemIndex];

        // Update background preview
        if (backgroundPreview != null && currentItem.icon != null)
        {
            backgroundPreview.sprite = currentItem.icon;
        }

        // Update item name
        if (itemNameText != null)
        {
            foreach (var text in itemNameText)
            {
                text.text = currentItem.displayName;
            }

        }

        // Get current state
        ShopItem.ItemState itemState = currentItem.GetState(gameManager.GetTotalCoins());

        // Update UI based on state
        if (actionButton != null)
        {
            foreach (var item in statusContainer)
            {
                item.SetActive(false);
            }
            switch (itemState)
            {
                case ShopItem.ItemState.Selected:
                    statusContainer[0].SetActive(true);
                    if (priceContainer != null) priceContainer.SetActive(false);
                    break;

                case ShopItem.ItemState.Purchased:
                    statusContainer[3].SetActive(true);
                    if (priceContainer != null) priceContainer.SetActive(false);
                    break;

                case ShopItem.ItemState.Purchasable:
                    statusContainer[1].SetActive(true);
                    if (priceContainer != null)
                    {
                        priceContainer.SetActive(true);
                        if (priceText != null) priceText.text = currentItem.price.ToString();
                    }
                    break;

                case ShopItem.ItemState.Unavailable:
                    statusContainer[2].SetActive(true);
                    if (priceContainer != null)
                    {
                        priceContainer.SetActive(true);
                        if (priceText != null) priceText.text = currentItem.price.ToString();
                    }
                    break;
            }
        }
    }

    private void UpdateCoinsText()
    {
        if (coinsText != null && gameManager != null)
        {
            coinsText.text = gameManager.GetTotalCoins().ToString();
        }
    }

    private void LoadPurchasedItems()
    {
        // Initialize default states
        foreach (var item in shopItems)
        {
            item.isPurchased = item.isUnlockedByDefault;
            item.isSelected = false;
        }

        // Load purchased items from PlayerPrefs
        string purchasedItemsData = PlayerPrefs.GetString(purchasedItemsKey, "");
        if (!string.IsNullOrEmpty(purchasedItemsData))
        {
            string[] purchasedIds = purchasedItemsData.Split(',');
            foreach (var id in purchasedIds)
            {
                ShopItem item = shopItems.Find(i => i.id == id);
                if (item != null)
                {
                    item.isPurchased = true;
                }
            }
        }

        // Load selected item
        string selectedId = PlayerPrefs.GetString(selectedItemIdKey, "");
        if (!string.IsNullOrEmpty(selectedId))
        {
            ShopItem selectedItem = shopItems.Find(i => i.id == selectedId);
            if (selectedItem != null && selectedItem.isPurchased)
            {
                selectedItem.isSelected = true;

                // Find index of selected item
                int index = shopItems.FindIndex(i => i.id == selectedId);
                if (index >= 0)
                {
                    currentItemIndex = index;
                }
            }
        }

        // If nothing is selected, select the first unlocked item
        if (!shopItems.Exists(i => i.isSelected) && shopItems.Count > 0)
        {
            for (int i = 0; i < shopItems.Count; i++)
            {
                if (shopItems[i].isPurchased)
                {
                    shopItems[i].isSelected = true;
                    currentItemIndex = i;
                    break;
                }
            }
        }
    }

    private void SavePurchasedItems()
    {
        List<string> purchasedIds = new List<string>();
        foreach (var item in shopItems)
        {
            if (item.isPurchased)
            {
                purchasedIds.Add(item.id);
            }
        }

        string purchasedItemsData = string.Join(",", purchasedIds);
        PlayerPrefs.SetString(purchasedItemsKey, purchasedItemsData);
        PlayerPrefs.Save();
    }
}