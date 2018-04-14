using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class StrangerDangerScript : MonoBehaviour {

    private struct SpeedVars
    {
        public float ForwardSpeed;
        public float BackwardSpeed;
        public float StrafeSpeed;
        public float JumpForce;
    }

    private SpeedVars speedVars;

    public float freezeTimeWhenSeen = 3f;

    public float freezeTimer = 0f;

    public LayerMask playerMask;

    public Text text;

    public bool isVisible = false;

    RigidbodyFirstPersonController.MovementSettings playerMoveSettings;
    PlayerController controller;
    GameObject player;
    float time;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        var tmp = p.GetComponent<RigidbodyFirstPersonController>();
        controller = p.transform.GetChild(0).GetComponent<PlayerController>();
        playerMoveSettings = tmp.movementSettings;
        player = p;

        speedVars = new SpeedVars
        {
            ForwardSpeed = playerMoveSettings.ForwardSpeed,
            BackwardSpeed = playerMoveSettings.BackwardSpeed,
            StrafeSpeed = playerMoveSettings.StrafeSpeed,
            JumpForce = playerMoveSettings.JumpForce
        };

        this.playerMask = LayerMask.GetMask("Player");
        this.isVisible = false;
    }


    void OnBecameVisible()
    {
        this.isVisible = true;
        //Debug.Log("visible!");
    }

    void OnBecameInvisible()
    {
        this.isVisible = false;
        //Debug.Log("invisible!");
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit rayHit;
        var tmp = player.transform.position - transform.position;
        var hit = Physics.Raycast(transform.position, tmp, out rayHit, Vector3.Distance(player.transform.position, transform.position));

        Debug.DrawRay(transform.position, player.transform.position - transform.position, Color.red);
        
        if (rayHit.transform.Equals(player.transform) && isVisible)
        {
            freezeTimer = Mathf.Min(freezeTimeWhenSeen, freezeTimer + Time.deltaTime);
        }
        else
        {
            freezeTimer = Mathf.Max(0, freezeTimer - Time.deltaTime);
        }
        var percentage = (freezeTimer / freezeTimeWhenSeen);
        Freeze(percentage);
    }

    private void Freeze(float percentage)
    {
        var movespeed = 1 - percentage;
        playerMoveSettings.ForwardSpeed = speedVars.ForwardSpeed * movespeed;
        playerMoveSettings.BackwardSpeed = speedVars.BackwardSpeed * movespeed;
        playerMoveSettings.StrafeSpeed = speedVars.StrafeSpeed * movespeed;
        playerMoveSettings.JumpForce = speedVars.JumpForce * movespeed;

        if (text != null)
        {
            var alpha = (percentage * 255f) / 255f;
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
        }

        if (freezeTimer.Equals(freezeTimeWhenSeen))
        {
            controller.Kill();
            if(text != null)
                text.text = "You lose!";
        }
    }
}
