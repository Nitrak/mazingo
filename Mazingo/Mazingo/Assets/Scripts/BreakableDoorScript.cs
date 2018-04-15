using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableDoorScript : MonoBehaviour
{
    private float explosionTime = -1;

    public delegate void EventHandler(object sender);
    public event EventHandler OnVictory;

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
        if(OnVictory != null)
        {
            OnVictory(this);
        }
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
