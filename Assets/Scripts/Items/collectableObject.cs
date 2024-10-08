using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collectableObject : MonoBehaviour
{
    [SerializeField] public int damageValue;
    public virtual void Player1CollectItem()
    {

    }
    public virtual void Player2CollectItem() 
    { 
    
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Player1CollectItem();
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damageValue);
        }
        else if (collision.gameObject.tag == "Player2")
        {
            Player2CollectItem();
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damageValue);
        }
        Destroy(gameObject);
    }
}
