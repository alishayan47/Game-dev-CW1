using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FishController : MonoBehaviour
{
    public float acceleration = 50f;
    public float maxSpeed = 30f;
    public float turnSpeed = 5f;
    public float jumpForce = 7f;
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool isGrounded;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }


    void FixedUpdate()
    {
        float move = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");

        // Ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        anim.SetBool("IsGrounded", isGrounded);
        // Move forward
        if (rb.velocity.magnitude < maxSpeed)
        {
            rb.AddForce(transform.forward * move * acceleration);
        }

        // Turn
        if (rb.velocity.magnitude > 0.5f)
        {
            rb.AddTorque(Vector3.up * turn * turnSpeed);
        }

        // Jump
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
