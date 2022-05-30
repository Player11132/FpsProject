using System;
using UnityEngine;

//Made using Dani fps movement tutorial, with some additions to fit me

public class FpsController : MonoBehaviour
{

    [Header("Asignables")]
    [SerializeField]private Transform playerCam;
    [SerializeField]private Transform orientation;
    [SerializeField]private Transform Headposition;
    [Tooltip("Not necesary")]
    [SerializeField] private GunManager gunManager;

    //Other
    private Rigidbody rb;

    [Header("Sensitivity")]
    private float xRotation;
    public float sensitivity = 50f;
    private float sensMultiplier = 1f;

    [Header("Movement")]
    public float moveSpeed = 4500;
    public float maxSpeed = 20;
    public float sprintspeed=3000;
    public bool grounded;
    public LayerMask whatIsGround;

    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    [Tooltip("Crouch and Slide")]
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float crouchspeed;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;

    [Tooltip("Jumping")]
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;
    
    [Header("Parkour")]

    [Header("Detection")]
    //I could use a list/dictionary but I think I would complicate too much
    public Detection Climb;
    public Detection checkClimb;
    public Detection Vault;
    public Detection checkVault;
    [Header("The actual Movement")]
    public Vector2 offset= new Vector2(0.7f,0.3f);

    public bool IsParkour;
    private float chosenParkourMoveTime;

    public AnimationCurve parkour_aceleration;

    [Tooltip("Vaulting")]
    private bool CanVault;
    public float VaultTime; //how long the vault takes
    public Transform VaultEndPoint;
    [Tooltip("Climbing")]
    private bool CanClimb;
    public float ClimbTime; //how long the climb takes
    public Transform endPosition;

    private float curve_time=0;
    private float parkour_speed=0;
    private Vector3 RecordedMoveToPosition; //the position of the vault end point in world space to move the player to
    private Vector3 RecordedStartPosition; // position of player right before vault

    //Input
    float x, y;
    bool jumping, sprinting, crouching;

    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;
    private Vector3 oldvelocity;

    private Vector3 startpos;
    private RaycastHit vaultHit;
    private RaycastHit climbHit;
    private bool checkEnviorment=true;

    [System.Serializable] //Trick to show structs in inspectors!
    public struct Detection {
        public float lenght;
        public Transform origin;
        public LayerMask layer;
        public bool intersection;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        playerScale = transform.localScale;
        playerScale.y = GetComponent<CapsuleCollider>().height;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void Update()
    {
        MyInput();
        Look();
        //Move camera
        playerCam.position = Headposition.position;
        CheckEnviorment();

      if(CanVault&&jumping)
        {

            oldvelocity = rb.velocity/2f;
            checkEnviorment = false;
            rb.isKinematic = true;
            //calculate the endpoint
            if (vaultHit.transform.InverseTransformPoint(transform.position).z < 0)
                RecordedMoveToPosition = vaultHit.point + new Vector3(0, vaultHit.transform.localScale.y / 2 + offset.y, offset.x);
            else
                RecordedMoveToPosition = vaultHit.point + new Vector3(0, vaultHit.transform.localScale.y / 2 + offset.y, -offset.x);
            endPosition.position = RecordedMoveToPosition;
            RecordedStartPosition = transform.position;
            IsParkour = true;
            chosenParkourMoveTime = VaultTime;
        }
      else if (CanClimb&&jumping)
        {

            oldvelocity = rb.velocity/2;
            checkEnviorment = false;
            rb.isKinematic = true;
            //calculate the endpoint
            if (climbHit.transform.InverseTransformPoint(transform.position).z < 0)
                RecordedMoveToPosition = climbHit.point + new Vector3(0, climbHit.transform.localScale.y / 2 + offset.y, offset.x);
            else
                RecordedMoveToPosition = climbHit.point + new Vector3(0, climbHit.transform.localScale.y / 2 + offset.y, -offset.x);
            
            endPosition.position = RecordedMoveToPosition;
            RecordedStartPosition = transform.position;
            IsParkour = true;
            chosenParkourMoveTime = ClimbTime;
        }
    }

    /// Find user input. Should put this in its own class but im lazy
    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
        crouching = Input.GetKey(KeyCode.LeftControl);
        sprinting = Input.GetKey(KeyCode.LeftShift);

        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch();
        if (Input.GetKeyUp(KeyCode.LeftControl))
            StopCrouch();
    }

    private void CheckEnviorment()
    {
        Vault.intersection = Physics.Raycast(Vault.origin.position, Vault.origin.forward,out vaultHit, Vault.lenght, Vault.layer, QueryTriggerInteraction.Ignore);
        checkVault.intersection = Physics.Raycast(checkVault.origin.position, checkVault.origin.forward, checkVault.lenght, checkVault.layer, QueryTriggerInteraction.Ignore);
        if (Vault.intersection && !checkVault.intersection&&!IsParkour&&checkEnviorment)
            CanVault = true;
        else
            CanVault = false;

        Climb.intersection = Physics.Raycast(Climb.origin.position, Climb.origin.forward,out climbHit, Climb.lenght, Climb.layer, QueryTriggerInteraction.Ignore);
        checkVault.intersection = Physics.Raycast(checkClimb.origin.position, checkClimb.origin.forward, checkClimb.lenght, checkClimb.layer, QueryTriggerInteraction.Ignore);
        if (Climb.intersection && !checkClimb.intersection&&!IsParkour&&checkEnviorment)
            CanClimb = true;
        else
            CanClimb = false;

    }
  
    private void StartCrouch()
    {
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        if (rb.velocity.magnitude > 0.5f)
        {
            if (grounded)
            {
                rb.AddForce(orientation.transform.forward * slideForce);
            }
        }
    }


    private void StopCrouch()
    {
        transform.GetComponent<CapsuleCollider>().height = playerScale.y;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    private void Movement()
    {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

        //If holding jump && ready to jump, then jump
        if (readyToJump && jumping && !IsParkour && !rb.isKinematic) Jump();
        if (readyToJump)
        {
            startpos = transform.position;
        }

        if (IsParkour && curve_time < 1f)
        {
            parkour_speed = parkour_aceleration.Evaluate(curve_time);
            curve_time += Time.deltaTime / chosenParkourMoveTime;
            transform.position = Vector3.Lerp(RecordedStartPosition, RecordedMoveToPosition, parkour_speed);

            if (curve_time>= 1f)
            {
                IsParkour = false;
                curve_time = 0;
                rb.isKinematic = false;
                checkEnviorment = true;
                rb.velocity = oldvelocity;
                Debug.Log(oldvelocity);
            }

        }
            //Set max speed
            float maxSpeed = this.maxSpeed;

            //If sliding down a ramp, add force down so player stays grounded and also builds speed
            if (crouching && grounded && readyToJump)
            {
                rb.AddForce(Vector3.down * Time.deltaTime * 3000);
                return;
            }

            //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
            if (x > 0 && xMag > maxSpeed) x = 0;
            if (x < 0 && xMag < -maxSpeed) x = 0;
            if (y > 0 && yMag > maxSpeed) y = 0;
            if (y < 0 && yMag < -maxSpeed) y = 0;

            //Some multipliers
            float multiplier = 1f, multiplierV = 1f;

            // Movement in air
            if (!grounded)
            {
                multiplier = 0.5f;
                multiplierV = 0.5f;
            }

            //Decide on speed
            float curSpeed = moveSpeed;

            if (sprinting)
                curSpeed = sprintspeed;
            if (crouching)
                curSpeed = crouchspeed;

            // Movement while sliding
            if (grounded && crouching) multiplierV = 0f;

            //Apply forces to move player
            rb.AddForce(orientation.transform.forward * y * curSpeed * Time.deltaTime * multiplier * multiplierV);
            rb.AddForce(orientation.transform.right * x * curSpeed * Time.deltaTime * multiplier);

            gunManager.speed = curSpeed * y;
    }

    private void Jump()
    {
        if (grounded && readyToJump)
        {
            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private float desiredX;
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) return;

        //Slow down sliding
        if (crouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }


    private void StopGrounded()
    {
        grounded = false;
    }


}
