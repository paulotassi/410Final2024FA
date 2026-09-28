using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{

    public float projectileSpeed;
    public int projectileDamage;
    public bool projectileStun = false;
    public float projectileStunDuration = 1f;
    public float lifetime = 5f;
    public Rigidbody2D rb;
    public GameObject playerAim;
    public GameObject projectileCollsion;
    [SerializeField] GameManager gameManager;

    // Familiar mode: when a launcher owns this projectile it flies out, then homes back instead of disappearing
    private FamiliarLauncher launcher;
    private int launcherSlot;
    private bool returning;
    private Vector3 launchPoint;

    public void SetLauncher(FamiliarLauncher owner, int slot)
    {
        launcher = owner;
        launcherSlot = slot;
    }

    private void Start()
    {
        if (this.gameObject.name == "SpellProjectileP2")
        {
            //Insert Rotation Logic
        }
        else
        {

        }
       rb = this.gameObject.GetComponent<Rigidbody2D>();
       gameManager = FindFirstObjectByType<GameManager>();

       if (launcher != null)
       {
           launchPoint = transform.position;
           rb.linearVelocity = transform.right * projectileSpeed * launcher.SpeedMultiplier;
       }
       else
       {
           rb.linearVelocity = transform.right * projectileSpeed;
           Destroy(gameObject, lifetime); // Destroy the projectile after a certain time
       }
    }

    private void Update()
    {
        if (launcher == null) return;

        if (!returning)
        {
            if ((transform.position - launchPoint).sqrMagnitude >= launcher.maxRange * launcher.maxRange)
                StartReturn();
            return;
        }

        // Home back to the witch and reload
        Vector3 toWitch = launcher.transform.position - transform.position;
        if (toWitch.sqrMagnitude < 0.6f * 0.6f)
        {
            launcher.FamiliarReturned(launcherSlot);
            Destroy(gameObject);
            return;
        }
        rb.linearVelocity = toWitch.normalized * projectileSpeed * launcher.SpeedMultiplier * launcher.returnSpeedMultiplier;
    }

    private void OnDestroy()
    {
        // Lost in some other way (e.g. level unload) - hand the familiar back so the witch can't run dry for good
        if (launcher != null && !returning) launcher.FamiliarReturned(launcherSlot);
    }

    private void StartReturn()
    {
        returning = true;
    }

    // Normal projectiles vanish on impact; familiars turn around and fly home instead
    private void Finish()
    {
        if (launcher != null)
        {
            StartReturn();
            return;
        }
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (returning) return;

        Debug.Log("Collided with: " + collision.gameObject.name);
        if (!collision.gameObject.GetComponent<PlayerController>() && !collision.gameObject.GetComponent<EnemyHealth>())
        {
            Instantiate(projectileCollsion, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
            Finish();
        }
        else if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(projectileDamage);
            if (projectileStun)
            {
                collision.gameObject.GetComponent<PlayerController>().StartCoroutine(collision.gameObject.GetComponent<PlayerController>().Stunned(projectileStunDuration));
            }
            Instantiate(projectileCollsion, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
            Finish();
        }
        else if (collision.gameObject.GetComponent<EnemyController>() != null)
        {

            if (projectileStun)
            {
                collision.gameObject.GetComponent<EnemyController>().StartCoroutine(collision.gameObject.GetComponent<EnemyController>().Stunned(projectileStunDuration));
            }
            else
            {
                collision.gameObject.GetComponent<EnemyHealth>().TakeDamage(projectileDamage);
            }
            Instantiate(projectileCollsion, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
            Finish();
        }
        else if (collision.gameObject.GetComponent<BossHP>() != null)
        {



            collision.gameObject.GetComponent<BossHP>().TakeDamage(projectileDamage);
            Instantiate(projectileCollsion, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
            Finish();


        }
    }
}
