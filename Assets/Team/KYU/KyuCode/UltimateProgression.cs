using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; 

public class UltimateProgression : MonoBehaviour
{
    [Header("Progression")]
    public int currentClicks = 0;
    public int clicksToUlt = 100;

    [Header("UI References")]
    public Slider ultSlider;
    public TextMeshProUGUI ultText;

    [Header("Ultimate Animation UI")]
    public Animator ultUIAnimator; 
    private int showUltTriggerHash;
    private int hideUltTriggerHash; // เพิ่มตัวแปรสำหรับซ่อน/กลับสู่สถานะ Idle

    [Header("Links")]
    public UltimateSkill ultimateSkill;

    // ตัวแปรสำหรับตรวจจับการเปลี่ยนสถานะ
    private bool wasActive = false; 

    public bool IsUltimateActive
    {
        get { return ultimateSkill != null && ultimateSkill.isActiveAndEnabled; }
    }

    private void Start()
    {
        UpdateUI();
        if (ultUIAnimator != null)
        {
            // ควรตั้ง Trigger แยกกันสำหรับ เปิด/ปิด หรือใช้ Trigger เดียวตามการตั้งค่า Animator
            showUltTriggerHash = Animator.StringToHash("ShowUlt"); 
            // *สมมติว่าคุณมี Trigger ชื่อ HideUlt ด้วย หรือใช้ ShowUlt ในการกลับสถานะ*
            hideUltTriggerHash = Animator.StringToHash("HideUlt"); 
        }
    }

    private void Update()
    {
        bool nowActive = IsUltimateActive;

        if (nowActive)
        {
            // 1. ถ้าอัลติกำลังทำงาน: นับถอยหลัง (Text และ Slider)
            if (ultimateSkill != null && ultSlider != null)
            {
                float remaining = ultimateSkill.RemainingTime;
                float duration = ultimateSkill.ultDuration; // ดึงระยะเวลาทั้งหมดจาก UltimateSkill

                // A. อัปเดต Slider: ตั้งค่า Max Value เป็นระยะเวลาทั้งหมด
                ultSlider.maxValue = duration;
                ultSlider.value = remaining;

                // B. อัปเดต Text: โชว์เวลาที่เหลือ
                ultText.text = $"ULT: {Mathf.CeilToInt(remaining)}s";
            }
        }
        
        // 2. ตรวจจับการจบอัลติ (Transition จาก Active -> Inactive)
        if (wasActive && !nowActive)
        {
            // อัลติจบแล้ว:
            
            // A. สั่งให้ Animation กลับไป Idle
            if (ultUIAnimator != null)
            {
                // ใช้ Trigger ที่สั่งให้ UI กลับไปซ่อน (ตามการตั้งค่า Animator)
                ultUIAnimator.SetTrigger(hideUltTriggerHash); 
            }
            
            // B. รีเซ็ต Slider/Text ให้กลับไปโชว์ 0/100
            ultSlider.maxValue = clicksToUlt; // คืนค่า Max Value ให้เป็นจำนวนคลิก
            UpdateUI();
        }

        // 3. อัปเดตสถานะ
        wasActive = nowActive;
    }

    // --- ส่วนการนับคลิก (เหมือนเดิม) ---
    public void RegisterClick()
    {
        if (IsUltimateActive) return;

        currentClicks++;

        if (currentClicks >= clicksToUlt)
        {
            Debug.Log("⚡ ULT READY & ACTIVATED! ⚡");
            ActivateUltimate();
            currentClicks = 0;
        }

        UpdateUI();
    }

    private void ActivateUltimate()
    {
        if (ultimateSkill != null)
        {
            ultimateSkill.StartUltimateDuration();
            
            // สั่งให้ Animation เล่น (โชว์ UI)
            if (ultUIAnimator != null)
            {
                // ใช้ Trigger เพื่อเริ่ม Animation โชว์ตัว UI
                ultUIAnimator.SetTrigger(showUltTriggerHash); 
            }
        }
    }

    // ฟังก์ชันจัดการ Slider และ Text (ใช้ตอนเก็บเกจเท่านั้น)
    private void UpdateUI()
    {
        if (ultSlider != null)
        {
            // ใช้ค่า clicksToUlt ในการเก็บเกจ
            ultSlider.maxValue = clicksToUlt;
            ultSlider.value = currentClicks;
        }

        // โชว์ 0/100 ตอนเก็บเกจ
        if (ultText != null)
        {
            ultText.text = $"{currentClicks} / {clicksToUlt}";
        }
    }
}