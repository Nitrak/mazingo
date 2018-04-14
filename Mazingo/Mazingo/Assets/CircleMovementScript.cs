using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleMovementScript : MonoBehaviour
{

    public float radius = 3;
    public float speed;
    private float rotationTimer;

    private Vector3 initialTransform;

    // Use this for initialization
    void Start()
    {
        initialTransform = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        rotationTimer += Time.deltaTime * speed;

        float x = initialTransform.x + Mathf.Cos(rotationTimer) * radius;
        float y = 0;
        float z = initialTransform.z + Mathf.Sin(rotationTimer) * radius;

        transform.position = new Vector3(x, y, z);

    }
}
