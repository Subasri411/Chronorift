using UnityEngine;

public class PowerChip : MonoBehaviour
{
    [SerializeField] private int chipId = 0;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private float bobDistance = 0.2f;

    private Vector3 initialPosition;
    private bool isCollected = false;
    private SpriteRenderer sr;
    private Collider2D col;

    public int ChipId => chipId;
    public bool IsCollected => isCollected;

    private void Awake()
    {
        initialPosition = transform.position;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (initialPosition == Vector3.zero) initialPosition = transform.position;
    }

    private void Update()
    {
        if (isCollected) return;

        float yOffset = Mathf.Sin(Time.time * bobSpeed + chipId * 1.5f) * bobDistance;
        transform.position = initialPosition + new Vector3(0f, yOffset, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            Collect();
        }
    }

    public void Collect()
    {
        if (isCollected) return;
        isCollected = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEggPickup();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectPowerChip(chipId);
        }

        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (col == null) col = GetComponent<Collider2D>();
        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;
    }

    public void ResetChip()
    {
        isCollected = false;
        if (initialPosition == Vector3.zero) initialPosition = transform.position;
        transform.position = initialPosition;
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (col == null) col = GetComponent<Collider2D>();
        if (sr != null) sr.enabled = true;
        if (col != null) col.enabled = true;
    }
}
