using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour,IInteractable
{
    [Header("Vehicle settings")]
    [Tooltip("In meters")]
    public float wheelbase;
    public float reartrack;
    public float turnradius;

    [Header("Asignables")]
    public Wheel[] wheels;

    [Header("Seats")]
    public Transform driver_seat;
    public Transform Get_out_Driver_point;
    public Transform[] passanger_seat;

    private float ackAnglL;
    private float ackAnglR;
    private bool drived;

    private Transform player_obj;


    void Update()
    {
        if (drived)
        {
            float steerinput = Input.GetAxis("Horizontal");
            if (steerinput > 0)//right
            {
                ackAnglL = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnradius + (reartrack / 2))) * steerinput;
                ackAnglR = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnradius - (reartrack / 2))) * steerinput;
            }
            else if (steerinput < 0)//left
            {
                ackAnglR = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnradius + (reartrack / 2))) * steerinput;
                ackAnglL = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnradius - (reartrack / 2))) * steerinput;
            }
            else //straight
            {
                ackAnglL = 0;
                ackAnglR = 0;
            }

            foreach (Wheel wheel in wheels)
            {
                if (wheel.side == 0 && wheel.steering && !wheel.inverted_steering)
                {
                    wheel.ackAngl = ackAnglL;
                }
                else if (wheel.side == 1 && wheel.steering && !wheel.inverted_steering)
                {
                    wheel.ackAngl = ackAnglR;
                }
                else if (wheel.side == 0 && wheel.steering && wheel.inverted_steering)
                {
                    wheel.ackAngl = -ackAnglL;
                }
                else if (wheel.side == 1 && wheel.steering && wheel.inverted_steering)
                {
                    wheel.ackAngl = -ackAnglR;
                }
            }

            player_obj.transform.position = driver_seat.position;
            player_obj.transform.rotation = driver_seat.rotation;

            if(Input.GetKeyDown(KeyCode.E))
            {
                player_obj.transform.position = Get_out_Driver_point.position; 
                player_obj.GetComponent<Rigidbody>().isKinematic = false;
                player_obj.GetComponent<CapsuleCollider>().enabled = true;
                GetComponent<BoxCollider>().enabled = true;
                GetComponent<BoxCollider>().isTrigger = true;
                drived = false;
                foreach (Wheel wheel in wheels)
                    wheel.controlable = false;
            }
        }
    }

    //Interaction
    public float HoldDuration => throw new System.NotImplementedException();

    public bool HoldInteract => throw new System.NotImplementedException();

    public bool is_Ineractable { get => true; set => throw new System.NotImplementedException(); }

    public void OnEndHover()
    {
        print("Looked Away");
    }

    public void OnInteract()
    {
        if(is_Ineractable)
        {
            FpsController player = FindObjectOfType<FpsController>();
            player.transform.GetComponent<Rigidbody>().isKinematic = true;
            player_obj = player.transform;
            player_obj.GetComponent<CapsuleCollider>().enabled = false;
            drived = true;
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<BoxCollider>().isTrigger = false;
            foreach (Wheel wheel in wheels)
                wheel.controlable = true;
        }
    }

    public void OnStartHover()
    {
        Debug.Log("Looking at a Vehicle");
    }

}
