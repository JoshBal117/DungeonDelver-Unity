using UnityEngine;

public class SlimeChaseAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float stopDistance = 0.5f; // how close before it stops

    private Transform target;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Find the player by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("SlimeChaseAI: No object with tag 'Player' found in the scene.");
        }
    }

    void FixedUpdate()
    {
        if (target == null || rb == null) return;

        // Direction toward the player
        Vector2 direction = (target.position - transform.position).normalized;

        // Stop if we're close enough
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= stopDistance)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
    }
}
