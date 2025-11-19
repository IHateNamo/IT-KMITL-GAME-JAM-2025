using UnityEngine;
using System.Collections.Generic;

public class UltimateSkill : MonoBehaviour
{
    public float sampleDistance = 0.05f; 
    private Vector3 lastPos;
    private bool isDragging = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastPos = GetMouseWorld();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 cur = GetMouseWorld();

            float dist = Vector3.Distance(lastPos, cur);
            if (dist > 0f)
            {
                // แบ่งเป็นช่วง ๆ เพื่อเช็ค linecast ระหว่างจุด
                int steps = Mathf.CeilToInt(dist / sampleDistance);
                for (int i = 0; i < steps; i++)
                {
                    Vector3 a = Vector3.Lerp(lastPos, cur, (float)i / steps);
                    Vector3 b = Vector3.Lerp(lastPos, cur, (float)(i + 1) / steps);

                    RaycastHit2D hit = Physics2D.Linecast(a, b);
                    if (hit.collider != null && hit.collider.CompareTag("Enemy"))
                    {
                        Debug.Log("Hit enemy: " + hit.collider.name);
                }

                lastPos = cur;
            }
        }}}

    private Vector3 GetMouseWorld()
    {
        Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        p.z = 0;
        return p;
    }}
