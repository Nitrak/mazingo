using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableDoorScript : MonoBehaviour
{
    private float explosionTime = -1;

    public GameController controller;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (explosionTime > 0)
            explosionTime -= Time.deltaTime;
        if (explosionTime > -1 && explosionTime < 0)
            ExplodeDoor();
    }

    private void ExplodeDoor()
    {
        gameObject.SetActive(false);
        controller.LevelComplete();
    }

    private void OnTriggerEnter(Collider other)
    {
        var bomb = other.gameObject.GetComponent<BombController>();
        if (bomb != null)
        {
            explosionTime = bomb.StartBomb();
        }
    }

}
