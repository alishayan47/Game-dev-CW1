using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TrafficAI : MonoBehaviour
{
    public WaypointNode currentNode;

    public float moveSpeed = 6f;
    public float turnSpeed = 5f;
    public float nodeReachDistance = 0.8f;

    private WaypointNode targetNode;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (currentNode == null)
        {
            Debug.LogError("TrafficAI: No starting node assigned!");
            return;
        }

        ChooseNextNode();
    }

    void FixedUpdate()
    {
        if (targetNode == null) return;

        Vector3 targetPos = targetNode.transform.position;
        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;

        if (distance < nodeReachDistance)
        {
            currentNode = targetNode;
            ChooseNextNode();
            return;
        }

        Vector3 direction = toTarget.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));

        Vector3 newPosition = rb.position + transform.forward * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    void ChooseNextNode()
    {
        if (currentNode.neighbours == null || currentNode.neighbours.Count == 0)
        {
            Debug.LogWarning("Traffic reached dead-end node: " + currentNode.name);
            targetNode = null;
            return;
        }

        int randomIndex = Random.Range(0, currentNode.neighbours.Count);
        targetNode = currentNode.neighbours[randomIndex];
    }
}