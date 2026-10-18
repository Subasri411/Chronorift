using UnityEngine;

public class CastleLever : MonoBehaviour
{
    private static CastleLever instance;
    public static CastleLever Instance
    {
        get
        {
            if (instance == null) instance = FindAnyObjectByType<CastleLever>();
            return instance;
        }
        private set => instance = value;
    }

    [SerializeField] private Sprite leverOffSprite;
    [SerializeField] private Sprite leverOnSprite;
    [SerializeField] private SpriteRenderer leverRenderer;
    [SerializeField] private GameObject secretBookshelfVisual; // optional bookshelf that slides / reveals

    private bool isPlayerInRange = false;
    private bool isActivated = false;

    public bool IsActivated => isActivated;

    private void Awake()
    {
        Instance = this;
        if (leverRenderer == null) leverRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (isActivated) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGamePausedOrOver()) return;

        if (isPlayerInRange)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPrompt("HIDDEN LEVER\nPress [E] to Pull Ancient Lever");
            }

            if (ChronoInput.GetInteractDown())
            {
                ActivateLever();
            }
        }
    }

    public void ActivateLever()
    {
        if (isActivated) return;
        isActivated = true;

        if (leverRenderer != null && leverOnSprite != null)
        {
            leverRenderer.sprite = leverOnSprite;
        }

        if (secretBookshelfVisual != null)
        {
            secretBookshelfVisual.SetActive(true);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayAltarUnlock();
        }

        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.TriggerShake(0.6f, 0.25f);
        }

        if (SecretDoor.Instance != null)
        {
            SecretDoor.Instance.OpenDoor();
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CanCollectTimeCrystal = true;
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPrompt("Secret Room Unlocked!");
            UIManager.Instance.ShowNotification("Rumble... A secret stone door slides open in the castle wall!");
            UIManager.Instance.SetObjective("The secret room is open! Enter and claim the Time Crystal.");
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

    public void ResetLever()
    {
        isActivated = false;
        isPlayerInRange = false;
        if (leverRenderer != null && leverOffSprite != null)
        {
            leverRenderer.sprite = leverOffSprite;
        }
    }
}
