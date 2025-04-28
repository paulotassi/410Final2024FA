using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class projectile : NetworkBehaviour
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
        Debug.Log(" I have Spawned");

        rb = this.gameObject.GetComponent<Rigidbody2D>();

        rb.linearVelocity = transform.right * projectileSpeed;

        Destroy(gameObject, lifetime); // Destroy the projectile after a certain time
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.GetComponent<PlayerController>())
        {
            Destroy(this.gameObject);
        }
        else if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            MultiPlayerHealth health = collision.gameObject.GetComponent<MultiPlayerHealth>();
            if (health != null)
            {
                health.TakeDamageServerRpc(projectileDamage); // ServerRpc now
            }

            Destroy(this.gameObject);
        }
    }
}
