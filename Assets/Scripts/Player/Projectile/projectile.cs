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
        if (!collision.gameObject.GetComponent<PlayerController>())
        {
            Destroy(this.gameObject);
        }
        else if (collision.gameObject.GetComponent<PlayerController>() != null && gameManager.competetiveMode)
        {
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(projectileDamage);
            //Debug.Log("Hit a Player");
            Destroy(this.gameObject);
        }
    }
}
