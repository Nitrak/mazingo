using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
    public float deathDistance;
    public float detonationTimeWhenCarried = 20;
    public float explosionTimeWhenPlaced = 5;
    private float detonationTimer;

    public AudioClip explosionSound;
    public AudioClip tickingSound;
    public float pitchFactor = 1;
    private AudioSource audioSource;

    private bool pickedUp;
    private bool canBePickedUp = true;
    private bool isExploded;

    private PlayerController player;
    private Rigidbody body;

    // Use this for initialization
    void Start()
    {
        body = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        player.OnDropped += Player_OnDropped;
        player.OnPickedUp += Player_OnPickedUp; ;
        audioSource = GetComponent<AudioSource>();
        isExploded = false;
    }

    private void Player_OnPickedUp(object sender, PlayerController.CarriedEventArgs e)
    {
        if(e.Item == body)
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
        Debug.Log("MY PICK UP");
        if (canBePickedUp)
            pickedUp = true;
    }

    public void DropBomb()
    {
        pickedUp = false;
    }

    public float StartBomb()
    {
        detonationTimer = 0;
        detonationTimeWhenCarried = explosionTimeWhenPlaced;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        pickedUp = true;
        transform.GetChild(1).GetComponent<LineRenderer>().enabled = true;
        transform.GetChild(1).transform.rotation = Quaternion.identity;
        return explosionTimeWhenPlaced;
    }

    private void Explode()
    {
        Debug.Log("booom");
        isExploded = true;
        audioSource.clip = null;
        audioSource.pitch = 1;
        transform.GetChild(0).gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        audioSource.volume = 1;
        audioSource.PlayOneShot(explosionSound);
        var player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z);

        player.GetComponent<Rigidbody>().AddExplosionForce(2000, transform.position, 200);

        if (Vector3.Distance(player.transform.position, transform.position) < deathDistance)
        {
            Debug.Log("DEAD");//player.Kill();
        }

        StartCoroutine(Disable());
    }

    IEnumerator Disable()
    {
        yield return new WaitForSeconds(5);
        transform.gameObject.SetActive(false);
    }

    public float GetExplosionZone()
    {
        return deathDistance;
    }

}
