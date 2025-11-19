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

    private static readonly int IsGroundedParam = Animator.StringToHash("IsGrounded");

    private void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        col  = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();

        rb.gravityScale   = 0f;   // player doesn’t move
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        // NO MOVEMENT – click action only
        rb.linearVelocity = Vector2.zero;

        bool grounded = IsGrounded();
        anim.SetBool(IsGroundedParam, grounded);
    }

    private bool IsGrounded()
    {
        if (col == null) return false;

        Bounds b = col.bounds;
        Vector2 feetPos = new Vector2(b.center.x, b.min.y - groundRayExtraHeight);

        // ~0 = everything, ignore our own collider manually
        RaycastHit2D hit = Physics2D.Raycast(feetPos, Vector2.down, groundRayDistance, ~0);
        return hit.collider != null && hit.collider != col;
    }
}
