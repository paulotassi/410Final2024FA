using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{

    public float projectileSpeed;
    public float projectileDamage;
    public float lifetime = 5f;
    public Rigidbody2D rb;

    private void Start()
    {
       rb = this.gameObject.GetComponent<Rigidbody2D>();
       rb.velocity = transform.right * projectileSpeed;

        Destroy(gameObject, lifetime); // Destroy the projectile after a certain time
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.GetComponent<PlayerController>())
        {
            Destroy(gameObject);
        }
    }

}
