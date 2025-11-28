using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelToggle : MonoBehaviour
{
    [Header("References")]
    public GameObject upgradePanel;
    
    private CanvasGroup canvasGroup;
    private Toggle toggle;
    
    private void Awake()
    {
        // Get Toggle component
        toggle = GetComponent<Toggle>();
        
        if (toggle == null)
        {
            Debug.LogError("❌ Toggle component not found! This script must be on a Toggle GameObject!");
            return;
        }
        
        // Setup CanvasGroup
        if (upgradePanel != null)
        {
            canvasGroup = upgradePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = upgradePanel.AddComponent<CanvasGroup>();
            }
        }
        else
        {
            Debug.LogError("❌ UpgradePanel is null! Drag UpgradePanel into Inspector!");
        }
        
        // Listen to toggle changes
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }
    
    private void Start()
    {
        // Set initial state (closed)
        toggle.isOn = false;
        UpdatePanelVisibility(false);
    }
    
    private void OnToggleChanged(bool isOn)
    {
        UpdatePanelVisibility(isOn);
    }
    
    private void UpdatePanelVisibility(bool show)
    {
        if (canvasGroup == null) return;
        
        if (show)
        {
            // Show panel
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            Debug.Log("✅ Panel Opened");
        }
        else
        {
            // Hide panel
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            Debug.Log("✅ Panel Closed");
        }
    }
}
