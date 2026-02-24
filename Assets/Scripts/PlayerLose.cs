using UnityEngine;

public class PlayerLose : MonoBehaviour
{
    [Header("Fall Death Settings")]
    public float fallDeathY = -10f;

    private bool hasLost = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (hasLost) return;

        if (transform.position.y < fallDeathY)
        {
            LoseGame();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasLost) return;

        if (collision.collider.CompareTag("Cop") ||
            collision.collider.CompareTag("Traffic"))
        {
            LoseGame();
        }
    }

    void LoseGame()
    {
        hasLost = true;

        // Stop movement
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Disable other player scripts
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }

        Debug.Log("YOU LOST");

        // Show UI
        if (UIManager.instance != null)
        {
            UIManager.instance.ShowLosePanel();
        }

        // Pause game AFTER panel shows
        Time.timeScale = 0f;
    }
}