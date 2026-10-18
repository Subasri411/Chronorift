using UnityEngine;

public class DragonEgg : MonoBehaviour
{
    [SerializeField] private int eggId = 0;
    [SerializeField] private string eggName = "Sacred Dragon Egg";
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private float bobDistance = 0.2f;

    private Vector3 initialPosition;
    private bool isCollected = false;
    private SpriteRenderer sr;
    private Collider2D col;

    public int EggId => eggId;
    public string EggName => eggName;
    public bool IsCollected => isCollected;

    private void Awake()
    {
        initialPosition = transform.position;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        if (isCollected) return;

        // Smooth floating motion
        float yOffset = Mathf.Sin(Time.time * bobSpeed + eggId * 1.5f) * bobDistance;
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
            GameManager.Instance.CollectEgg(eggId, eggName);
        }

        // Hide visuals and disable collider
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (col == null) col = GetComponent<Collider2D>();
        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;
    }

    public void ResetEgg()
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
