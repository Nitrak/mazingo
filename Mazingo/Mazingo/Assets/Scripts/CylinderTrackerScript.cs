using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderTrackerScript : MonoBehaviour {
    GameObject player;

    public GameObject effect;
    float time;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        ForkParticlePlugin.Instance.AddEffect(effect);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = LookAt(transform.position, player.transform.position);
        ForkParticlePlugin.Instance.Test();
    }

    public static Quaternion LookAt(Vector3 sourcePoint, Vector3 destPoint)
    {
        destPoint   = new Vector3(destPoint.x, 0f, destPoint.z);
        sourcePoint = new Vector3(sourcePoint.x, 0f, sourcePoint.z);
        Vector3 forwardVector = Vector3.Normalize(destPoint - sourcePoint);

        Vector3 rotAxis = Vector3.Cross(Vector3.forward, forwardVector);
        float dot = Vector3.Dot(Vector3.forward, forwardVector);

        Quaternion q;
        q.x = rotAxis.x;
        q.y = rotAxis.y;
        q.z = rotAxis.z;
        q.w = dot + 1;

        return q;
    }

}
