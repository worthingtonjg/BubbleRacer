using System.Collections;
using Unity.Profiling;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    public GameObject[] CarBodies;
    public WheelCollider frontRight;
    public WheelCollider frontLeft;
    public WheelCollider backRight;
    public WheelCollider backLeft;

    public Transform frontRightTransform;
    public Transform frontLeftTransform;
    public Transform backRightTransform;
    public Transform backLeftTransform;

    public float acceleration = 1000f;
    public float breakingForce = 300f;
    public float maxTurnAngle = 50f;
    public float RaceProgress;
    public AudioClip crashClip;
    public float spinOutForce = 100f;
    public float boostForce = 1500f;
    public float startingAcceleration = 250f;

    private float currentAcceleration = 0f;
    private float currentBreakingForce = 0f;
    private float currentTurnAngle = 0f;

    private Rigidbody rb;
    private GameController gameController;
    private AudioSource audioSource;
    private float gasInput;
    private float steeringInput;
    private bool brakesApplied;
    private bool boostPressed;

    private float cooldownTimer;
    private float boostCooldown = 30f;

    void Start()
    {
        int carIndex = PlayerPrefs.GetInt("carIndex");
        
        int index = 0;
        foreach(var body in CarBodies)
        {
            body.SetActive(index++ == carIndex);;
        }

        rb = GetComponent<Rigidbody>();
        gameController = GameObject.Find("GameController").GetComponent<GameController>();   
        audioSource = GetComponent<AudioSource>();

        switch(carIndex)
        {
            case 0:
                gameController.UpgradeBoost1 = false;
                gameController.UpgradeBoost2 = false;
                gameController.UpgradeHandling1 = false;
                gameController.UpgradeHandling2 = false;
                gameController.UpgradeDurability1 = false;
                gameController.UpgradeDurability2 = false;
                break;
            case 1:
                gameController.UpgradeHandling1 = true;
                gameController.UpgradeHandling2 = false;
                gameController.UpgradeBoost1 = false;
                gameController.UpgradeBoost2 = false;
                gameController.UpgradeDurability1 = false;
                gameController.UpgradeDurability2 = false;
                break;
            case 2:
                gameController.UpgradeHandling1 = true;
                gameController.UpgradeHandling2 = false;
                gameController.UpgradeBoost1 = true;
                gameController.UpgradeBoost2 = false;
                gameController.UpgradeDurability1 = true;
                gameController.UpgradeDurability2 = false;
                break;
            case 3:
                gameController.UpgradeHandling1 = true;
                gameController.UpgradeHandling2 = true;
                gameController.UpgradeBoost1 = true;
                gameController.UpgradeBoost2 = false;
                gameController.UpgradeDurability1 = true;
                gameController.UpgradeDurability2 = false;
                break;
            case 4:
                gameController.UpgradeHandling1 = true;
                gameController.UpgradeHandling2 = true;
                gameController.UpgradeBoost1 = true;
                gameController.UpgradeBoost2 = true;
                gameController.UpgradeDurability1 = true;
                gameController.UpgradeDurability2 = false;
                break;
            case 5:
                gameController.UpgradeHandling1 = true;
                gameController.UpgradeHandling2 = true;
                gameController.UpgradeBoost1 = true;
                gameController.UpgradeBoost2 = true;
                gameController.UpgradeDurability1 = true;
                gameController.UpgradeDurability2 = true;
                break;
        }

        if(gameController.UpgradeHandling2)
        {
            SetSidewaysFriction(frontLeft, 2f);
            SetSidewaysFriction(frontRight, 2f);
            SetSidewaysFriction(backLeft, 2f);
            SetSidewaysFriction(backRight, 2f);

            SetForwarFriction(frontLeft, 2f);
            SetForwarFriction(frontRight, 2f);
            SetForwarFriction(backLeft, 2f);
            SetForwarFriction(backRight, 2f);
        }
        else if(gameController.UpgradeHandling1)
        {
            SetSidewaysFriction(frontLeft, 1f);
            SetSidewaysFriction(frontRight, 1f);
            SetSidewaysFriction(backLeft, 1f);
            SetSidewaysFriction(backRight, 1f);

            SetForwarFriction(frontLeft, 1f);
            SetForwarFriction(frontRight, 1f);
            SetForwarFriction(backLeft, 1f);
            SetForwarFriction(backRight, 1f);
        }
        else
        {
            SetSidewaysFriction(frontLeft, .5f);
            SetSidewaysFriction(frontRight, .5f);
            SetSidewaysFriction(backLeft, .5f);
            SetSidewaysFriction(backRight, .5f);

            SetForwarFriction(frontLeft, .5f);
            SetForwarFriction(frontRight, .5f);
            SetForwarFriction(backLeft, .5f);
            SetForwarFriction(backRight, .5f);
        }

        boostCooldown = 30f;

        if(gameController.UpgradeBoost1)
        {
            boostCooldown = 10f;
        }

        if(gameController.UpgradeBoost2)
        {
            boostCooldown = 5f;
        }
      
        enabled = false;
    }

    private void SetSidewaysFriction(WheelCollider wheelCollider, float stiffness)
    {
        WheelFrictionCurve friction = wheelCollider.sidewaysFriction;
        friction.stiffness = stiffness;
        wheelCollider.sidewaysFriction = friction;
    }

    private void SetForwarFriction(WheelCollider wheelCollider, float stiffness)
    {
        WheelFrictionCurve friction = wheelCollider.forwardFriction;
        friction.stiffness = stiffness;
        wheelCollider.forwardFriction = friction;
    }


    private void FixedUpdate()
    {
        cooldownTimer -= Time.deltaTime;
        gameController.BoostButton.SetActive(cooldownTimer <= 0);

        BoostStartinAcceleration();
        BoostCar();

        currentAcceleration = (Input.GetAxis("Vertical") + gasInput) * acceleration;

        if(Input.GetKey(KeyCode.Space) || brakesApplied)
        {
            currentBreakingForce = breakingForce;
        }
        else
        {
            currentBreakingForce = 0f;
        }

        currentTurnAngle = (Input.GetAxis("Horizontal") + steeringInput) * maxTurnAngle;

        frontLeft.steerAngle = currentTurnAngle;
        frontRight.steerAngle = currentTurnAngle;

        frontRight.motorTorque = currentAcceleration;
        frontLeft.motorTorque = currentAcceleration;

        frontRight.brakeTorque = currentBreakingForce;
        frontLeft.brakeTorque = currentBreakingForce;
        backRight.brakeTorque = currentBreakingForce;
        backLeft.brakeTorque = currentBreakingForce;

        UpdateWheelMesh(frontRight, frontRightTransform);
        UpdateWheelMesh(frontLeft, frontLeftTransform);
        UpdateWheelMesh(backRight, backRightTransform);
        UpdateWheelMesh(backLeft, backLeftTransform);

        int waypoint = gameController.ClosestWaypointToPlayer() + 1;
        float totalWaypoints = gameController.waypoints.Length * gameController.MaxLaps; 
        float currentWaypoint = (gameController.currentLap - 1) * gameController.waypoints.Length + waypoint;
        RaceProgress = (currentWaypoint/totalWaypoints)*100;
    }   

    void BoostStartinAcceleration()
    {
        if(rb.linearVelocity.magnitude < 10f && Input.GetAxis("Vertical") > 0)
        {
            if(gameController.UpgradeBoost2)
            {
                rb.AddForce(transform.forward * (startingAcceleration * 4), ForceMode.Impulse);
            }
            else if(gameController.UpgradeBoost1)
            {
                rb.AddForce(transform.forward * (startingAcceleration * 2), ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(transform.forward * startingAcceleration, ForceMode.Impulse);
            }
        }
    }

    public void BoostCar()
    {
        if(!boostPressed) return;
        cooldownTimer = boostCooldown;
        gameController.BoostButton.SetActive(cooldownTimer <= 0);

        if(gameController.UpgradeBoost2)
        {
            rb.AddForce(transform.forward * boostForce, ForceMode.Impulse);
        }
        else if(gameController.UpgradeBoost1)
        {
            rb.AddForce(transform.forward * (boostForce / 2), ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(transform.forward * (boostForce / 4), ForceMode.Impulse);
        }

        boostPressed = false;
    }

    void UpdateWheelMesh(WheelCollider collider, Transform transform)
    {
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
        transform.position = position;
        transform.rotation = rotation;
    }

    private void OnCollisionEnter(Collision other) {
        audioSource.PlayOneShot(crashClip);

        if(gameController.UpgradeDurability2)
        {
            // No impact force
        } 
        else if(gameController.UpgradeDurability1)
        {
            GetComponent<Rigidbody>().AddForce(-1 * transform.forward * (spinOutForce / 1.5f), ForceMode.Impulse);
        }
        else
        {
            GetComponent<Rigidbody>().AddForce(-1 * transform.forward * spinOutForce, ForceMode.Impulse);
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
