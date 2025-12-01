using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Vector2 input;
    private Rigidbody2D rb;

    void Awake()
    {
        // Try to get an existing Rigidbody2D
        rb = GetComponent<Rigidbody2D>();

        // If there isn't one yet, add it
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0;      // No gravity in top-down
        rb.freezeRotation = true; // Prevent spinning
    }

    void Update()
    {
        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    void FixedUpdate()
    {
        rb.velocity = input * moveSpeed;
    }
}
