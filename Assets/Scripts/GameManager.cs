using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Score")]
    public int currentScore = 0;
    public int targetScore = 5;

    void Awake()
    {
        if (instance == null) 
            instance = this;
        else 
            Destroy(gameObject);

        // making sure the level is not stuck paused
        Time.timeScale = 1f;
    }

    void Start()
    {
        // to show initial score immediately (0/target)
        if (UIManager.instance != null)
            UIManager.instance.UpdateScore(currentScore, targetScore);
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        Debug.Log("SCORE: " + currentScore);

        if (UIManager.instance != null)
            UIManager.instance.UpdateScore(currentScore, targetScore);

        if (currentScore >= targetScore)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        Debug.Log("YOU WIN");

        if (UIManager.instance != null)
        {
            UIManager.instance.ShowWinPanel();
        }

        Time.timeScale = 0f;
    }

}