using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class collectableObject : MonoBehaviour
{
    
    public virtual void Player1CollectItem(Collider2D playerCollided)
    {
        GameObject PlayerThatCollided = playerCollided.gameObject;

    }
    public virtual void Player2CollectItem(Collider2D playerCollided) 
    {
        GameObject PlayerThatCollided = playerCollided.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Player1CollectItem(collision);
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag == "Player2")
        {
            Player2CollectItem(collision);
            Destroy(gameObject);
        }
        
        
    }
}
