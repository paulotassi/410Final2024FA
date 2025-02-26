using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 100;

    // Function to handle taking damage
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Enemy DAMAGED!");
        if (health <= 0)
        {
            // Handle death, like destroying the object
            Destroy(transform.gameObject);
            
        }
    }
}
