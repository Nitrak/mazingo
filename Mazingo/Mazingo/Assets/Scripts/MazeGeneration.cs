using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class StrangerDangerScript : MonoBehaviour
{

    private struct SpeedVars
    {
        public float WalkSpeed;
        public float RunSpeed;
        public float JumpForce;
    }

    public LayerMask playerMask;
    public Text text;
    public float freezeTimeWhenSeen = 3f;

    private SpeedVars speedVars;
    private bool isVisible = false;
    private float freezeTimer = 0f;

    private FirstPersonController playerMoveSettings;
    private PlayerController controller;
    private GameObject player;
    private Camera playerCamera;

    private Bounds myBounds;
    //RigidbodyFirstPersonController.MovementSettings playerMoveSettings;

    void Start()
    {
        myBounds = GetComponent<Collider>().bounds;
        var p = GameObject.FindGameObjectWithTag("Player");
        var tmp = p.GetComponent<RigidbodyFirstPersonController>();
        controller = p.transform.GetChild(0).GetComponent<PlayerController>();
        playerCamera = p.transform.GetChild(0).GetComponent<Camera>();
        playerMoveSettings = p.GetComponent<FirstPersonController>();
        player = p;

        speedVars = new SpeedVars
        {
            WalkSpeed = playerMoveSettings.GetMovementSpeed(),
            RunSpeed = playerMoveSettings.GetRunSpeed(),
            JumpForce = playerMoveSettings.GetJumpSpeed()
        };

        this.playerMask = LayerMask.GetMask("Player");
        this.isVisible = false;
    }


    private bool VisibilityCheck()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        if (GeometryUtility.TestPlanesAABB(planes, myBounds))
            return true;
        else
            return false;
    }

    // Update is called once per frame
    void Update()
    {
        isVisible = VisibilityCheck();

        RaycastHit rayHit;
        var tmp = player.transform.position - transform.position;
        var hit = Physics.Raycast(transform.position, tmp, out rayHit, Vector3.Distance(player.transform.position, transform.position) + 1);

        Debug.DrawRay(transform.position, player.transform.position - transform.position, Color.red);

        if (hit && rayHit.transform.Equals(player.transform) && isVisible)
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
        playerMoveSettings.SetWalkSpeed(speedVars.WalkSpeed * movespeed);
        playerMoveSettings.SetRunSpeed(speedVars.RunSpeed * movespeed);
        playerMoveSettings.SetJumpSpeed(speedVars.JumpForce * movespeed);


        if (text != null)
        {
            var alpha = (percentage * 255f) / 255f;
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
        }

        if (freezeTimer.Equals(freezeTimeWhenSeen))
        {
            playerMoveSettings.SetWalkSpeed(speedVars.WalkSpeed);
            playerMoveSettings.SetRunSpeed(speedVars.RunSpeed);
            playerMoveSettings.SetJumpSpeed(speedVars.JumpForce);
            freezeTimer = 0f;
            controller.Kill();
            if (text != null)
                text.text = "You lose!";
        }
    }
}
