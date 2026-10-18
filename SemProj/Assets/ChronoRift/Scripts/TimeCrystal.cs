using UnityEngine;

public class TimeCrystal : MonoBehaviour
{
    private static TimeCrystal instance;
    public static TimeCrystal Instance
    {
        get
        {
            if (instance == null) instance = FindAnyObjectByType<TimeCrystal>();
            return instance;
        }
        private set => instance = value;
    }

    [SerializeField] private float bobSpeed = 2.5f;
    [SerializeField] private float bobHeight = 0.25f;
    [SerializeField] private Vector3 initialPosition;

    private bool isCollected = false;
    private SpriteRenderer sr;
    private Collider2D col;

    private void Awake()
    {
        Instance = this;
        if (initialPosition == Vector3.zero)
        {
            initialPosition = transform.position;
        }
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Start()
    {
        if (initialPosition == Vector3.zero)
        {
            initialPosition = transform.position;
        }
        ResetCrystal();
    }

    private void Update()
    {
        if (isCollected)
        {
            transform.position += Vector3.up * 1.5f * Time.deltaTime;
            return;
        }

        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = initialPosition + new Vector3(0f, yOffset, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        // Check if level objective has been fulfilled
        if (GameManager.Instance != null && !GameManager.Instance.CanCollectTimeCrystal) return;

        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            Collect();
        }
    }

    public void Collect()
    {
        if (isCollected) return;
        if (GameManager.Instance != null && !GameManager.Instance.CanCollectTimeCrystal) return;
        isCollected = true;

        if (col == null) col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayVictory();
        }

        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.TriggerShake(1.0f, 0.35f);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinGame();
        }
    }

    public void ResetCrystal()
    {
        isCollected = false;
        if (initialPosition == Vector3.zero && transform.position != Vector3.zero)
        {
            initialPosition = transform.position;
        }
        if (initialPosition != Vector3.zero)
        {
            transform.position = initialPosition;
        }
        if (col == null) col = GetComponent<Collider2D>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (col != null) col.enabled = true;
        if (sr != null) sr.enabled = true;
    }

    public void SetInitialPosition(Vector3 pos)
    {
        initialPosition = pos;
        transform.position = pos;
    }
}
