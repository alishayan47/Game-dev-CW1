using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;

    [Header("Positioning")]
    public Vector3 offset = new Vector3(0f, 5f, -8f);
    public float smoothSpeed = 8f;

    void LateUpdate()
    {
        if (!target) return;

        // Desired position behind the player
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        // Smoothly move camera
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Always look at fish
        transform.LookAt(target.position + Vector3.up * 1.0f);
    }
}
