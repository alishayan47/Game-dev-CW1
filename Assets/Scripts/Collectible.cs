using UnityEngine;

public class Collectible : MonoBehaviour 
{
    public int scoreValue = 1; 
    public float rotationSpeed = 100f; 
    public float bobSpeed = 2f; 
    public float bobHeight = 0.25f;

    private Vector3 startPosition;

    void Start() 
    {
        startPosition = transform.position; // initial position so bobbing is relative to original height
    }

    void Update() 
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f); // Rotates object around Y axis to create spinning effect,
        // multiplied by deltaTime for consistent rotation speed per second
        
                // Time.time drives continuous animation,
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        // Uses sine wave to calculate smooth oscillation over time

        transform.position = new Vector3(transform.position.x, newY, transform.position.z); // Updates only the Y position while preserving X and Z,
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player")) // if the object entering the trigger is the player
        {
            if (GameManager.instance != null) // Safety check to ensure GameManager exists
                GameManager.instance.AddScore(scoreValue);
            // Calls AddScore method to increase player's score by this collectible's value

            Destroy(gameObject);
            // Removes collectible from scene after being picked up 
        }
    }
}