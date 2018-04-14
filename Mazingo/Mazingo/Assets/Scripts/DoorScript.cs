using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{

    public float openTime = 2;
    public bool requireKey = false;
    public GameObject key;
    public Transform openPosition;


    private Vector3 closedPosition;
    private bool hasKey;
    private float movingTime;

    private bool openDoor;
    private bool closeDoor;


    // Use this for initialization
    void Start()
    {
        closedPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        if (openDoor)
            movingTime = Mathf.Min(1, movingTime + Time.deltaTime);
        else if (closeDoor)
            movingTime = Mathf.Max(0, movingTime - Time.deltaTime);



        if (movingTime > 0 && movingTime < 1)
        {
            transform.position = Vector3.Lerp(closedPosition, openPosition.position, movingTime);
        }
    }

    public void DoorEnter(Collider collision)
    {
        Debug.Log("enter");

        if (collision.gameObject.tag == "Player")
        {
            if ((requireKey && hasKey) || !requireKey)
            {
                closeDoor = false;
                openDoor = true;
            }
        }
    }

    public void DoorExit(Collider collision)
    {
        Debug.Log("exit");
        if (collision.gameObject.tag == "Player")
        {
            closeDoor = true;
            openDoor = false;
        }
    }
}
