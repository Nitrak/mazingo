using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour {
    public class CarriedEventArgs : EventArgs
    {
        private Rigidbody item;
        public CarriedEventArgs(Rigidbody i)
        {
            this.item = i;
        }

        public Rigidbody Item
        {
            get
            {
                return item;
            }
        }
    }

    private Rigidbody CarriedObject;
    private Rigidbody player;

    private Quaternion CarriedInitialRotation;
    private Transform CarriedInitialParent;
    private LayerMask objectMask;
    private Timer pickupGraceTimer;
    private Timer spawnGraceTimer;

    private Vector3 spawnPosition;
    private Quaternion cameraSpawnRotation;

    private bool interactPressed = false;
    private bool pickupGracePeriod;
    private bool spawnGracePeriod;
    private bool CarryingItem;
    private float carriedObjectAngularDrag;

    public Text actionText;
    public GameObject deathScreen;
    public KeyCode interactKey = KeyCode.E;
    public int pickupGraceDelay = 200;
    public int spawnGraceDelay = 2000;
    public float carryingDistance = 2f;
    public float rayLength = 5f;

    public delegate void CarryEventHandler(object sender, CarriedEventArgs e);
    public event CarryEventHandler OnPickedUp;
    public event CarryEventHandler OnDropped;



    // Use this for initialization
    void Start () {
        this.CarryingItem = false;
        this.CarriedObject = null;
        this.pickupGracePeriod = false;
        this.objectMask = LayerMask.GetMask("Objects");

        this.player = this.transform.parent.GetComponent<Rigidbody>();

        cameraSpawnRotation = player.transform.rotation;
        spawnPosition = player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        recordPlayerInput();
        if (!CarryingItem && !pickupGracePeriod)
        {
            castRay();  
        }
        else
        {
            if ((interactPressed && !pickupGracePeriod) || (CarriedObject != null  && CarriedObject.constraints == RigidbodyConstraints.FreezeAll))
            {
                //Debug.Log("dropping item");
                dropItem();
            }
            else if(CarryingItem)
            {
                updateCarriedItemPosition();
            }
        }
    }

    public void Kill() {
        if(!spawnGracePeriod)
            StartCoroutine(Respawn());
    }
    
    private IEnumerator Respawn()
    {
        startSpawnGracePeriod();
        yield return new WaitForSeconds(1);
        deathScreen.SetActive(true);
        Debug.Log("rip");
        player.velocity = Vector3.zero;
        this.transform.parent.rotation = Quaternion.identity;
        this.transform.rotation = Quaternion.identity;
        this.transform.parent.position = spawnPosition;
        //player.position = spawnPosition;

        yield return new WaitForSeconds(2);
        deathScreen.SetActive(false);
    }

    public void dropItem() {
        startPickupGracePeriod();
        
        this.CarryingItem = false;
        //Debug.Log("dropped item");
        //this.CarriedObject.transform.parent = this.CarriedInitialParent;
        this.CarriedObject.angularDrag = carriedObjectAngularDrag;
        this.CarriedObject.useGravity = true;
        var dropped = CarriedObject;
        this.CarriedObject = null;
        if(OnDropped != null)
            OnDropped(this, new CarriedEventArgs(dropped));
    }

    public void pickItemUp(Rigidbody item)
    {
        startPickupGracePeriod();
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
        updateCarriedItemPosition();
        if(OnPickedUp != null)
            OnPickedUp(this, new CarriedEventArgs(item));
    }

    private void startPickupGracePeriod()
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

    private void startSpawnGracePeriod()
    {
        spawnGracePeriod = true;
        spawnGraceTimer = new Timer((obj) =>
        {
            //Debug.Log("Grace period over");
            spawnGracePeriod = false;
            spawnGraceTimer.Dispose();
        },
                    null, spawnGraceDelay, Timeout.Infinite);
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
                    actionText.text = "";
                }
                else if (!CarryingItem)
                {
                    actionText.text = String.Format("Press <{0}> to interact", interactKey.ToString());
                }
                
            }
            else
            {
            }
        }
        else
        {
            actionText.text = "";
        }
    }

    private void updateCarriedItemPosition()
    {
        this.CarriedObject.MovePosition(this.transform.position + (this.transform.rotation * (Vector3.forward * carryingDistance)));
    }

    private void recordPlayerInput()
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

    private Quaternion lookAt(Vector3 sourcePoint, Vector3 destPoint)
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
