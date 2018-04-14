using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillScript : MonoBehaviour {

    PlayerController player;

    LayerMask objectMask = LayerMask.GetMask("Objects");

    // Use this for initialization
    void Start () {
        player = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetComponent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag=="Player")
            player.Kill();
        else { 
            var controller = other.GetComponent<BombController>();
            if(controller != null)
            {
                controller.Explode();
            }
        }
    }
}
