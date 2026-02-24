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
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotate
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);

        // Floating / bobbing effect
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.instance != null)
                GameManager.instance.AddScore(scoreValue);
            Destroy(gameObject);
        }
    }
}