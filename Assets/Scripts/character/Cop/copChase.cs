using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaseTrigger2D : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;

    [Header("Chase Settings")]
    public float chaseSpeed = 3f;
    public float stopDistance = 0.5f;
    public bool startsFacingLeft = true; // ✅ set this to true if enemy's idle/run faces left

    private Rigidbody2D rb;
    private bool isChasing = false;
    private Vector3 initialScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (isChasing)
        {
            float direction = Mathf.Sign(player.position.x - transform.position.x);

            // Move toward player
            if (Vector2.Distance(transform.position, player.position) > stopDistance)
                rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
            else
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            // ✅ Flip based on starting orientation
            if (startsFacingLeft)
            {
                // Enemy initially faces left — flip logic is inverted
                if (direction > 0)
                    transform.localScale = new Vector3(-Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
                else if (direction < 0)
                    transform.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
            }
            else
            {
                // Enemy initially faces right
                if (direction > 0)
                    transform.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
                else if (direction < 0)
                    transform.localScale = new Vector3(-Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isChasing)
        {
            isChasing = true;
            Debug.Log("Enemy started chasing (trigger)!");

            if (animator != null)
                animator.SetBool("spotsPlayer", true);
        }
    }
}
