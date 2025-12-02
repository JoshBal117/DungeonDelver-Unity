using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SideScrollerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 4f;          // give some nice weight
        rb.freezeRotation = true;      // no spinning
    }

    void Update()
{
    // Horizontal input (A/D or Left/Right)
    moveInput = Input.GetAxisRaw("Horizontal");

    if (Input.GetButtonDown("Jump"))
    {
       // Debug.Log("Jump button pressed!");
    }

   if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
{
    rb.velocity = new Vector2(rb.velocity.x, jumpForce);
}
}

    void FixedUpdate()
{
    // Move left/right
    rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

    // Ground check
    isGrounded = Physics2D.OverlapCircle(
        groundCheck.position,
        groundCheckRadius,
        groundLayer
    );

    //Debug.Log("isGrounded: " + isGrounded);
}

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
