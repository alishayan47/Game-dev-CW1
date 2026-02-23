using UnityEngine;
using System.Collections.Generic;

public class WaypointNode2 : MonoBehaviour
{
    public List<WaypointNode2> neighbours = new List<WaypointNode2>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.3f);

        Gizmos.color = Color.red;
        foreach (var n in neighbours)
        {
            if (n != null)
                Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }
}
