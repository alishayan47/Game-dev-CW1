using UnityEngine;
using System.Collections.Generic;

public class CopChaseAI2 : MonoBehaviour
{
    public Transform target;       // player transform that the cop will chase
    public Transform waypointsParent;   // Parent object containing all cop waypoint nodes

    //Movement
    public float moveSpeed = 10f;    // Maximum allowed forward speed
    public float turnSpeed = 6f;     // hyperparameter for How fast the cop rotates toward its target direction
    public float acceleration = 25f;        // Force applied to move the cop forward

    //chase settings
    public float directChaseDistance = 6f;   //  distance under which the cop switches from A* to direct chasing (need this so the cop swerves)
    public float repathDistanceThreshold = 6f;   // Minimum movement of player before recalculating path

    //path settings
    public float nodeReachDistance = 0.8f;  // distance considered close enough to a node to move to the next one

    private Rigidbody rb;

    private List<WaypointNode2> currentPath = new List<WaypointNode2>(); //  to store A* path
    private int currentPathIndex = 0;       // to track which node in the path we are currently moving toward

    private WaypointNode2 lastGoalNode;     // Stores previous goal node to prevent unnecessary repathing
    private bool isDirectChasing = false;   // Tracks whether cop is in direct chase mode or in A* mode

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.centerOfMass = new Vector3(0, -0.5f, 0); // trying to lower center of mass to improve stability

        RecalculatePath();                  // Compute initial A* path when game starts
    }

    void FixedUpdate()
    {
        if (!target) // debugging check in case player reference is missing
            return; 

        float distanceToTarget = Vector3.Distance(transform.position, target.position); // Measure distance to player

        // To Enter Direct Chase
        if (distanceToTarget < directChaseDistance) // If player is close enough
        {
            isDirectChasing = true;         // Enter direct chase mode
            ApplyMovement(GetDirectChaseDirection());  // Move directly toward player
            return;           // Skip A* logic this frame
        }

        // To EXIT Direct Chase mode
        if (isDirectChasing)    // If we were chasing but now player is far
        {
            isDirectChasing = false;  // Exit direct chase mode

            rb.angularVelocity = Vector3.zero;   // resetting rotation momentum to prevent spinning (this was a major debugging problem)

            Vector3 localVel = transform.InverseTransformDirection(rb.velocity); // Convert velocity into local space
            localVel.x = 0f;                // Remove sideways drift
            rb.velocity = transform.TransformDirection(localVel); // Convert back to world space

            lastGoalNode = null;     // Forcing path recalculation
            RecalculatePath();      // Resume A* navigation
        }

        // Repathing
        WaypointNode2 currentGoalNode = GetClosestNode(target.position); // Determine nearest node to player

        if (currentGoalNode != null)
        {
            if (lastGoalNode == null)       // First frame of tracking
            {
                lastGoalNode = currentGoalNode; // Store initial goal
            }
            else
            {
                float goalShift = Vector3.Distance(currentGoalNode.transform.position, lastGoalNode.transform.position); // Measure how far player moved in graph

                if (goalShift > repathDistanceThreshold && currentPathIndex > 1) // Because if the cop is still near the start of its path, repathing constantly causes jitter.
                {
                    RecalculatePath();      // to only recompute A* if player moved meaningfully
                }
            }
        }

        if (currentPath == null || currentPathIndex >= currentPath.Count) // If path is invalid
        {
            RecalculatePath();              // Rebuild A* path
        }
        ApplyMovement(GetPathDirection());  // Continue moving along A* path
    }

    Vector3 GetPathDirection()
    {
        if (currentPath == null || currentPath.Count == 0)
            return Vector3.zero;            // No direction if no path

        while (currentPathIndex < currentPath.Count) // Skip nodes we are already touching
        {
            Vector3 toNode = currentPath[currentPathIndex].transform.position - transform.position;
            toNode.y = 0f;          // Ignore vertical difference (so cars dont fly)

            if (toNode.magnitude < nodeReachDistance)
            {
                currentPathIndex++;         // Move to next node
            }
            else
            {
                return toNode.normalized;   // Return normalized direction toward node (because direction should not scale speed)
            }
        }

        return Vector3.zero;    // Return zero if path finished (again avoids post path spinning)
    }

    Vector3 GetDirectChaseDirection()
    {
        Vector3 toTarget = target.position - transform.position; // Direction to player
        toTarget.y = 0f;           // Ignoring height
        return toTarget.normalized;    // Returning normalized direction
    }

    void ApplyMovement(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) // Prevent zero direction rotation
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction); // to create a rotation that is facing direction
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime)); // Smooth rotation over time

        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity); // Convert velocity to local space
        localVelocity.x = 0f;          // Remove sideways movement
        rb.velocity = transform.TransformDirection(localVelocity); // Convert back to world space

        float alignment = Vector3.Dot(transform.forward, direction); // checking how aligned we are with direction

        if (alignment > 0.2f)          // Only accelerate if mostly facing target (otherwise cops were doing donuts)
        {
            if (rb.velocity.magnitude < moveSpeed)
                rb.AddForce(transform.forward * acceleration, ForceMode.Acceleration); // Apply forward acceleration
        }
        else
        {
            rb.velocity *= 0.9f;       // Slight braking when turning sharply
        }

        rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, 3f); // Prevent excessive spinning (ANother strategy to avoid donuts)
    }

    void RecalculatePath()
    {
        WaypointNode2 startNode = GetClosestNode(transform.position); 
        WaypointNode2 goalNode = GetClosestNode(target.position);

        if (startNode == null || goalNode == null)
        {
            currentPath = null;   // If graph invalid, clear path
            return;
        }

        currentPath = FindPath(startNode, goalNode); // Run A* algorithm
        currentPathIndex = 0;     // Reset path index
        lastGoalNode = goalNode;   // Updating tracking goal

        if (currentPath != null && currentPath.Count > 1)
        {
            Vector3 toFirst = currentPath[0].transform.position - transform.position;
            toFirst.y = 0f;

            float dot = Vector3.Dot(transform.forward, toFirst.normalized); // Check if node is behind

            if (dot < 0f)             // If node is behind us
            {
                currentPathIndex = 1; // Skip it to prevent sudden U turn (and avoid cop collisions)
            }
        }
    }

    WaypointNode2 GetClosestNode(Vector3 position)
    {
        WaypointNode2 closest = null;
        float minDist = Mathf.Infinity;

        foreach (Transform child in waypointsParent) // Iterate all nodes
        {
            WaypointNode2 node = child.GetComponent<WaypointNode2>(); // Get node component
            if (!node) 
                continue;

            float dist = Vector3.Distance(position, child.position); // Measure distance

            if (dist < minDist)
            {
                minDist = dist;
                closest = node;       // Track closest node
            }
        }
        return closest;    // Return nearest node
    }

   
    // A* Search
    List<WaypointNode2> FindPath(WaypointNode2 start, WaypointNode2 goal)
    {
        // The openSet stores nodes that are discovered but not yet fully evaluated
        List<WaypointNode2> openSet = new List<WaypointNode2>();

        // The closedSet stores nodes that have already been evaluated
        HashSet<WaypointNode2> closedSet = new HashSet<WaypointNode2>(); // To prevent revisiting nodes and avoid infinite loops

        // cameFrom maps each node to the node we reached it from
        // This is required to reconstruct the final shortest path after reaching the goal
        Dictionary<WaypointNode2, WaypointNode2> cameFrom = new Dictionary<WaypointNode2, WaypointNode2>();

        // gScore stores the actual cost from the start node to each node
        Dictionary<WaypointNode2, float> gScore = new Dictionary<WaypointNode2, float>(); // to represent the real travel distance so far

        // put the start node to the openSet to begin exploration
        openSet.Add(start);

        // The cost to reach the start node from itself is always zero
        gScore[start] = 0f;

        // Continue searching while there are still nodes to explore
        while (openSet.Count > 0)
        {
            // We must select the node with the lowest estimated total cost (fScore)
            WaypointNode2 current = openSet[0];

            // Loop through all open nodes to find the one with the smallest fScore
            foreach (var node in openSet)
            {
                // fScore = gScore (real cost so far) + heuristic cost to goal
               
                if (GetFScore(node, goal, gScore) < GetFScore(current, goal, gScore)) // We compare each node to find the smallest total estimated path
                    current = node;
            }

            // If the current node is the goal, we have found the shortest path
            if (current == goal)
                return ReconstructPath(cameFrom, current);

            // Remove current from openSet since we are now evaluating it
            openSet.Remove(current);

            // Add it to closedSet so we do not evaluate it again
            closedSet.Add(current);

            // Check all neighbors connected to current node
            foreach (var neighbor in current.neighbours)
            {
                // Skip invalid or already evaluated neighbors
                if (neighbor == null || closedSet.Contains(neighbor))
                    continue;

                // Compute tentative cost from start to this neighbor via current
                // This is g(current) + distance(current to neighbor)
                float tentativeG = gScore[current] + Vector3.Distance(current.transform.position, neighbor.transform.position);

                // If neighbor is not in openSet, add it for future evaluation
                if (!openSet.Contains(neighbor))
                    openSet.Add(neighbor);

                // If we already have a better path to this neighbor, skip it
                else if (tentativeG >= gScore.GetValueOrDefault(neighbor, Mathf.Infinity)) // To prevent replacing a shorter path with a longer one
                    continue;

                // Record that we reached neighbor from current
                cameFrom[neighbor] = current;  // This is used later to reconstruct the full path

                // Update the best known cost to reach neighbor
                gScore[neighbor] = tentativeG;
            }
        }

        // If we exit the loop without finding the goal,
        // there is no possible path in the graph
        return null;
    }
    float GetFScore(WaypointNode2 node, WaypointNode2 goal, Dictionary<WaypointNode2, float> gScore)
    {
        // g = real cost from start node to this node
        float g = gScore.GetValueOrDefault(node, Mathf.Infinity);

        // h = heuristic cost estimate from this node to goal
        // we'll use Euclidean distance
        float h = Vector3.Distance(node.transform.position, goal.transform.position);

        // f = total estimated cost of path going through this node
        // A* chooses nodes with smallest f value first
        return g + h;
    }
    List<WaypointNode2> ReconstructPath(Dictionary<WaypointNode2, WaypointNode2> cameFrom, WaypointNode2 current)
    {
        // Create list starting from goal node
        List<WaypointNode2> path = new List<WaypointNode2> { current };

        // Walk backwards from goal to start using cameFrom mapping
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];

            // Insert at index 0 to reverse the order
            // to build path from start -> goal
            path.Insert(0, current);
        }

        // Return final ordered shortest path
        return path;
    }
}