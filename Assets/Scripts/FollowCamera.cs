using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;

    [Header("Offsets")]
    public Vector3 backOffset = new Vector3(0f, 4f, -6f);
    public Vector3 frontOffset = new Vector3(0f, 4f, 6f);

    public float smoothSpeed = 8f;

    private bool frontView = false;

    void Update()
    {
        // Press C to toggle camera
        if (Input.GetKeyDown(KeyCode.E))
        {
            frontView = !frontView;
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector3 chosenOffset = frontView ? frontOffset : backOffset;

        Vector3 desiredPosition = target.position + target.TransformDirection(chosenOffset);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.LookAt(target.position + Vector3.up * 1.2f);
    }
}