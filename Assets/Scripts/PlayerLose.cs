using UnityEngine;

public class PlayerLose : MonoBehaviour 
{
    public float fallDeathY = -10f; // Y-position threshold below which the player loses 

    private bool hasLost = false; 
    private Rigidbody rb; 

    void Start() 
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update() 
    {
        if (hasLost) 
            return; // Stops further checks if the player has already lost

        if (transform.position.y < fallDeathY) // Checks if player has fallen below allowed Y limit
        {
            LoseGame(); // Triggers loss if player falls off the map
        }
    }

    void OnCollisionEnter(Collision collision) 
    {
        if (hasLost) 
            return; // Prevents repeated loss triggers

        if (collision.collider.CompareTag("Cop") || collision.collider.CompareTag("Traffic"))
        {
            // Checks if collided object is a Cop or Traffic using tags
            LoseGame(); // Triggers loss on collision with those objects
        }
    }

    void LoseGame() // all logic that occurs when player loses
    {
        hasLost = true; // to prevent re entry into this function

        rb.velocity = Vector3.zero; // Immediately stops all linear movement
        rb.angularVelocity = Vector3.zero; // stops all rotational movement


        Debug.Log("YOU LOST"); //  for debugging purposes

        if (UIManager.instance != null) // Ensures UIManager exists before calling it
        {
            UIManager.instance.ShowLosePanel();
            // Displays the lose UI panel on screen
        }

        Time.timeScale = 0f;
        // Pauses the entire game by stopping time progression
    }
}