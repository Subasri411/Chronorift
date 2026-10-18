using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    private static PlayerController instance;
    public static PlayerController Instance
    {
        get
        {
            if (instance == null) instance = FindAnyObjectByType<PlayerController>();
            return instance;
        }
        private set => instance = value;
    }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7.5f;
    [SerializeField] private float jumpForce = 16.0f;
    [SerializeField] private float fallMultiplier = 1.8f;
    [SerializeField] private float lowJumpMultiplier = 1.5f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.15f);
    [SerializeField] private float groundCheckOffset = -0.45f;

    [Header("Visual Sprites")]
    public Sprite idleSprite;
    public Sprite walk1Sprite;
    public Sprite walk2Sprite;
    public Sprite jumpSprite;

    private Rigidbody2D rb;
    private BoxCollider2D boxCol;
    private SpriteRenderer sr;

    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private const float CoyoteTime = 0.15f;
    private const float JumpBufferTime = 0.15f;

    private float walkTimer;
    private bool walkToggle;
    private Vector3 spawnPoint;
    private bool inputEnabled = true;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        boxCol = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        spawnPoint = transform.position;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 3f;

        // Default ground layer to everything except Ignore Raycast and Player
        if (groundLayer.value == 0)
        {
            groundLayer = ~LayerMask.GetMask("Ignore Raycast");
        }
    }

    private void Start()
    {
        spawnPoint = transform.position;
    }

    private void Update()
    {
        if (!inputEnabled || GameManager.Instance != null && GameManager.Instance.IsGamePausedOrOver())
        {
            return;
        }

        CheckGrounded();

        // Jump buffer & Coyote time
        if (isGrounded)
        {
            coyoteTimeCounter = CoyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (ChronoInput.GetJumpDown())
        {
            jumpBufferCounter = JumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Execute Jump
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            ExecuteJump();
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        // Variable jump height
        if (ChronoInput.GetJumpUp() && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        UpdateVisuals();

        // Fall check (pit hazard)
        if (transform.position.y < -12f)
        {
            Respawn();
        }
    }

    private void FixedUpdate()
    {
        if (!inputEnabled || GameManager.Instance != null && GameManager.Instance.IsGamePausedOrOver())
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float h = ChronoInput.GetHorizontal();
        rb.linearVelocity = new Vector2(h * moveSpeed, rb.linearVelocity.y);

        // Better jump falling gravity
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = 3f * fallMultiplier;
        }
        else if (rb.linearVelocity.y > 0 && !ChronoInput.GetJumpDown())
        {
            rb.gravityScale = 3f * lowJumpMultiplier;
        }
        else
        {
            rb.gravityScale = 3f;
        }
    }

    private void CheckGrounded()
{
    Vector2 checkPos = (Vector2)transform.position
        + new Vector2(0f, groundCheckOffset);

    Collider2D[] hits = Physics2D.OverlapBoxAll(
        checkPos, groundCheckSize, 0f, groundLayer);

    isGrounded = false;

    foreach (Collider2D hit in hits)
    {
        if (hit != boxCol && !hit.isTrigger)
        {
            isGrounded = true;
            break;
        }
    }
}

    private void ExecuteJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayJump();
        }
    }

    private void UpdateVisuals()
    {
        if (sr == null) return;

        float h = ChronoInput.GetHorizontal();
        if (Mathf.Abs(h) > 0.1f)
        {
            sr.flipX = h < 0f;
        }

        if (!isGrounded)
        {
            if (jumpSprite != null) sr.sprite = jumpSprite;
        }
        else if (Mathf.Abs(h) > 0.1f)
        {
            walkTimer += Time.deltaTime;
            if (walkTimer >= 0.12f)
            {
                walkTimer = 0f;
                walkToggle = !walkToggle;
            }
            sr.sprite = (walkToggle && walk2Sprite != null) ? walk2Sprite : (walk1Sprite != null ? walk1Sprite : idleSprite);
        }
        else
        {
            walkTimer = 0f;
            if (idleSprite != null) sr.sprite = idleSprite;
        }
    }

    public void Respawn()
    {
        if (spawnPoint == Vector3.zero) spawnPoint = transform.position;
        transform.position = spawnPoint;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayDenied();
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
        }
    }

    public void SetSpawnPoint(Vector3 pos)
    {
        spawnPoint = pos;
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (!enabled && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hazard"))
        {
            Respawn();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 checkPos = transform.position + new Vector3(0f, groundCheckOffset, 0f);
        Gizmos.DrawWireCube(checkPos, new Vector3(groundCheckSize.x, groundCheckSize.y, 0f));
    }
}
