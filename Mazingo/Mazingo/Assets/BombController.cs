using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{

    public float detonationTimeWhenCarried = 5;
    private float detonationTimer;

    public AudioClip explosionSound;
    public AudioClip tickingSound;
    public float pitchFactor = 1;
    private AudioSource audioSource;

    private bool pickedUp;
    private bool canBePickedUp = true;
    private bool isExploded;

    // Use this for initialization
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        if (pickedUp)
        {
            detonationTimer = Mathf.Min(detonationTimeWhenCarried, detonationTimer + Time.deltaTime);
        }
        else
        {
            detonationTimer = Mathf.Max(0, detonationTimer - Time.deltaTime);
        }

        if (detonationTimer == 0)
            audioSource.pitch = 0.5f;
        else
            audioSource.pitch = 1 + (detonationTimer / detonationTimeWhenCarried) * pitchFactor;


        if (!isExploded && detonationTimer >= detonationTimeWhenCarried)
            Explode();
    }

    public void PickUpBomb()
    {
        if (canBePickedUp)
            pickedUp = true;
    }

    public void DropBomb()
    {
        pickedUp = false;
    }

    public void StartBomb()
    {
        detonationTimer = 0;
        detonationTimeWhenCarried = 5;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        pickedUp = true;
    }

    private void Explode()
    {
        isExploded = true;
        audioSource.clip = null;
        audioSource.pitch = 1;
        transform.GetChild(0).gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        audioSource.volume = 1;
        audioSource.PlayOneShot(explosionSound);
        StartCoroutine(Disable());
    }

    IEnumerator Disable()
    {
        yield return new WaitForSeconds(5);
        transform.gameObject.SetActive(false);
    }

}
