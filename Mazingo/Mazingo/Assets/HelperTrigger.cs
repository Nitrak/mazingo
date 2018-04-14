using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperTrigger : MonoBehaviour {

    public DoorScript helpObject;

    private void OnTriggerEnter(Collider collision)
    {
        helpObject.DoorEnter(collision);
    }

    private void OnTriggerExit(Collider collision)
    {
        helpObject.DoorExit(collision);
    }

}
