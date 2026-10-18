using UnityEngine;

public class Altar : MonoBehaviour
{
    private static Altar instance;
    public static Altar Instance
    {
        get
        {
            if (instance == null) instance = FindAnyObjectByType<Altar>();
            return instance;
        }
        private set => instance = value;
    }

    [SerializeField] private GameObject[] eggSockets; // 3 visual sockets that light up
    // interact radius handled by trigger collider

    private bool isPlayerInRange = false;
    private bool isActivated = false;

    public bool IsActivated => isActivated;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ResetAltar();
    }

    private void Update()
    {
        if (isActivated) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGamePausedOrOver()) return;

        if (isPlayerInRange)
        {
            int eggs = GameManager.Instance != null ? GameManager.Instance.EggsCollectedCount : 0;
            if (eggs < 3)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowPrompt($"ANCIENT ALTAR (LOCKED)\nRequires 3 Sacred Dragon Eggs ({eggs}/3 found)");
                }

                if (ChronoInput.GetInteractDown())
                {
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayDenied();
                    }
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowNotification("The altar rejects you! Search the jungle for all 3 Dragon Eggs.");
                    }
                }
            }
            else
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowPrompt("ANCIENT ALTAR\nPress [E] to Place Dragon Eggs & Open Temple");
                }

                if (ChronoInput.GetInteractDown())
                {
                    ActivateAltar();
                }
            }
        }
    }

    public void ActivateAltar()
    {
        if (isActivated) return;
        isActivated = true;

        // Light up the egg sockets
        if (eggSockets != null)
        {
            foreach (var sock in eggSockets)
            {
                if (sock != null) sock.SetActive(true);
            }
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayAltarUnlock();
        }

        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.TriggerShake(0.6f, 0.25f);
        }

        if (TempleGate.Instance != null)
        {
            TempleGate.Instance.OpenGate();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAltarActivated();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPrompt("Temple Gate Unlocked! Enter the Ancient Temple.");
            UIManager.Instance.ShowNotification("Rumble... The temple gates have swung open!");
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

    public void ResetAltar()
    {
        isActivated = false;
        isPlayerInRange = false;
        if (eggSockets != null)
        {
            foreach (var sock in eggSockets)
            {
                if (sock != null) sock.SetActive(false);
            }
        }
    }
}
