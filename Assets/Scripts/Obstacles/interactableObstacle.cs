using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interactableObstacle : MonoBehaviour
{
    [SerializeField] public int damageValue;
    [SerializeField] protected GameObject playerEntered;
    public virtual void ObjectCollision()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Player2")
        {
            ObjectCollision();
            playerEntered = collision.gameObject;
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damageValue);
        } 
        
    }
   
}
