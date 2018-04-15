using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
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
    private Image deathImage;

    private struct SpeedVars
    {
        public float WalkSpeed;
        public float RunSpeed;
        public float JumpForce;
    }

    private SpeedVars speedVars;
    private FirstPersonController playerMoveSettings;

    private Quaternion CarriedInitialRotation;
    private Transform CarriedInitialParent;
    private LayerMask objectMask;
    private Timer pickupGraceTimer;
    private Timer spawnGraceTimer;

    private Vector3 spawnPosition;
    private Quaternion cameraSpawnRotation;

    private bool interactPressed = false;
    private bool throwPressed = false;
    private bool pickupGracePeriod;
    private bool spawnGracePeriod;
    private bool CarryingItem;
    private float carriedObjectAngularDrag;



    public Text actionText;
    public Animator fadeAnimation;
    public GameObject deathScreen;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode throwKey = KeyCode.Mouse0;
    public int pickupGraceDelay = 200;
    public int spawnGraceDelay = 2000;
    public float carryingDistance = 2f;
    public float rayLength = 5f;

    public delegate void CarryEventHandler(object sender, CarriedEventArgs e);
    public delegate void EventHandler(object sender);
    public event CarryEventHandler OnPickedUp;
    public event CarryEventHandler OnDropped;
    public event CarryEventHandler OnThrown;
    public event EventHandler OnDeath;



    // Use this for initialization
    void Start () {
        this.CarryingItem = false;
        this.CarriedObject = null;
        this.pickupGracePeriod = false;
        this.objectMask = LayerMask.GetMask("Objects");

        this.player = this.transform.parent.GetComponent<Rigidbody>();
        this.deathImage = deathScreen.GetComponent<Image>();

        this.cameraSpawnRotation = player.transform.rotation;
        this.spawnPosition = player.transform.position;
        playerMoveSettings = this.transform.parent.GetComponent<FirstPersonController>();
        speedVars = new SpeedVars
        {
            WalkSpeed = playerMoveSettings.GetMovementSpeed(),
            RunSpeed = playerMoveSettings.GetRunSpeed(),
            JumpForce = playerMoveSettings.GetJumpSpeed()
        };
    }

    private void FixedUpdate()
    {
        if (!CarryingItem && !pickupGracePeriod)
        {
            castRay();
        }
        else
        {
            if (CarryingItem)
            {
                updateCarriedItemPosition();
                if (!pickupGracePeriod)
                {
                    if (interactPressed || (CarriedObject != null && CarriedObject.constraints == RigidbodyConstraints.FreezeAll))
                    {
                        //Debug.Log("dropping item");
                        dropItem();
                    }
                    else if (throwPressed)
                    {
                        throwItem((transform.rotation * Vector3.forward)*300);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        recordPlayerInput();
    }

    public void SetSpawnPosition(Vector3 spawn)
    {
        this.spawnPosition = spawn;
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

    public void throwItem(Vector3 force)
    {
        startPickupGracePeriod();

        this.CarryingItem = false;
        //Debug.Log("dropped item");
        //this.CarriedObject.transform.parent = this.CarriedInitialParent;
        this.CarriedObject.angularDrag = carriedObjectAngularDrag;
        this.CarriedObject.useGravity = true;
        var dropped = CarriedObject;
        CarriedObject.AddForce(force);
        this.CarriedObject = null;
        if (OnDropped != null)
            OnDropped(this, new CarriedEventArgs(dropped));
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
 
    private void updateCarriedItemPosition()
    {
        this.CarriedObject.MovePosition(this.transform.position + (this.transform.rotation * (Vector3.forward * carryingDistance)));
    }

    private void recordPlayerInput()
    {
        if (Input.GetKeyDown(throwKey))
        {
            throwPressed = true;
        }
        if (Input.GetKeyDown(interactKey))
        {
            interactPressed = true;
        }
        if (Input.GetKeyUp(throwKey))
        {
            throwPressed = false;
        }
        if (Input.GetKeyUp(interactKey))
        {
            interactPressed = false;
        }
    }

    public void Kill() {
        if(!spawnGracePeriod)
            StartCoroutine(Respawn());
    }

    public bool HasBomb()
    {
        if(CarriedObject != null)
        {
            return CarriedObject.gameObject.GetComponent<BombController>() != null;
        }
        return false;
    }
    
    public IEnumerator Respawn()
    {
        if(OnDeath != null)
            OnDeath(this);
        Debug.Log("Spawning player");
        startSpawnGracePeriod();
        dropItem();

        playerMoveSettings.shouldTakeInput = false;
        yield return new WaitForSeconds(0.3f);
        
        deathScreen.SetActive(true);
        yield return FadeOut();
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

    IEnumerator FadeOut()
    {
        Debug.Log("fading!");
        fadeAnimation.SetBool("Fade", true);
        yield return new WaitUntil(() => deathImage.color.a >= 1f);
        Debug.Log("rip");
        player.velocity = Vector3.zero;
        this.transform.parent.rotation = Quaternion.identity;
        this.transform.rotation = Quaternion.identity;
        this.transform.parent.position = spawnPosition;
        fadeAnimation.SetBool("Fade", false);
        yield return new WaitUntil(() => deathImage.color.a <= 0f);
        deathScreen.SetActive(false);
        playerMoveSettings.shouldTakeInput = true;
    }
}
