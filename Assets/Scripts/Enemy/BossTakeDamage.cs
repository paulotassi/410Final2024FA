using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // Required for working with UI elements

public class BossTakeDamage : MonoBehaviour
{
    public int attackDamage = 25;
    private BossHP BossHealth;
    private Transform Boss;


    void Start()
    {
        Boss = GameObject.FindGameObjectWithTag("Boss").transform;

        BossHealth = Boss.GetComponent<BossHP>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object we collided with is tagged as "Playerspell"
        if (other.CompareTag("PlayerSpell"))
        {
            BossHP targetBossHP = BossHealth;
            // Apply damage
            targetBossHP.TakeDamage(attackDamage);

            // Optionally, destroy the Playerspell GameObject after collision
            // Destroy(other.gameObject);
        }
    }
}
