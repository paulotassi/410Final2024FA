using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collectableObject : MonoBehaviour
{
    
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
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag == "Player2")
        {
            Player2CollectItem();
            Destroy(gameObject);
        }
        
        
    }
}
