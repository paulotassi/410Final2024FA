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
    private float returnAt;          // time the flight home starts (a short pause on impact)
    private Vector3 launchPoint;
    private SpriteRenderer body;
    private Vector3 bodyBaseScale;

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
           Transform b = transform.Find("FamiliarBody");
           if (b != null)
           {
               body = b.GetComponent<SpriteRenderer>();
               bodyBaseScale = b.localScale;
           }
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
                StartReturn(0f);
            return;
        }

        // Hang for a moment after an impact
        if (Time.time < returnAt)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Home back to the witch and reload
        Vector3 toWitch = launcher.transform.position - transform.position;
        if (toWitch.sqrMagnitude < 1f)
        {
            launcher.FamiliarReturned(launcherSlot);
            Destroy(gameObject);
            return;
        }
        rb.linearVelocity = toWitch.normalized * launcher.returnSpeed * launcher.SpeedMultiplier;
    }

    private void OnDestroy()
    {
        // Lost in some other way (e.g. level unload) - hand the familiar back so the witch can't run dry for good
        if (launcher != null && !returning) launcher.FamiliarReturned(launcherSlot);
    }

    private void StartReturn(float pause)
    {
        returning = true;
        returnAt = Time.time + pause;
    }

    // Normal projectiles vanish on impact; familiars flash, spark and then fly home instead
    private void Finish(bool hitSomethingAlive = false)
    {
        if (launcher != null)
        {
            launcher.PlayHitFeedback(transform.position, hitSomethingAlive);
            StartReturn(launcher.hitPause);
            StartCoroutine(ImpactFlash());
            return;
        }
        Destroy(this.gameObject);
    }

    // The familiar flashes and squashes for a moment when it hits something
    private IEnumerator ImpactFlash()
    {
        if (body == null) yield break;
        Color original = body.color;
        float t = 0f;
        const float duration = 0.25f;
        while (t < duration && body != null)
        {
            float k = 1f - t / duration;
            body.color = Color.Lerp(original, new Color(1f, 0.35f, 0.35f), k);
            body.transform.localScale = bodyBaseScale * (1f + 0.5f * k);
            t += Time.deltaTime;
            yield return null;
        }
        if (body != null)
        {
            body.color = original;
            body.transform.localScale = bodyBaseScale;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (returning) return;

        // projectiles pass through each other
        if (collision.GetComponentInParent<projectile>() != null) return;

        // enemies have child colliders (e.g. the HitBox), so look upward for the enemy scripts
        EnemyHealth enemyHealth = collision.GetComponentInParent<EnemyHealth>();
        EnemyController enemyController = collision.GetComponentInParent<EnemyController>();

        if (!collision.gameObject.GetComponent<PlayerController>() && enemyHealth == null && collision.GetComponentInParent<BossHP>() == null)
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
            Finish(true);
        }
        else if (enemyController != null && enemyHealth != null)
        {

            if (projectileStun)
            {
                enemyController.StartCoroutine(enemyController.Stunned(projectileStunDuration));
            }
            else
            {
                // Familiar hits count as whole hits: enemies with hitsToKill set (ghosts, spiders) die in that many
                if (launcher != null && enemyHealth.hitsToKill > 0) enemyHealth.TakeHit();
                else enemyHealth.TakeDamage(projectileDamage);
            }
            Instantiate(projectileCollsion, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
            Finish(true);
        }
        else if (collision.gameObject.GetComponent<BossHP>() != null)
        {



            collision.gameObject.GetComponent<BossHP>().TakeDamage(projectileDamage);
            Instantiate(projectileCollsion, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
            Finish(true);


        }
    }
}
