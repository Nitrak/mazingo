﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTextScript : MonoBehaviour
{

    TextMesh text;
    private float countDown;
    private float countDownTimer;
    bool startText;
    BombController bc;

    // Use this for initialization
    void Start()
    {
        bc = transform.parent.GetComponent<BombController>();
        startText = false;
        text = GetComponent<TextMesh>();
        text.text = countDownTimer.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        countDownTimer = bc.GetTimeLeft();
        text.text = countDownTimer.ToString();

    }
}
