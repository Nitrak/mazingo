using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
    public const int RoomTimeoutMultiplier = 5;

    public float deathDistance;
    private float detonationTimeWhenCarried;
    public float explosionTimeWhenPlaced = 5;
    private float detonationTimer;
    private float initialDetonationTimer;

    public AudioClip explosionSound;
    public AudioClip tickingSound;
    public float pitchFactor = 1;
    public AudioSource audioSource;

    private bool pickedUp;
    private bool startBombTimer;
    private bool canBePickedUp = true;
    private bool isExploded;

    private Vector3 initialLocalScale;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    private PlayerController player;
    private Rigidbody body;

    public AudioSource music;
    public AudioClip pickupSound;

    // Use this for initialization
    void Start()
    {
        SetBombTimeout();
        this.initialDetonationTimer = detonationTimeWhenCarried;
        body = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetComponent<PlayerController>();
        player.OnDropped += Player_OnDropped;
        player.OnPickedUp += Player_OnPickedUp;
        isExploded = false;

        this.spawnPosition = this.transform.position;
        this.spawnRotation = this.transform.rotation;
        this.initialLocalScale = this.transform.localScale;
    }

    private void Player_OnPickedUp(object sender, PlayerController.CarriedEventArgs e)
    {
        if (e.Item == body)
            PickUpBomb();
    }

    private void Player_OnDropped(object sender, PlayerController.CarriedEventArgs e)
    {
        if (e.Item == body)
            DropBomb();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, deathDistance);
    }

    // Update is called once per frame
    void Update()
    {
        if (pickedUp || startBombTimer)
        {
            detonationTimer = Mathf.Min(detonationTimeWhenCarried, detonationTimer + Time.deltaTime);
        }
        else
        {
            //detonationTimer = Mathf.Max(0, detonationTimer - Time.deltaTime);
        }

        if (detonationTimer == 0)
            audioSource.pitch = 0.5f;
        else
            audioSource.pitch = 0.5f + (detonationTimer / detonationTimeWhenCarried) * pitchFactor;

        if (!isExploded && detonationTimer >= detonationTimeWhenCarried)
            Explode();
    }

    public void PickUpBomb()
    {
        if (canBePickedUp)
        {
            if (!music.isPlaying)
            {
                player.gameObject.GetComponent<AudioSource>().Stop();
                music.clip = pickupSound;
                music.Play();
            }
            pickedUp = true;
            SetBombTimeout();
        }
    }

    private void SetBombTimeout()
    {
        var roomController = GameController.instance.GetRoomController();
        var dist = roomController.GetMazeGenerator().FindDistance(ref roomController.Maze);
        this.detonationTimeWhenCarried = Math.Max(10, dist * RoomTimeoutMultiplier);
    }

    public void DropBomb()
    {
        //pickedUp = false;
    }

    public float StartBomb()
    {
        detonationTimer = 0;
        detonationTimeWhenCarried = explosionTimeWhenPlaced;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        startBombTimer = true;
        transform.GetChild(1).GetComponent<LineRenderer>().enabled = true;
        transform.GetChild(1).transform.rotation = Quaternion.identity;
        return explosionTimeWhenPlaced;
    }

    public void Explode()
    {
        if (pickedUp)
            player.dropItem();

        Debug.Log("booom");
        isExploded = true;
        music.clip = null;
        music.Stop();
        audioSource.clip = null;
        audioSource.pitch = 1;
        transform.GetChild(0).gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        audioSource.volume = 1;
        audioSource.PlayOneShot(explosionSound);
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        playerObject.transform.position = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 1, playerObject.transform.position.z);

        playerObject.GetComponent<Rigidbody>().AddExplosionForce(2000, transform.position, 200);

        if (Vector3.Distance(playerObject.transform.position, transform.position) < deathDistance)
        {
            player.Kill();
        }

        StartCoroutine(Respawn());
    }


    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5);
        audioSource.loop = false;
        audioSource.Stop();
        music.Stop();
        foreach (Transform obj in transform)
        {
            Destroy(obj.gameObject);
        }
        Destroy(this.gameObject);
        /*
        Debug.Log("asplode!");
        body.velocity = Vector3.zero;
        this.transform.position = spawnPosition;
        this.transform.rotation = spawnRotation;
        this.transform.localScale = initialLocalScale;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        transform.GetChild(1).GetComponent<LineRenderer>().enabled = false;
        startBombTimer = false;

        isExploded = false;
        audioSource.clip = tickingSound;
        audioSource.pitch = 0.5f;
        audioSource.volume = 1;
        detonationTimer = 0;
        detonationTimeWhenCarried = initialDetonationTimer;

        Debug.Log("We're back!");*/
    }

    IEnumerator Disable()
    {

        yield return new WaitForSeconds(5);
        //transform.gameObject.SetActive(false);
    }

    public float GetTimeLeft()
    {
        return Mathf.Ceil(detonationTimeWhenCarried - detonationTimer);
    }

    public float GetExplosionZone()
    {
        return deathDistance;
    }

}
