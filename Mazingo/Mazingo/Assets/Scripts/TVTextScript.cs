using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVTextScript : MonoBehaviour
{

    TextMesh text;
    public float countDown = 2;
    private float countDownTimer;
    int testNumber;
    bool startText;
    string initialText = "";

    // Use this for initialization
    void Start()
    {
        countDownTimer = countDown;
        startText = false;
        text = GetComponent<TextMesh>();
        testNumber = (int)Mathf.Floor(Random.Range(402017, 754392));
        initialText = "TEST SUBJECT\n #" + testNumber;
        text.text = initialText;
    }

    // Update is called once per frame
    void Update()
    {
        countDownTimer -= Time.deltaTime;

        if (countDownTimer < 0)
        {
            if(startText)
                text.text = initialText;
            else
                text.text = "Locate explosive\n Breaking Point";

            countDownTimer = countDown; 
            startText = !startText;
        }
    }
}
