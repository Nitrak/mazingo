using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrangerDangerScript : MonoBehaviour {

    public float freezeTimeWhenSeen = 3f;

    public float freezeTimer = 0f;

    public LayerMask playerMask;

    public Text text;

    public bool isVisible = false;

    GameObject player;
    float time;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
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
        var hit = Physics.Raycast(transform.position, player.transform.position - transform.position, out rayHit, 5f);

        Debug.DrawRay(transform.position, player.transform.position - transform.position, Color.red);

        
        if (rayHit.transform.Equals(player.transform) && isVisible)
        {
            freezeTimer = Mathf.Min(freezeTimeWhenSeen, freezeTimer + Time.deltaTime);
        }
        else
        {
            freezeTimer = Mathf.Max(0, freezeTimer - Time.deltaTime);
        }

        var alpha = ((freezeTimer / freezeTimeWhenSeen) * 255f) / 255f;
        //Debug.Log(alpha);
        text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);

        if (freezeTimer.Equals(freezeTimeWhenSeen))
        {
            text.text = "You lose!";
        }
    }
}
