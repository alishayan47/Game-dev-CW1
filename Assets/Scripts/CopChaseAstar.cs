using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class CopChaseAI : MonoBehaviour
{
    public Transform target;
    public Transform waypointsParent;

    [Header("Movement")]
    public float moveSpeed = 10f;
    public float turnSpeed = 6f;
    public float acceleration = 25f;

    [Header("Chase Settings")]
    public float directChaseDistance = 6f;

    [Header("Path Settings")]
    public float nodeReachDistance = 0.8f;

    private Rigidbody rb;

    private List<WaypointNode> currentPath = new List<WaypointNode>();
    private int currentPathIndex = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        RecalculatePath();
    }

    void FixedUpdate()
    {
        if (!target) return;

        Vector3 desiredDirection;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget < directChaseDistance)
        {
            desiredDirection = GetDirectChaseDirection();
        }
        else
        {
            if (currentPath == null || currentPathIndex >= currentPath.Count)
                RecalculatePath();

            desiredDirection = GetPathDirection();
        }

        ApplyMovement(desiredDirection);
    }

    Vector3 GetPathDirection()
    {
        if (currentPath == null || currentPathIndex >= currentPath.Count)
            return transform.forward;

        Vector3 nodeTarget = currentPath[currentPathIndex].transform.position;
        Vector3 toNode = nodeTarget - transform.position;
        toNode.y = 0f;

        if (toNode.magnitude < nodeReachDistance)
        {
            currentPathIndex++;
            return transform.forward;
        }

        return toNode.normalized;
    }

    Vector3 GetDirectChaseDirection()
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        return toTarget.normalized;
    }

    void ApplyMovement(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));

        float alignment = Vector3.Dot(transform.forward, direction);

        if (alignment > 0.5f)
        {
            if (rb.velocity.magnitude < moveSpeed)
            {
                rb.AddForce(transform.forward * acceleration, ForceMode.Acceleration);
            }
        }
        else
        {
            rb.velocity *= 0.9f;
        }
    }

    void RecalculatePath()
    {
        WaypointNode startNode = GetClosestNode(transform.position);
        WaypointNode goalNode = GetClosestNode(target.position);

        if (startNode == null || goalNode == null) return;

        currentPath = FindPath(startNode, goalNode);
        currentPathIndex = 0;
    }

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

    // -------- A* --------

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
            {
                if (GetFScore(node, goal, gScore) < GetFScore(current, goal, gScore))
                    current = node;
            }

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