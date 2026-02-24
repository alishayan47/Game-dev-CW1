using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public TMP_Text scoreText;
    public GameObject losePanel;
    public GameObject winPanel;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        losePanel.SetActive(false);
        winPanel.SetActive(false);
    }

    public void UpdateScore(int current, int target)
    {
        scoreText.text = $"Score: {current} / {target}";
    }

    public void ShowLosePanel()
    {
        losePanel.SetActive(true);
    }

    public void ShowWinPanel()
    {
        winPanel.SetActive(true);
    }
}