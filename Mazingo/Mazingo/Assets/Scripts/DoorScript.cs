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
        if (openDoor && transform.position != openPosition.position)
            movingTime += Time.deltaTime;
        else if (closeDoor && transform.position != openPosition.position)
            movingTime -= Time.deltaTime;
        if (movingTime > 1)
            movingTime = 1;
        if (movingTime < 0)
            movingTime = 0;
        if(movingTime>0 && movingTime<1)
            transform.position = Vector3.Lerp(closedPosition, openPosition.position, movingTime);
    }


    private void OnTriggerEnter(Collider collision)
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

    private void OnTriggerExit(Collider collision)
    {
        Debug.Log("exit");
        if (collision.gameObject.tag == "Player")
        {
            closeDoor = true;
            openDoor = false;
        }
    }
}
