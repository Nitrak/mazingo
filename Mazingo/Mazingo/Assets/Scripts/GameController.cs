using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public float Tick = 0.5f;
    private float timer;
    Assets.Scripts.Engine.RoomController rc;


	// Use this for initialization
	void Start () {
        rc = new Assets.Scripts.Engine.RoomController();
        if (Input.GetKeyDown(KeyCode.K))
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().Kill();
        }
	}
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;
        if(timer<=Tick)
        {
            timer = timer % Tick;
            rc.Load();
        }
	}
}
