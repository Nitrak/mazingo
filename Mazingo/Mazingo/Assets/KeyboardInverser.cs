using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class KeyboardInverser : MonoBehaviour {


    private FirstPersonController playerMoveSettings;

    // Use this for initialization
    void Start () {
        var p = GameObject.FindGameObjectWithTag("Player");
        playerMoveSettings = p.GetComponent<FirstPersonController>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerMoveSettings.RandomizeControls();
            Debug.Log("HER");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
            playerMoveSettings.NormalizeControls();
    }

}
