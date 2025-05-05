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
    [SerializeField] GameManager gameManager;


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
       rb.linearVelocity = transform.right * projectileSpeed;
       gameManager = FindFirstObjectByType<GameManager>();
       
        Destroy(gameObject, lifetime); // Destroy the projectile after a certain time
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collided with: " + collision.gameObject.name);
        if (!collision.gameObject.GetComponent<PlayerController>() && !collision.gameObject.GetComponent<EnemyHealth>())
        {
            Destroy(this.gameObject);
        }
        else if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(projectileDamage);
            if (projectileStun)
            {
                collision.gameObject.GetComponent<PlayerController>().StartCoroutine(collision.gameObject.GetComponent<PlayerController>().Stunned(projectileStunDuration));
            }
            Destroy(this.gameObject);
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
            Destroy(this.gameObject);
        }
        else if (collision.gameObject.GetComponent<BossHP>() != null)
        {


            
                collision.gameObject.GetComponent<BossHP>().TakeDamage(projectileDamage);
            

        }
    }
}
