using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int currentScore = 0;
    public int targetScore = 5;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void AddScore(int amount)
    {
        currentScore += amount;

        Debug.Log("Score: " + currentScore);

        if (currentScore >= targetScore)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        Debug.Log("YOU WIN!");

        // Option 1: Freeze time
        Time.timeScale = 0f;

        // Option 2 (later): Load win screen scene
        // SceneManager.LoadScene("WinScene");
    }
}