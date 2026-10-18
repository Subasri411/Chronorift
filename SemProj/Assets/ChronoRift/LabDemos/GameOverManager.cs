using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverMenuUI; // Assign the Game Over menu UI panel in the Inspector
    public PlayerHealth playerHealth; // Reference to the player's health script
    private bool isGameOver = false;

    void Start()
    {
        if (gameOverMenuUI != null)
        {
            gameOverMenuUI.SetActive(false);
        }
        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
        }
    }

    void Update()
    {
        if (playerHealth != null && playerHealth.currentHealth <= 0 && !isGameOver)
        {
            TriggerGameOver();
        }

        // Diagnostic trigger: Press K to simulate lethal damage for testing
        if (Input.GetKeyDown(KeyCode.K) && playerHealth != null && !isGameOver)
        {
            playerHealth.TakeDamage(100);
        }
    }

    public void TriggerGameOver()
    {
        isGameOver = true;
        if (gameOverMenuUI != null)
        {
            gameOverMenuUI.SetActive(true); // Show the Game Over menu
        }
        Time.timeScale = 0f; // Pause the game
    }

    // Restart the game
    public void RestartGame()
    {
        Time.timeScale = 1f; // Unpause the game
        if (Application.isPlaying)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
        }
    }

    // Quit the game
    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
