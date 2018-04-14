using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVTextScript : MonoBehaviour {

    TextMesh text;

	// Use this for initialization
	void Start () {
        text = GetComponent<TextMesh>();
        text.text = "WELCOME TESTSUBJECT \n #" +Mathf.Floor(Random.Range(402017,754392));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
