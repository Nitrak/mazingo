using Assets.Scripts.Engine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController instance;

    public float Tick = 0.5f;
    private float timer;
    private int currentLevel;
    private RoomController roomController;
    private PlayerController playerController;


	// Use this for initialization
	void Start () {
        this.currentLevel = 0;
        instance = this;
        roomController = new Assets.Scripts.Engine.RoomController();
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        roomController.StartLevel(currentLevel);
	}

    //TODO: Callback here
    public void LevelComplete()
    {
        roomController.StartLevel(++currentLevel);
    }
	
	// Update is called once per frame
	void Update () {
        if (!roomController.IsLoaded()) return;

        if (Input.GetKeyDown(KeyCode.K))
        {
            playerController.Kill();
        }

        timer += Time.deltaTime;
        if(timer>=Tick)
        {
            //Debug.Log("Loading..");
            timer = timer % Tick;
            roomController.Load();
        }
	}

}
