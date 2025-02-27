using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 100;
    public AdvancedEnemyController advancedEnemyController;
    public GameObject spiderlingPrefab;

    public void Start()
    {
        advancedEnemyController = GetComponent<AdvancedEnemyController>();
    }

    // Function to handle taking damage
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Enemy DAMAGED!");
        if (health <= 0)
        {
            // Handle death, like destroying the object
            Destroy(transform.gameObject);
            if (advancedEnemyController != null)
            {
                for (int i = 0; i < advancedEnemyController.spiderlingAmount; i++)
                {
                    Vector3[] spawnlocation = new Vector3[i]; // Create an array to hold spawn positions for ingredients

                    for (int j = 0; j < spawnlocation.Length; j++)
                    {
                        // Generate a random position near the player
                        spawnlocation[j] = new Vector3(
                            this.gameObject.transform.position.x + Random.Range(-3, 3),
                            this.gameObject.transform.position.y + Random.Range(-3, 3),
                            this.gameObject.transform.position.z
                        );
                    }

                    GameObject[] totalSpawnedSpiderlings = new GameObject[i]; // Array to hold ingredient GameObjects

                    for (int j = 0; j < totalSpawnedSpiderlings.Length; j++)
                    {
                        // Instantiate each ingredient at its calculated spawn location
                        totalSpawnedSpiderlings[j] = Instantiate(spiderlingPrefab, spawnlocation[j], Quaternion.identity);
                    }
                }
            }
            
        }
    }
}
