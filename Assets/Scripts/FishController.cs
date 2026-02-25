using UnityEngine;

public class FishController : MonoBehaviour 
{
    public float acceleration = 50f; 
    public float maxSpeed = 30f; // maximum velocity magnitude to prevent infinite acceleration
    public float turnSpeed = 5f; //  how strongly the player rotates left/right
    public float jumpForce = 7f; 
    public float groundCheckDistance = 0.3f; // Distance used for raycast to detect ground below player
    public LayerMask groundLayer; // Specifies which layers count as ground for jump detection

    private Rigidbody rb; 
    private bool isGrounded; // whether player is currently touching the ground
    private Animator anim; // Animator to control animation states

    void Start() 
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>(); 
    }

    void FixedUpdate() 
    {
        float move = Input.GetAxis("Vertical");
        // forward/backward input 

        float turn = Input.GetAxis("Horizontal");
        // left/right input 

        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer); // Casts a ray downward to detect if player is close to ground
        // ensures jumping only happens when touching ground layer

        anim.SetBool("IsGrounded", isGrounded);

        if (rb.velocity.magnitude < maxSpeed)
        {
            rb.AddForce(transform.forward * move * acceleration); // Applies forward force relative to player orientation,
            // acceleration scaled by input value to allow forward and backward motion
        }

        if (rb.velocity.magnitude > 0.5f)
        {
            rb.AddTorque(Vector3.up * turn * turnSpeed);  // scaled by input to allow steering  only when moving
            // Applies rotational torque around Y axis
        }

        if (Input.GetKey(KeyCode.Space) && isGrounded)// only allowed when player is grounded
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Applies upward motion to simulate jump
                                                //impulse changes velocity instantly
        }
    }
}