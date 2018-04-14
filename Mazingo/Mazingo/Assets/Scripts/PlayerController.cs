﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour {
    private List<object> Inventory;

    private Rigidbody CarriedObject;

    private bool interactPressed = false;

    private bool pickupGracePeriod;
    private Transform CarriedInitialParent;
    private bool CarryingItem;
    private Timer pickupGraceTimer;
    private LayerMask objectMask;
    private float carriedObjectAngularDrag;

    public float rayLength = 5f;
    public KeyCode interactKey = KeyCode.E;
    public float carryingDistance = 2f;
    
    public int pickupGraceDelay = 200;
    private Quaternion CarriedInitialRotation;

    // Use this for initialization
    void Start () {
        this.Inventory = new List<object>();
        this.CarryingItem = false;
        this.CarriedObject = null;
        this.pickupGracePeriod = false;
        this.objectMask = LayerMask.GetMask("Objects");
	}
 
    // Update is called once per frame
    void Update()
    {
        RecordPlayerInput();
        if (!CarryingItem && !pickupGracePeriod)
        {
            castRay();  
        }
        else
        {
            if (interactPressed && !pickupGracePeriod)
            {
                //Debug.Log("dropping item");
                dropItem();
            }
            else
            {
                UpdateCarriedItemPosition();
            }
        }
    }

    private void dropItem() {
        startGracePeriod();

        this.CarryingItem = false;
        //Debug.Log("dropped item");
        //this.CarriedObject.transform.parent = this.CarriedInitialParent;
        this.CarriedObject.angularDrag = carriedObjectAngularDrag;
        this.CarriedObject.useGravity = true;
        this.CarriedObject = null;
    }

    private void pickItemUp(Rigidbody item)
    {
        startGracePeriod();
        CarryingItem = true;
        CarriedObject = item;
        //Debug.Log("Picked item up");
        this.CarriedObject = item;
        this.CarriedInitialParent = item.transform.parent;
        this.CarriedInitialRotation = item.transform.rotation;
        this.CarriedObject.useGravity = false;
        carriedObjectAngularDrag = CarriedObject.angularDrag;
        this.CarriedObject.angularDrag = 100;
        //this.CarriedObject.transform.parent = this.transform;
        UpdateCarriedItemPosition();
    }

    private void startGracePeriod()
    {
        pickupGracePeriod = true;
        pickupGraceTimer = new Timer((obj) =>
        {
            //Debug.Log("Grace period over");
            pickupGracePeriod = false;
            pickupGraceTimer.Dispose();
        },
                    null, pickupGraceDelay, Timeout.Infinite);
    }
 
    private void castRay()
    {
        RaycastHit ray;
        //Debug.Log("cast");
        bool hit = Physics.Raycast(transform.position, transform.rotation * Vector3.forward, out ray, rayLength, objectMask);
        //Debug.DrawRay(transform.position, transform.rotation * Vector3.forward, Color.red, rayLength);
        if (hit)
        {
            //Debug.Log("hit");
            //interaction
            var d = ray.collider.gameObject.GetComponent<Rigidbody>();
            if (d != null)
            {
                if (interactPressed && !CarryingItem)
                {
                    pickItemUp(d);
                }
            }
            else
            {
            }
        }
        else
        {
        }
    }

    private void UpdateCarriedItemPosition()
    {
        this.CarriedObject.MovePosition(this.transform.position + (this.transform.rotation * (Vector3.forward * carryingDistance)));
    }

    private void RecordPlayerInput()
    {
        if (Input.GetKeyDown(interactKey))
        {
            interactPressed = true;
        }
        if (Input.GetKeyUp(interactKey))
        {
            interactPressed = false;
        }
    }

    private Quaternion LookAt(Vector3 sourcePoint, Vector3 destPoint)
    {
        destPoint = new Vector3(destPoint.x, destPoint.y, destPoint.z);
        sourcePoint = new Vector3(sourcePoint.x, sourcePoint.y, sourcePoint.z);
        Vector3 forwardVector = Vector3.Normalize(destPoint - sourcePoint);

        Vector3 rotAxis = Vector3.Cross(Vector3.forward, forwardVector);
        float dot = Vector3.Dot(Vector3.forward, forwardVector);

        Quaternion q;
        q.x = rotAxis.x;
        q.y = rotAxis.y;
        q.z = rotAxis.z;
        q.w = dot + 1;

        return q;
    }
}
