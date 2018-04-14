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

    // Use this for initialization
    void Start()
    {
        countDownTimer = countDown;
        startText = false;
        text = GetComponent<TextMesh>();
        testNumber = (int)Mathf.Floor(Random.Range(402017, 754392));
        text.text = "WELCOME TEST SUBJECT \n #" + testNumber;
    }

    // Update is called once per frame
    void Update()
    {
        countDownTimer -= Time.deltaTime;

        if (countDownTimer < 0)
        {
            if(startText)
                text.text = "WELCOME TEST SUBJECT \n #" + testNumber;
            else
                text.text = "Commence operation\n Seek & Destroy";

            countDownTimer = countDown; 
            startText = !startText;
        }
    }
}
