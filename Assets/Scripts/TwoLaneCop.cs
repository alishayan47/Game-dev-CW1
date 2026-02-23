using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class TwoLaneCop : MonoBehaviour
{
    public Transform target;               // Player transform
    public Transform waypointsParent;      // Parent containing cop nodes

    [Header("Movement")]
    public float moveSpeed = 10f;          // Maximum forward speed
    public float turnSpeed = 6f;           // Rotation smoothing
    public float acceleration = 25f;       // Acceleration force

    [Header("Chase Settings")]
    public float directChaseDistance = 6f; // Switch to direct chase under this distance
    public float repathDistanceThreshold = 6f; // How far target must shift before repathing

    [Header("Path Settings")]
    public float nodeReachDistance = 0.8f; // Distance to consider a node reached

    private Rigidbody rb;

    private List<WaypointNode> currentPath = new List<WaypointNode>();
    private int currentPathIndex = 0;

    private WaypointNode lastGoalNode;    // Used to prevent goal flipping
    private bool isDirectChasing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Physics stability configuration
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        RecalculatePath(); // Initial path
    }

    void FixedUpdate()
    {
        if (!target) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // ---------------- DIRECT CHASE MODE ----------------
        if (distanceToTarget < directChaseDistance)
        {
            isDirectChasing = true;
            ApplyMovement(GetDirectChaseDirection());
            return;
        }

        // If we exit direct chase, force a fresh path
        if (isDirectChasing)
        {
            isDirectChasing = false;
            RecalculatePath();
        }

        // ---------------- SMART REPATHE CONTROL ----------------

        WaypointNode currentGoalNode = GetClosestNode(target.position);

        if (currentGoalNode != null && lastGoalNode != null)
        {
            float goalShift = Vector3.Distance(
                currentGoalNode.transform.position,
                lastGoalNode.transform.position);

            // Only repath if:
            // 1) Player moved significantly
            // 2) Cop has already progressed in its path (prevents early jitter)
            if (goalShift > repathDistanceThreshold && currentPathIndex > 1)
            {
                RecalculatePath();
            }
        }

        // If path ended or invalid
        if (currentPath == null || currentPathIndex >= currentPath.Count)
        {
            RecalculatePath();
        }

        ApplyMovement(GetPathDirection());
    }

    // Returns direction toward current path node
    Vector3 GetPathDirection()
    {
        if (currentPath == null || currentPathIndex >= currentPath.Count)
            return transform.forward;

        Vector3 nodeTarget = currentPath[currentPathIndex].transform.position;
        Vector3 toNode = nodeTarget - transform.position;
        toNode.y = 0f;

        // Advance node when close enough
        if (toNode.magnitude < nodeReachDistance && currentPathIndex < currentPath.Count - 1)
        {
            currentPathIndex++;
            return transform.forward;
        }

        return toNode.normalized;
    }

    // Returns direct pursuit direction
    Vector3 GetDirectChaseDirection()
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        return toTarget.normalized;
    }

    // Handles smooth rotation and forward movement
    void ApplyMovement(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));

        float alignment = Vector3.Dot(transform.forward, direction);

        // Allow movement even with mild turning
        if (alignment > 0.2f)
        {
            if (rb.velocity.magnitude < moveSpeed)
                rb.AddForce(transform.forward * acceleration, ForceMode.Acceleration);
        }
        else
        {
            rb.velocity *= 0.95f; // Mild braking when turning sharply
        }
    }

    // Recalculate A* path from current position to player's closest node
    void RecalculatePath()
    {
        WaypointNode startNode = GetClosestNode(transform.position);
        WaypointNode goalNode = GetClosestNode(target.position);

        if (startNode == null || goalNode == null)
        {
            currentPath = null;
            return;
        }

        currentPath = FindPath(startNode, goalNode);
        currentPathIndex = 0;
        lastGoalNode = goalNode;
    }

    // Finds nearest node in graph
    WaypointNode GetClosestNode(Vector3 position)
    {
        WaypointNode closest = null;
        float minDist = Mathf.Infinity;

        foreach (Transform child in waypointsParent)
        {
            WaypointNode node = child.GetComponent<WaypointNode>();
            if (!node) continue;

            float dist = Vector3.Distance(position, child.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }

        return closest;
    }

    // ---------------- A* SEARCH ----------------

    List<WaypointNode> FindPath(WaypointNode start, WaypointNode goal)
    {
        List<WaypointNode> openSet = new List<WaypointNode>();
        HashSet<WaypointNode> closedSet = new HashSet<WaypointNode>();

        Dictionary<WaypointNode, WaypointNode> cameFrom = new Dictionary<WaypointNode, WaypointNode>();
        Dictionary<WaypointNode, float> gScore = new Dictionary<WaypointNode, float>();

        openSet.Add(start);
        gScore[start] = 0f;

        while (openSet.Count > 0)
        {
            WaypointNode current = openSet[0];

            foreach (var node in openSet)
                if (GetFScore(node, goal, gScore) < GetFScore(current, goal, gScore))
                    current = node;

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbor in current.neighbours)
            {
                if (neighbor == null || closedSet.Contains(neighbor))
                    continue;

                float tentativeG = gScore[current] +
                    Vector3.Distance(current.transform.position, neighbor.transform.position);

                if (!openSet.Contains(neighbor))
                    openSet.Add(neighbor);
                else if (tentativeG >= gScore.GetValueOrDefault(neighbor, Mathf.Infinity))
                    continue;

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeG;
            }
        }

        return null;
    }

    float GetFScore(WaypointNode node, WaypointNode goal,
        Dictionary<WaypointNode, float> gScore)
    {
        float g = gScore.GetValueOrDefault(node, Mathf.Infinity);
        float h = Vector3.Distance(node.transform.position, goal.transform.position);
        return g + h;
    }

    List<WaypointNode> ReconstructPath(
        Dictionary<WaypointNode, WaypointNode> cameFrom,
        WaypointNode current)
    {
        List<WaypointNode> path = new List<WaypointNode> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }
}