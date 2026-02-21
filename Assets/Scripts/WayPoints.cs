using UnityEngine;
using System.Collections.Generic;

public class WaypointNode : MonoBehaviour
{
    public List<WaypointNode> neighbours = new List<WaypointNode>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.3f);

        Gizmos.color = Color.white;
        foreach (var n in neighbours)
        {
            if (n != null)
                Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }
}
