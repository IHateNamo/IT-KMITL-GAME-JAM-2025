using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Ground Check (for animations only)")]
    [SerializeField] private float groundRayExtraHeight = 0.05f;
    [SerializeField] private float groundRayDistance = 0.2f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;

    // Animator parameters (no Blend Tree)
    private static readonly int IsGroundedParam = Animator.StringToHash("IsGrounded");

    private void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        col  = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();

        // Static avatar – no gravity, no rotation
        rb.gravityScale   = 0f;
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        // Absolutely no movement: player stays in place
        rb.linearVelocity = Vector2.zero;

        // Optional: still know if feet touch something for animation
        bool grounded = IsGrounded();
        anim.SetBool(IsGroundedParam, grounded);
    }

    // Ground check from collider height + offset + raycast
    private bool IsGrounded()
    {
        if (col == null) return false;

        Bounds b = col.bounds;

        // feet position = bottom of collider with small extra offset down
        Vector2 feetPos = new Vector2(b.center.x, b.min.y - groundRayExtraHeight);

        // LayerMask ~0 = everything; just ignore our own collider
        RaycastHit2D hit = Physics2D.Raycast(feetPos, Vector2.down, groundRayDistance, ~0);

        return hit.collider != null && hit.collider != col;
    }
}
