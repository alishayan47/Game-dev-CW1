using UnityEngine;

public class PlayerLose : MonoBehaviour
{
    private bool hasLost = false;
    private Rigidbody rb;

    void Start()
    {
        Time.timeScale = 1f;
        rb = GetComponent<Rigidbody>();
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

        // Stop player movement
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Disable all movement scripts on player
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }

        // Freeze entire game
        Time.timeScale = 0f;

        Debug.Log("YOU LOST");
    }
}