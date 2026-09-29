using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 100;
    [Tooltip("If above 0, a familiar hit takes off 1/N of the enemy's health, so it dies in exactly N hits. 0 = use the projectile's damage")]
    public int hitsToKill = 0;
    private int maxHealth;
    public AdvancedEnemyController advancedEnemyController;
    public GameObject spiderlingPrefab;
    public SpriteRenderer spriteRenderer;
    public Color originalColor;

    public void Start()
    {
        advancedEnemyController = GetComponent<AdvancedEnemyController>();
        originalColor = spriteRenderer.color;
        maxHealth = health;
    }

    // One whole hit: 1/hitsToKill of the enemy's health
    public void TakeHit()
    {
        int damage = hitsToKill > 0 ? Mathf.CeilToInt(maxHealth / (float)hitsToKill) : health;
        TakeDamage(damage);
    }

    // Function to handle taking damage
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Enemy DAMAGED!");
        StartCoroutine(EnemyFlashOnDamage());
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

    public virtual IEnumerator EnemyFlashOnDamage()
    {
        Debug.Log("Get Wrecked Damaging enemy should change color");
        
        
        
        Color flashColor = Color.red;
        float flashInterval = 0.05f;
        bool isFlashing = false;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            spriteRenderer.color = isFlashing ? originalColor : flashColor;
            isFlashing = !isFlashing;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }
        spriteRenderer.color = originalColor;
    }
}
