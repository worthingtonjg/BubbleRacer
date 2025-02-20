using UnityEngine;

public class CarController : MonoBehaviour
{
    public float speed = 10.0f;
    public float turnSpeed = 45.0f;

    private Rigidbody rb;
    private bool boostPressed;
    private bool brakesApplied;
    private float gasInput;
    private float steeringInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Accelerate
        float acceleration = (Input.GetAxis("Vertical") + gasInput) * speed;
        Vector3 movement = transform.forward * acceleration;
        rb.AddForce(movement, ForceMode.Acceleration);

        // Steer
        float turn = (Input.GetAxis("Horizontal") + steeringInput) * turnSpeed;
        Quaternion turnRotation = Quaternion.Euler(0, turn, 0);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    void Update()
    {
        // Brake
        if (Input.GetKey(KeyCode.Space) || brakesApplied)
        {
            rb.linearDamping = 10;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    public void ApplyGasInput(float value)
    {
        gasInput = value;
    }

    public void ApplySteeringInput(float value)
    {
        steeringInput = value;
    }

    public void ApplyBrakeInput(bool onOff)
    {
        brakesApplied = onOff;
    }

    public void ApplyBoostInput()
    {
        boostPressed = true;
    }
}
