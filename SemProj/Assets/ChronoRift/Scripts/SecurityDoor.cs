using UnityEngine;

public class SecurityDoor : MonoBehaviour
{
    private static SecurityDoor instance;
    public static SecurityDoor Instance
    {
        get
        {
            if (instance == null) instance = FindAnyObjectByType<SecurityDoor>();
            return instance;
        }
        private set => instance = value;
    }

    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private SpriteRenderer doorRenderer;
    [SerializeField] private float slideHeight = 3.5f;
    [SerializeField] private float openSpeed = 2.5f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        Instance = this;
        closedPosition = transform.position;
        openPosition = closedPosition + new Vector3(0f, slideHeight, 0f);

        if (doorCollider == null) doorCollider = GetComponent<Collider2D>();
        if (doorRenderer == null) doorRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Update()
    {
        if (isOpen)
        {
            transform.position = Vector3.MoveTowards(transform.position, openPosition, openSpeed * Time.deltaTime);
            if (doorRenderer != null && doorRenderer.color.a > 0.2f)
            {
                Color c = doorRenderer.color;
                c.a = Mathf.MoveTowards(c.a, 0.2f, Time.deltaTime * 2f);
                doorRenderer.color = c;
            }
        }
    }

    public void OpenDoor()
    {
        isOpen = true;
        if (doorCollider == null) doorCollider = GetComponent<Collider2D>();
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayGateOpen();
        }
    }

    public void ResetDoor()
    {
        isOpen = false;
        if (closedPosition == Vector3.zero) closedPosition = transform.position;
        transform.position = closedPosition;
        if (doorCollider == null) doorCollider = GetComponent<Collider2D>();
        if (doorCollider != null)
        {
            doorCollider.enabled = true;
        }
        if (doorRenderer == null) doorRenderer = GetComponent<SpriteRenderer>();
        if (doorRenderer != null)
        {
            Color c = doorRenderer.color;
            c.a = 1f;
            doorRenderer.color = c;
        }
    }
}
