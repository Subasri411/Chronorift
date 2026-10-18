using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null) instance = FindAnyObjectByType<UIManager>();
            return instance;
        }
        private set => instance = value;
    }

    [Header("Panels")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("HUD Elements")]
    [SerializeField] private Text livesText;
    [SerializeField] private Text eggCounterText;
    [SerializeField] private Image[] eggIcons;
    [SerializeField] private Text objectiveText;
    [SerializeField] private GameObject promptBox;
    [SerializeField] private Text promptText;
    [SerializeField] private Text notificationText;

    [Header("Victory Elements")]
    [SerializeField] private Text victoryStatsText;
    [SerializeField] private GameObject nextLevelBtn;

    private float notificationTimer = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Start()
    {
        ShowMainMenu();
        if (GameManager.Instance != null)
        {
            UpdateLivesUI(GameManager.Instance.CurrentLives);
        }
    }

    private void Update()
    {
        if (notificationTimer > 0f)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0f && notificationText != null)
            {
                notificationText.gameObject.SetActive(false);
            }
        }
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        HidePrompt();
    }

    public void ShowGameplay()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    public void ShowPauseMenu(bool show)
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(show);
        if (show)
        {
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        }
    }

    public void ShowHowToPlay(bool show)
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(show);
    }

    public void ShowVictory(float timeElapsed)
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(true);

        int minutes = Mathf.FloorToInt(timeElapsed / 60f);
        int seconds = Mathf.FloorToInt(timeElapsed % 60f);
        bool isLevel3 = SceneManager.GetActiveScene().name.Contains("3");
        bool isLevel2 = SceneManager.GetActiveScene().name.Contains("2") && !isLevel3;

        if (victoryStatsText != null)
        {
            if (isLevel3)
            {
                victoryStatsText.text = $"TIMELINE RESTORED!\nYou have saved all eras!\n\nTIME: {minutes:00}:{seconds:00}\nLIVES REMAINING: {(GameManager.Instance != null ? GameManager.Instance.CurrentLives : 5)} / 5\nPOWER CHIPS: 3 / 3\nTIME CORE: RESTORED\nALL ERAS STABILIZED: 100%";
            }
            else if (isLevel2)
            {
                victoryStatsText.text = $"TIME: {minutes:00}:{seconds:00}\nLIVES REMAINING: {(GameManager.Instance != null ? GameManager.Instance.CurrentLives : 5)} / 5\nSECRET ROOM: UNLOCKED\nTIME CRYSTAL: RECOVERED\nTIMELINE STABILITY: 100%";
            }
            else
            {
                victoryStatsText.text = $"TIME: {minutes:00}:{seconds:00}\nLIVES REMAINING: {(GameManager.Instance != null ? GameManager.Instance.CurrentLives : 5)} / 5\nSACRED DRAGON EGGS: 3 / 3\nTIME CRYSTAL: RECOVERED\nTIMELINE STABILITY: 100%";
            }
        }

        if (nextLevelBtn != null)
        {
            nextLevelBtn.SetActive(!isLevel3); // Only active on Level 1 and Level 2
        }
    }

    public void OnNextLevelClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NextLevel();
        }
    }

    public void UpdateLivesUI(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"LIVES: {lives} / 5";
        }
    }

    public void UpdateEggUI(int count, bool[] collectedFlags)
    {
        if (eggCounterText != null)
        {
            eggCounterText.text = $"EGGS: {count} / 3";
        }

        if (eggIcons != null)
        {
            for (int i = 0; i < eggIcons.Length; i++)
            {
                if (eggIcons[i] != null)
                {
                    bool isGot = i < collectedFlags.Length && collectedFlags[i];
                    eggIcons[i].color = isGot ? Color.white : new Color(0.25f, 0.25f, 0.25f, 0.6f);
                }
            }
        }
    }

    public void UpdatePowerChipUI(int count)
    {
        if (eggCounterText != null)
        {
            eggCounterText.text = $"Power Chips: {count}/3";
        }

        if (eggIcons != null)
        {
            for (int i = 0; i < eggIcons.Length; i++)
            {
                if (eggIcons[i] != null)
                {
                    bool isGot = i < count;
                    eggIcons[i].color = isGot ? Color.white : new Color(0.25f, 0.25f, 0.25f, 0.6f);
                }
            }
        }
    }

    public void SetObjective(string text)
    {
        if (objectiveText != null)
        {
            objectiveText.text = "OBJECTIVE: " + text;
        }
    }

    public void ShowPrompt(string text)
    {
        if (promptBox != null) promptBox.SetActive(true);
        if (promptText != null) promptText.text = text;
    }

    public void HidePrompt()
    {
        if (promptBox != null) promptBox.SetActive(false);
    }

    public void ShowNotification(string text)
    {
        if (notificationText != null)
        {
            notificationText.text = text;
            notificationText.gameObject.SetActive(true);
            notificationTimer = 3.5f;
        }
    }
}
