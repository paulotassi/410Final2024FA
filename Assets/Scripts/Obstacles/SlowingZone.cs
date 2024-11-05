using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowingZone : interactableObstacle
{
    public float speedDuration;
    
    public override void ObjectCollision()
    {
        base.ObjectCollision();
        if (playerEntered != null)
        {
            StartCoroutine(slowPlayer());
            Debug.Log("Collided with: " + playerEntered.name);
            // Additional logic for handling the collision in the derived class
        }
        
        
        this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }

    public IEnumerator slowPlayer()
    {
        playerEntered.GetComponent<PlayerController>().moveHorizontalFlightSpeed = 40;
        
        yield return new WaitForSeconds(speedDuration);

        playerEntered.GetComponent<PlayerController>().moveHorizontalFlightSpeed = 20;
        Destroy(this.gameObject);
    }
}
