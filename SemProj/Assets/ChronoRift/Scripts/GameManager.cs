using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Victory
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindAnyObjectByType<GameManager>();
            return instance;
        }
        private set => instance = value;
    }

    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    public const int MaxLives = 5;
    public int CurrentLives { get; private set; } = MaxLives;
    public bool CanCollectTimeCrystal { get; set; } = false;

    // Level 1: Dragon Eggs
    private readonly bool[] eggsCollected = new bool[3];
    private int eggsCount = 0;
    private bool altarActivated = false;

    // Level 3: Power Chips
    private readonly bool[] powerChipsCollected = new bool[3];
    private int powerChipsCount = 0;

    private float levelTimer = 0f;

    public int EggsCollectedCount => eggsCount;
    public bool IsAltarActivated => altarActivated;
    public int PowerChipsCount => powerChipsCount;
    public bool IsSecurityDoorUnlocked => powerChipsCount >= 3;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        CurrentState = GameState.MainMenu;
        CurrentLives = MaxLives;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Start()
    {
        GoToMainMenu();
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            levelTimer += Time.deltaTime;

            if (ChronoInput.GetPauseDown())
            {
                PauseGame();
            }
        }
        else if (CurrentState == GameState.Paused)
        {
            if (ChronoInput.GetPauseDown())
            {
                ResumeGame();
            }
        }
    }

    public bool IsGamePausedOrOver()
    {
        return CurrentState != GameState.Playing;
    }

    public void StartGame()
    {
        ResetAllGameElements();
        levelTimer = 0f;
        SetState(GameState.Playing);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameplay();
            UIManager.Instance.UpdateLivesUI(CurrentLives);

            bool isLevel3 = SceneManager.GetActiveScene().name.Contains("3");
            bool isLevel2 = SceneManager.GetActiveScene().name.Contains("2") && !isLevel3;

            if (isLevel3)
            {
                UIManager.Instance.SetObjective("Search the laboratory to collect 3 Power Chips.");
                UIManager.Instance.ShowNotification("Era 3: Future City — Abandoned Time Lab");
                UIManager.Instance.UpdatePowerChipUI(powerChipsCount);
            }
            else if (isLevel2)
            {
                UIManager.Instance.SetObjective("Find the hidden lever and discover the secret room.");
                UIManager.Instance.ShowNotification("Era 2: Medieval Castle & Royal Library (1340 AD)");
            }
            else
            {
                UIManager.Instance.UpdateEggUI(eggsCount, eggsCollected);
                UIManager.Instance.SetObjective("Search the prehistoric jungle to find the 3 Sacred Dragon Eggs.");
                UIManager.Instance.ShowNotification("Mission Begin: Collect 3 Sacred Dragon Eggs!");
            }
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetInputEnabled(true);
        }
    }

    public void LoseLife()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentLives--;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLivesUI(CurrentLives);
        }

        if (CurrentLives <= 0)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowNotification("Chrono Energy Depleted! Restarting level...");
            }
            RestartGame();
        }
        else
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowNotification($"Chrono Shield damaged! Lives: {CurrentLives}/{MaxLives}");
            }
        }
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        SetState(GameState.Paused);
        Time.timeScale = 0f;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPauseMenu(true);
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick();
        }
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        Time.timeScale = 1f;
        SetState(GameState.Playing);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPauseMenu(false);
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick();
        }
    }

    public void CollectEgg(int eggId, string eggName)
    {
        if (eggId >= 0 && eggId < eggsCollected.Length)
        {
            if (!eggsCollected[eggId])
            {
                eggsCollected[eggId] = true;
                eggsCount++;

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateEggUI(eggsCount, eggsCollected);
                    UIManager.Instance.ShowNotification($"Recovered: {eggName}! ({eggsCount}/3)");

                    if (eggsCount >= 3)
                    {
                        UIManager.Instance.SetObjective("All 3 Eggs recovered! Bring them to the Ancient Altar.");
                    }
                    else
                    {
                        UIManager.Instance.SetObjective($"Find the remaining Sacred Dragon Eggs ({eggsCount}/3 found).");
                    }
                }
            }
        }
    }

    public void CollectPowerChip(int chipId)
    {
        if (chipId >= 0 && chipId < powerChipsCollected.Length)
        {
            if (!powerChipsCollected[chipId])
            {
                powerChipsCollected[chipId] = true;
                powerChipsCount++;

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdatePowerChipUI(powerChipsCount);
                    UIManager.Instance.ShowNotification($"Power Chip recovered! ({powerChipsCount}/3)");

                    if (powerChipsCount >= 3)
                    {
                        UIManager.Instance.SetObjective("Security door unlocked! Activate the Time Core in the final chamber.");
                        UIManager.Instance.ShowNotification("Security grid deactivated! Time Core is now accessible!");
                        if (SecurityDoor.Instance != null)
                        {
                            SecurityDoor.Instance.OpenDoor();
                        }
                        CanCollectTimeCrystal = true;
                    }
                    else
                    {
                        UIManager.Instance.SetObjective($"Search the laboratory to collect 3 Power Chips ({powerChipsCount}/3 found).");
                    }
                }
            }
        }
    }

    public void OnAltarActivated()
    {
        altarActivated = true;
        CanCollectTimeCrystal = true;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetObjective("The Temple is open! Enter and retrieve the Time Crystal.");
        }
    }

    public void WinGame()
    {
        if (CurrentState == GameState.Victory) return;
        SetState(GameState.Victory);

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetInputEnabled(false);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowVictory(levelTimer);
            bool isLevel3 = SceneManager.GetActiveScene().name.Contains("3");
            if (isLevel3)
            {
                UIManager.Instance.ShowNotification("TIMELINE RESTORED! You have saved all eras!");
            }
            else
            {
                UIManager.Instance.ShowNotification("TIMELINE STABILIZED! Great work Chrono Agent!");
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        ResetAllGameElements();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameplay();
            UIManager.Instance.UpdateLivesUI(CurrentLives);

            bool isLevel3 = SceneManager.GetActiveScene().name.Contains("3");
            bool isLevel2 = SceneManager.GetActiveScene().name.Contains("2") && !isLevel3;

            if (isLevel3)
            {
                UIManager.Instance.SetObjective("Search the laboratory to collect 3 Power Chips.");
                UIManager.Instance.ShowNotification("Era 3: Future City — Abandoned Time Lab");
                UIManager.Instance.UpdatePowerChipUI(powerChipsCount);
            }
            else if (isLevel2)
            {
                UIManager.Instance.SetObjective("Find the hidden lever and discover the secret room.");
                UIManager.Instance.ShowNotification("Era 2: Medieval Castle & Royal Library (1340 AD)");
            }
            else
            {
                UIManager.Instance.UpdateEggUI(eggsCount, eggsCollected);
                UIManager.Instance.SetObjective("Search the prehistoric jungle to find the 3 Sacred Dragon Eggs.");
                UIManager.Instance.ShowNotification("Mission Begin: Collect 3 Sacred Dragon Eggs!");
            }
        }

        levelTimer = 0f;
        SetState(GameState.Playing);

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetInputEnabled(true);
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        ResetAllGameElements();
        SetState(GameState.MainMenu);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMainMenu();
        }
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "SampleScene")
        {
            string level2Name = Application.CanStreamedLevelBeLoaded("Level 2") ? "Level 2" : "Level2";
            SceneManager.LoadScene(level2Name);
        }
        else if (currentScene == "Level 2" || currentScene == "Level2")
        {
            SceneManager.LoadScene("Level 3");
        }
        else
        {
            Debug.LogWarning("No next level configured for scene: " + currentScene);
        }
    }

    public void QuitGame()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick();
        }
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void ResetAllGameElements()
    {
        CurrentLives = MaxLives;
        CanCollectTimeCrystal = false;

        // Reset eggs (Level 1)
        for (int i = 0; i < eggsCollected.Length; i++)
        {
            eggsCollected[i] = false;
        }
        eggsCount = 0;
        altarActivated = false;

        var eggs = FindObjectsByType<DragonEgg>(FindObjectsInactive.Include);
        foreach (var egg in eggs)
        {
            egg.ResetEgg();
        }

        if (Altar.Instance != null) Altar.Instance.ResetAltar();
        if (TempleGate.Instance != null) TempleGate.Instance.ResetGate();

        // Reset lever & secret door (Level 2)
        if (CastleLever.Instance != null) CastleLever.Instance.ResetLever();
        if (SecretDoor.Instance != null) SecretDoor.Instance.ResetDoor();

        // Reset power chips & security door & time core (Level 3)
        for (int i = 0; i < powerChipsCollected.Length; i++)
        {
            powerChipsCollected[i] = false;
        }
        powerChipsCount = 0;

        var chips = FindObjectsByType<PowerChip>(FindObjectsInactive.Include);
        foreach (var chip in chips)
        {
            chip.ResetChip();
        }

        if (SecurityDoor.Instance != null) SecurityDoor.Instance.ResetDoor();
        if (TimeCore.Instance != null) TimeCore.Instance.ResetCore();

        // Reset time crystal
        if (TimeCrystal.Instance != null)
        {
            TimeCrystal.Instance.ResetCrystal();
        }

        // Reset player
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.Respawn();
            PlayerController.Instance.SetInputEnabled(CurrentState == GameState.Playing);
        }
    }

    private void SetState(GameState newState)
    {
        CurrentState = newState;
    }
}
