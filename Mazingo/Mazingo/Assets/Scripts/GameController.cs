using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController instance;

    public float Tick = 0.5f;
    private float timer;
    Assets.Scripts.Engine.RoomController rc;

    GameObject currentRoom;


	// Use this for initialization
	void Start () {
        instance = this;
        rc = new Assets.Scripts.Engine.RoomController();
        if (Input.GetKeyDown(KeyCode.K))
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().Kill();
        }
	}
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;
        if(timer>=Tick)
        {
            Debug.Log("Loading..");
            timer = timer % Tick;
            rc.Load();
        }
	}

    public void SetCurrentRoom(GameObject room)
    {
        currentRoom = room;
    }
}
