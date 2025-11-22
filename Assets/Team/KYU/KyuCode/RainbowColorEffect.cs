using UnityEngine;
using UnityEngine.UI; // จำเป็นต้องมีบรรทัดนี้เพื่อใช้งาน component เกี่ยวกับ UI

public class RainbowColorEffect : MonoBehaviour
{
    [Header("การตั้งค่า")]
    [Tooltip("ลาก component Image ที่ต้องการเปลี่ยนสีมาใส่ที่นี่ (ถ้าว่างไว้จะหาจาก GameObject นี้)")]
    public Image targetImage;

    [Tooltip("ความเร็วในการวนลูปสี (ค่ายิ่งมาก ยิ่งเปลี่ยนเร็ว)")]
    [Range(0.1f, 10f)] // สร้าง slider ให้ปรับค่าได้ง่ายๆ ใน Inspector
    public float cycleSpeed = 1.0f;

    [Tooltip("ความสดของสี (0 = ขาวดำ, 1 = สดที่สุด)")]
    [Range(0f, 1f)]
    public float saturation = 1.0f;

    [Tooltip("ความสว่างของสี (0 = มืดสุด, 1 = สว่างสุด)")]
    [Range(0f, 1f)]
    public float brightness = 1.0f;

    private void Start()
    {
        // ถ้ายังไม่ได้กำหนด targetImage ให้ลองหาจาก GameObject ที่ script นี้ติดอยู่
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        // ถ้ายังหาไม่เจออีก ให้แจ้งเตือน
        if (targetImage == null)
        {
            Debug.LogWarning("ไม่พบ component 'Image' กรุณาลากมาใส่ หรือติด script นี้ไว้กับ GameObject ที่มี Image");
        }
    }

    private void Update()
    {
        // ถ้าไม่มี Image ให้ทำงาน ก็ออกจากฟังก์ชันไปเลย
        if (targetImage == null) return;

        // คำนวณค่า Hue (เฉดสี) โดยอิงจากเวลา
        // Mathf.Repeat(Time.time * speed, 1.0f) จะให้ค่าที่วนลูปจาก 0.0 ถึง 1.0 เสมอ
        float hue = Mathf.Repeat(Time.time * cycleSpeed, 1.0f);

        // ใช้ Color.HSVToRGB เพื่อสร้างสีรุ้ง
        // H = Hue (เฉดสีที่เปลี่ยนไปเรื่อยๆ)
        // S = Saturation (ความสดของสี)
        // V = Value (ความสว่างของสี)
        Color rainbowColor = Color.HSVToRGB(hue, saturation, brightness);

        // นำสีที่ได้ไปกำหนดให้กับ component Image
        targetImage.color = rainbowColor;
    }
}