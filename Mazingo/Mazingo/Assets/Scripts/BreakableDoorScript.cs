using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public Animator fadeAnimation;
    public GameObject victoryScreen;
    public Image victoryFade;

    private void ExplodeDoor()
    {
        StartCoroutine(FinishLevel());
    }

    IEnumerator FinishLevel()
    {
        Time.timeScale = 1;
        //gameObject.SetActive(false);
        victoryScreen.SetActive(true);
        fadeAnimation.SetBool("Fade", true);
        yield return new WaitUntil(() => { return victoryFade.color.a >= 1f; });
        fadeAnimation.SetBool("Fade", false);
        Debug.Log("done fading");
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
