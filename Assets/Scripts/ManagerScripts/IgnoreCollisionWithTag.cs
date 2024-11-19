using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollisionWithTag : MonoBehaviour
{
    public string ignoreTag;

    // Start is called before the first frame update
    void Start()
    {
        Collider2D colliderToIgnore = GetComponentInChildren<Collider2D>();

        foreach (GameObject otherObject in GameObject.FindGameObjectsWithTag(ignoreTag))
        {
            Collider2D otherCollider = otherObject.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(colliderToIgnore, otherCollider, true);
        }
    }

    
}
