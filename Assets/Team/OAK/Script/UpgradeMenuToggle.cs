using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelToggle : MonoBehaviour
{
    [Header("UI Panel with CanvasGroup")]
    public GameObject upgradePanel; // ← ลาก UI Panel (not Canvas!)
    
    [Header("Optional")]
    public GameObject openButton;
    public Button closeButton;
    
    private CanvasGroup canvasGroup;
    
    private void Awake()
    {
        if (openButton == null)
            openButton = gameObject;
        
        if (upgradePanel == null)
        {
            Debug.LogError("❌ UpgradePanel is NULL! Drag the UI Panel here!");
            return;
        }
        
        // Get or Add CanvasGroup to the UI Panel
        canvasGroup = upgradePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = upgradePanel.AddComponent<CanvasGroup>();
            Debug.Log("✅ Added CanvasGroup to UI Panel");
        }
        
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseUpgradePanel);
    }
    
    private void Start()
    {
        CloseUpgradePanel();
    }
    
    public void OpenUpgradePanel()
    {
        Debug.Log("🔵 Opening Panel...");
        
        if (canvasGroup == null)
        {
            Debug.LogError("❌ CanvasGroup is NULL!");
            return;
        }
        
        // Show the panel
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        Debug.Log($"✅ Panel Opened! Alpha = {canvasGroup.alpha}");
        
        // Hide open button
        if (openButton != null)
            openButton.SetActive(false);
    }
    
    public void CloseUpgradePanel()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        if (openButton != null)
            openButton.SetActive(true);
    }
}
