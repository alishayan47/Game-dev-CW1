using UnityEngine;

public class FollowCamera : MonoBehaviour 
{
    public Transform target; // the player that the camera must follow

    public Vector3 backOffset = new Vector3(0f, 4f, -6f); // valyes for third person back view 
    public Vector3 frontOffset = new Vector3(0f, 4f, 6f); // for front facing view, (to see how far behind the cops are) 

    public float smoothSpeed = 8f; // controls how smoothly the camera interpolates toward the target position

    private bool frontView = false; // Tracks which mode the camera currently is in

    void Update() 
    {
        // Press E to toggle camera
        if (Input.GetKeyDown(KeyCode.E)) // if the E key was pressed this frame
        {
            frontView = !frontView; // Flips the boolean so camera switches between front and back ones
        }
    }

    void LateUpdate() // Called after all Update methods, using camera movement in this to avoid jitter
    {
        if (!target) return; // troubleshoots errors if no target has been assigned

        Vector3 chosenOffset = frontView ? frontOffset : backOffset; // Chooses which offset to use based on current view mode

        Vector3 desiredPosition = target.position + target.TransformDirection(chosenOffset); // Converts local offset into world space relative to the player's rotation
        // so camera stays properly behind/in front even when player rotates

        // to smoothly interpolates the camera from current position to desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        // preventing snapping and creating smooth follow movement

        //Rotates camera to look slightly above the player's position
        transform.LookAt(target.position + Vector3.up * 1.2f); // so the camera focuses around head instead of feet

    }
}