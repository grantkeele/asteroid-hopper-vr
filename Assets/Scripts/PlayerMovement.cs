using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    World world;
    FeetCollision feetColl;
    AsteroidGeneration asteroidGeneration;

    float midLength;
    float midWidth;
    float midHeight;

    Rigidbody rb;
    CapsuleCollider coll;

    [SerializeField] private GameObject body;
    [SerializeField] private Transform backpackAnchor;
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private GameObject rightHandAnchor;
    [SerializeField] private GameObject leftHandAnchor;
    GameObject primaryHand;
    GameObject secondaryHand;

    Ray floorRay;

    public float lookSensitivity = 3.0f;

    public float latMoveForce;
    public float vertMoveForce;
    public float torque;

    public float gravity;

    public bool grounded = false;
    public float groundSpeed;
    public float maxDeltaV;
    public float jumpForce;

    float xVel = 0;
    float yVel = 0;
    float zVel = 0;

    Vector2 rightStickInput;

    private Vector3 moveDirection = Vector3.zero;

    public float rotationSpeed;

    public Vector3 gravityPoint;
    Vector3 gravityDirection;

    Vector3 terrainNormal = Vector3.zero;

    bool orientDown = true;
    public int gravityMode = 0;


    void Start()
    {
        primaryHand = rightHandAnchor;
        secondaryHand = leftHandAnchor;

        world = GameObject.Find("World").GetComponent<World>();
        asteroidGeneration = GameObject.Find("World").GetComponent<AsteroidGeneration>();
        feetColl = GetComponentInChildren<FeetCollision>();

        midLength = world.polySize * world.chunkSize * asteroidGeneration.worldLength / 2;
        midWidth = world.polySize * world.chunkSize * asteroidGeneration.worldWidth / 2;
        midHeight = world.polySize * world.chunkSize * asteroidGeneration.worldHeight / 2;

        coll = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        gravityPoint = new Vector3(midLength, midWidth, midHeight);
        GameObject testSphere = GameObject.Find("testSphere");
        testSphere.transform.position = gravityPoint;

        //Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    void FixedUpdate()
    {
        if (orientDown) {
            // Rotate player towards gravitational field
            switch (gravityMode) {
                case 0:
                    gravityDirection = (gravityPoint - this.transform.position).normalized;
                    break;
                case 1:
                    gravityDirection = -terrainNormal;
                    break;
                default:
                    gravityDirection = (gravityPoint - this.transform.position).normalized;
                    break;
            }

            Quaternion gravityRotation = Quaternion.LookRotation(Vector3.Cross(gravityDirection, transform.right), -gravityDirection);

            if (grounded) {

                if (transform.rotation != gravityRotation) {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, gravityRotation, rotationSpeed * Time.deltaTime * 2);
                }

                moveDirection = Vector3.zero;

                Vector3 forwardSlopeDirection = secondaryHand.transform.forward - Vector3.Dot(secondaryHand.transform.forward, terrainNormal) * terrainNormal;
                Vector3 rightSlopeDirection = secondaryHand.transform.right - Vector3.Dot(secondaryHand.transform.right, terrainNormal) * terrainNormal;
                forwardSlopeDirection.Normalize();
                rightSlopeDirection.Normalize();

                xVel = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x * groundSpeed;
                zVel = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y * groundSpeed;

                moveDirection += forwardSlopeDirection * zVel;
                moveDirection += rightSlopeDirection * xVel;

                Vector3 velocityChange = moveDirection - rb.velocity;
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxDeltaV, maxDeltaV);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxDeltaV, maxDeltaV);
                velocityChange.y = Mathf.Clamp(velocityChange.y, -maxDeltaV, maxDeltaV);
                rb.AddForce(velocityChange, ForceMode.VelocityChange);

                if (Input.GetButton("Jump")) {
                    //rb.velocity = new Vector3(moveDirection.x, jumpForce / rb.mass, moveDirection.z);
                    rb.velocity += -gravityDirection * jumpForce / rb.mass;
                }
            }
            // If not grounded
            else {
                if (transform.rotation != gravityRotation) {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, gravityRotation, rotationSpeed * Time.deltaTime);
                }

                moveDirection = Vector3.zero;

                xVel = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x * latMoveForce;
                zVel = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y * latMoveForce;
                yVel = 0;

                moveDirection += body.transform.forward * zVel;
                moveDirection += body.transform.right * xVel;
                moveDirection += body.transform.up * yVel;
                rb.AddForce(moveDirection);
            }

            // Apply gravity
            rb.AddForce(gravityDirection * gravity * rb.mass);

            
        }

        // If the player is not orienting towards gravity
        else {
            moveDirection = Vector3.zero;

            xVel = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x * latMoveForce;
            zVel = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y * latMoveForce;
            yVel = 0;
            
            if (OVRInput.Get(OVRInput.Button.Three)) {
                yVel = -vertMoveForce;
            }
            if (OVRInput.Get(OVRInput.Button.Four)) {
                yVel = vertMoveForce;
            }

            moveDirection += body.transform.forward * zVel;
            moveDirection += body.transform.right * xVel;
            moveDirection += body.transform.up * yVel;
            rb.AddForce(moveDirection);
        }


        rightStickInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        rb.AddTorque(body.transform.up * -torque * rightStickInput.x);
        rb.AddTorque(body.transform.right * -torque * rightStickInput.y);

        if(OVRInput.Get(OVRInput.Button.One)){
            rb.AddTorque(body.transform.forward * torque);
        }
        if(OVRInput.Get(OVRInput.Button.Two)){
            rb.AddTorque(body.transform.forward * -torque);
        }


        grounded = false;
    }

    void Update() {   

        // Rotate backpack with body
        body.transform.localEulerAngles = new Vector3(0f, cameraTransform.localEulerAngles.y, 0f);
        
        backpackAnchor.position = cameraTransform.position;
        //backpackAnchor.localEulerAngles = new Vector3(0f, cameraTransform.localEulerAngles.y, 0f);

        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick)) {
            orientDown = !orientDown;
        }
    }

    private void OnCollisionStay(Collision collision) {
        terrainNormal = collision.GetContact(0).normal;
        grounded = true;
    }
}
