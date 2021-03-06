﻿using Assets.Scripts.Engine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public static GameController instance;
    public bool tutorial;

    public float Tick = 0.5f;
    private float timer;
    private int currentLevel;
    private RoomController roomController;
    private PlayerController playerController;
    // Use this for initialization

    void Start()
    {
        this.currentLevel = 0;
        instance = this;
        roomController = new Assets.Scripts.Engine.RoomController();
        playerController = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetComponent<PlayerController>();
        if (tutorial)
        {
            roomController.StartTutorial();
        }
        else
        {
            roomController.StartLevel(currentLevel);
        }
    }

    public RoomController GetRoomController()
    {
        return roomController;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    //TODO: Callback here
    public void LevelComplete()
    {
        if (tutorial)
        {
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            roomController.StartLevel(++currentLevel);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!roomController.IsLoaded()) return;

        if (Input.GetKeyDown(KeyCode.K))
        {
            playerController.Kill();
        }

        timer += Time.deltaTime;
        if (timer >= Tick)
        {
            //Debug.Log("Loading..");
            timer = timer % Tick;
            roomController.Load();
        }
    }

}
