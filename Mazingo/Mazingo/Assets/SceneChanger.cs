using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void StartDemo()
    {
        SceneManager.LoadScene("TutorialScene");
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}
