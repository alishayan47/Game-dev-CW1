using UnityEngine;

public class TrafficAI : MonoBehaviour 
{
    public WaypointNode currentNode; // the node that this traffic car is currently positioned at in the road graph

    public float moveSpeed = 6f; // how fast the traffic car moves forward
    public float turnSpeed = 5f; // how quickly the car rotates toward its next node
    public float nodeReachDistance = 0.8f; //threshold to decide when a node has been reached

    private WaypointNode targetNode; // next node the car is currently driving toward
    private Rigidbody rb; // Ref to the Rigidbody for physics-based movement

    void Start()
    {
        rb = GetComponent<Rigidbody>(); 

        if (currentNode == null) // check to ensure a starting node was assigned in Inspector
        {
            Debug.LogError("TrafficAI: No starting node assigned"); // for debugging
            return; // Stopping execution to prevent null reference issues otherwise i can never figure out what the problem is
        }

        ChooseNextNode(); // Selects the first target node to begin movement
    }

    void FixedUpdate() // since it is called at fixed intervals, makes sense to use this for traffic
    {
        if (targetNode == null) // no movement logic if no target node is assigned
            return; 

        Vector3 targetPos = targetNode.transform.position; //  position of the target node
        Vector3 toTarget = targetPos - transform.position; // calculates vector from car to target node

        toTarget.y = 0f; // removes vertical difference to keep car grounded and avoid tilting

        float distance = toTarget.magnitude; // Computes straight line distance to target node

        if (distance < nodeReachDistance) // Checks if car is close enough to consider node reached (helps in reduce lag)
        {
            currentNode = targetNode; // Updates current node to the one just reached
            ChooseNextNode(); // Selects a new next node to continue movement
            return;
        }

        Vector3 direction = toTarget.normalized; // convertmovement vector into a unit direction

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        // Creates rotation that faces the direction of travel

        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        // Smoothly rotates the car toward the desired direction for natural turning

        Vector3 newPosition = rb.position + transform.forward * moveSpeed * Time.fixedDeltaTime;
        // Calculates next position by moving forward relative to current orientation

        rb.MovePosition(newPosition); // moving Rigidbody using physics 

    }

    void ChooseNextNode() // Determines which connected node to drive toward next
    {
        if (currentNode.neighbours == null || currentNode.neighbours.Count == 0)
        {
            // Checks if this node has no outgoing connections (dead end) (should not be any in our map structure)

            Debug.LogWarning("Traffic reached dead-end node: " + currentNode.name); // For debuggging

            targetNode = null; // Stops movement if no next node exists
            return;
        }

        int randomIndex = Random.Range(0, currentNode.neighbours.Count); // Selects a random neighbour index to add unpredictability to traffic

        targetNode = currentNode.neighbours[randomIndex]; // Assigns chosen neighbour as the next target node
    }
}