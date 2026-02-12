using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FishController : MonoBehaviour
{
    public float acceleration = 20f;
    public float maxSpeed = 10f;
    public float turnSpeed = 12f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float move = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");

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
    }
}
