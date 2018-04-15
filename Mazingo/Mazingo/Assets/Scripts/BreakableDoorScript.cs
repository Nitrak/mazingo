using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class BreakableDoorScript : MonoBehaviour
{
    private float explosionTime = -1;

    public GameController controller;
    public Animator fadeAnimation;
    public GameObject victoryScreen;
    public Image victoryFade;

    private bool victoryGracePeriod = false;
    private Timer victoryGraceTimer;
    private int victoryGraceDelay = 4000;

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

    

    private void startVictoryGraceTimer()
    {
        victoryGracePeriod = true;
        victoryGraceTimer = new Timer((obj) =>
        {
            //Debug.Log("Grace period over");
            victoryGracePeriod = false;
            victoryGraceTimer.Dispose();
        },
                    null, victoryGraceDelay, Timeout.Infinite);
    }

    private void ExplodeDoor()
    {
        if(!victoryGracePeriod)
            StartCoroutine(FinishLevel());
    }

    IEnumerator FinishLevel()
    {
        startVictoryGraceTimer();
        Time.timeScale = 1;
        //gameObject.SetActive(false);
        victoryScreen.SetActive(true);
        fadeAnimation.SetBool("Fade", true);
        yield return new WaitUntil(() => { return victoryFade.color.a >= 1f; });
        fadeAnimation.SetBool("Fade", false);
        yield return new WaitUntil(() => victoryFade.color.a == 0f);
        Debug.Log("done fading");
        victoryScreen.SetActive(false);
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
