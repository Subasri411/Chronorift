using UnityEngine;

public class TimeCore : MonoBehaviour
{
    private static TimeCore instance;
    public static TimeCore Instance
    {
        get
        {
            if (instance == null) instance = FindAnyObjectByType<TimeCore>();
            return instance;
        }
        private set => instance = value;
    }

    [SerializeField] private float pulseSpeed = 2.5f;
    [SerializeField] private float bobHeight = 0.2f;

    private Vector3 initialPosition;
    private bool isPlayerInRange = false;
    private bool isActivated = false;

    public bool IsActivated => isActivated;

    private void Awake()
    {
        Instance = this;
        initialPosition = transform.position;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Start()
    {
        if (initialPosition == Vector3.zero) initialPosition = transform.position;
        ResetCore();
    }

    private void Update()
    {
        // Gentle hovering plasma pulse
        float yOffset = Mathf.Sin(Time.time * pulseSpeed) * bobHeight;
        transform.position = initialPosition + new Vector3(0f, yOffset, 0f);

        if (isActivated) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGamePausedOrOver()) return;

        if (isPlayerInRange)
        {
            bool doorUnlocked = GameManager.Instance != null && GameManager.Instance.IsSecurityDoorUnlocked;

            if (!doorUnlocked)
            {
                if (UIManager.Instance != null)
                {
                    int chips = GameManager.Instance != null ? GameManager.Instance.PowerChipsCount : 0;
                    UIManager.Instance.ShowPrompt($"TIME CORE (LOCKED)\nRequires 3 Power Chips ({chips}/3 collected)");
                }

                if (ChronoInput.GetInteractDown())
                {
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayDenied();
                    }
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowNotification("Security door locked! Collect all 3 Power Chips first.");
                    }
                }
            }
            else
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowPrompt("TIME CORE ONLINE\nPress [E] to Restore All Eras!");
                }

                if (ChronoInput.GetInteractDown())
                {
                    ActivateCore();
                }
            }
        }
    }

    public void ActivateCore()
    {
        if (isActivated) return;
        isActivated = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayVictory();
        }

        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.TriggerShake(1.2f, 0.4f);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPrompt("TIMELINE RESTORED! You have saved all eras!");
            UIManager.Instance.ShowNotification("TIMELINE RESTORED! You have saved all eras!");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinGame();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            isPlayerInRange = false;
            if (UIManager.Instance != null && !isActivated)
            {
                UIManager.Instance.HidePrompt();
            }
        }
    }

    public void ResetCore()
    {
        isActivated = false;
        isPlayerInRange = false;
        if (initialPosition != Vector3.zero)
        {
            transform.position = initialPosition;
        }
    }
}
