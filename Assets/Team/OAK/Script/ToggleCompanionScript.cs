using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompanionToggleManager : MonoBehaviour
{
    [Header("Companion")]
    public Companion companion;
    public GameObject companionGameObject;
    
    [Header("Purchase Settings")]
    public float buyCost = 500f;
    public bool isPurchased = false;
    
    [Header("UI")]
    public Button buyButton;
    public TextMeshProUGUI buttonText;
    
    [Header("Player Resources")]
    public UpgradeUI upgradeUI;
    
    [Header("Debug")]
    public bool showDebugLog = true;
    
    private void Start()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyButtonClick);
        
        // Initial state: hide companion
        if (companionGameObject != null)
            companionGameObject.SetActive(false);
        
        if (companion != null)
            companion.SetActive(false);
        
        UpdateUI();
    }
    
    private void Update()
    {
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (upgradeUI == null) return;
        
        if (!isPurchased)
        {
            // Show buy option
            if (buttonText != null)
                buttonText.text = $"Buy Companion\n{buyCost} Gold";
            
            if (buyButton != null)
                buyButton.interactable = (upgradeUI.playerGold >= buyCost);
        }
        else
        {
            // Already purchased - hide or disable button
            if (buttonText != null)
                buttonText.text = "Companion Active";
            
            if (buyButton != null)
                buyButton.interactable = false; // Disable button after purchase
        }
    }
    
    public void OnBuyButtonClick()
    {
        if (isPurchased)
        {
            if (showDebugLog)
                Debug.Log("Companion already purchased!");
            return;
        }
        
        if (upgradeUI == null)
        {
            Debug.LogError("❌ UpgradeUI reference is missing!");
            return;
        }
        
        if (upgradeUI.playerGold >= buyCost)
        {
            // Deduct gold
            upgradeUI.playerGold -= buyCost;
            
            // Mark as purchased
            isPurchased = true;
            
            // Activate companion permanently
            if (companionGameObject != null)
                companionGameObject.SetActive(true);
            
            if (companion != null)
                companion.SetActive(true);
            
            if (showDebugLog)
                Debug.Log($"✅ Companion Purchased! Gold remaining: {upgradeUI.playerGold}");
            
            UpdateUI();
        }
        else
        {
            if (showDebugLog)
                Debug.Log($"❌ Not enough gold! Need {buyCost}, have {upgradeUI.playerGold}");
        }
    }
    
    // Public getter
    public bool IsPurchased() => isPurchased;
}
