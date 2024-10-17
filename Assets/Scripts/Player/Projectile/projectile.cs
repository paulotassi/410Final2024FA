using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{

    public float projectileSpeed;
    public float projectileDamage;
    public Rigidbody2D rb;

    private void Start()
    {
       rb = this.gameObject.GetComponent<Rigidbody2D>();
       rb.velocity = transform.right * projectileSpeed;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.GetComponent<PlayerController>())
        {
            Destroy(gameObject);
        }
    }

}
