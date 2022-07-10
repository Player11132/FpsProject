using System.Collections.Specialized;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    private Rigidbody rb;
    [Header("Settings")]

    [Header("Suspension")]
    public float restLength;
    public float springTravel;
    public float springStiffness;
    public float damperStiffness;

    private float minLength;
    private float maxLength;
    private float lastLength;
    private float springLength;
    private float springForce;
    private float damperForce;
    private float springVelocity;

    private Vector3 suspensionForce;

    [Header("Wheel")]
    public float wheelRadius;
    public GameObject WheelGraphic;

    [Header("Steering")]
    public bool steering;
    public bool inverted_steering;
    [Tooltip("0=left 1=right")]
    [Range(0, 1)]
    public int side;

    [HideInInspector]
    public float ackAngl;
    [HideInInspector]
    public bool controlable;

    private Vector3 WheelvelLS;
    private float Fx;
    private float Fy;

    void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();

        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;
    }

    private void Update()
    {
        float steeringangl = Mathf.Lerp(transform.localRotation.y, ackAngl, 8f);
        transform.localRotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y + steeringangl, transform.localRotation.z);
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + wheelRadius))
        {
            lastLength = springLength;

            springLength = hit.distance - wheelRadius;
            springLength = Mathf.Clamp(springLength, minLength, maxLength);
            springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;
            springForce = springStiffness * (restLength - springLength);
            damperForce = damperStiffness * springVelocity;

            suspensionForce = (springForce + damperForce) * transform.up;

            WheelGraphic.transform.localPosition = WheelGraphic.transform.localPosition.up * springLength

            WheelvelLS = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
            if (controlable)
            {
                Fx = Input.GetAxis("Vertical") * springForce;
                Fy = WheelvelLS.x * springForce;
            }
            else
            {
                Fx = 0;
                Fy = 0;
            }

            rb.AddForceAtPosition(suspensionForce+(Fx*transform.forward)+(Fy*-transform.right), hit.point);
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * -springLength);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.up * -springLength, wheelRadius);
    }
}


