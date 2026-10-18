using UnityEngine;

public class TempleGate : MonoBehaviour
{
    private static TempleGate instance;
    public static TempleGate Instance
    {
        get
        {
            if (instance == null) instance = FindAnyObjectByType<TempleGate>();
            return instance;
        }
        private set => instance = value;
    }

    [SerializeField] private Collider2D gateCollider;
    [SerializeField] private SpriteRenderer gateRenderer;
    [SerializeField] private float slideHeight = 3.5f;
    [SerializeField] private float openSpeed = 2.0f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;

    private void Awake()
    {
        Instance = this;
        closedPosition = transform.position;
        openPosition = closedPosition + new Vector3(0f, slideHeight, 0f);

        if (gateCollider == null) gateCollider = GetComponent<Collider2D>();
        if (gateRenderer == null) gateRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (isOpen)
        {
            transform.position = Vector3.MoveTowards(transform.position, openPosition, openSpeed * Time.deltaTime);
            if (gateRenderer != null && gateRenderer.color.a > 0.2f)
            {
                Color c = gateRenderer.color;
                c.a = Mathf.MoveTowards(c.a, 0.2f, Time.deltaTime);
                gateRenderer.color = c;
            }
        }
    }

    public void OpenGate()
    {
        isOpen = true;
        if (gateCollider == null) gateCollider = GetComponent<Collider2D>();
        if (gateCollider != null)
        {
            gateCollider.enabled = false;
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayGateOpen();
        }
    }

    public void ResetGate()
    {
        isOpen = false;
        if (closedPosition == Vector3.zero) closedPosition = transform.position;
        transform.position = closedPosition;
        if (gateCollider == null) gateCollider = GetComponent<Collider2D>();
        if (gateRenderer == null) gateRenderer = GetComponent<SpriteRenderer>();
        if (gateCollider != null)
        {
            gateCollider.enabled = true;
        }
        if (gateRenderer != null)
        {
            Color c = gateRenderer.color;
            c.a = 1f;
            gateRenderer.color = c;
        }
    }
}
