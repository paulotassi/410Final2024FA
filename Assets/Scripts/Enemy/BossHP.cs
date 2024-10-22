using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // Required for working with UI elements

public class BossHP : MonoBehaviour
{
    public int health = 300;
    public int maxHealth = 300;  // Track the max health
    public float phaseChangeThreshold = 0.5f; // Changes phase at 50% health
    private int currentPhase = 1;

    public GameObject[] attackPatternsPhase1;
    public GameObject[] attackPatternsPhase2;

    private bool isPhaseChanging = false;

    // Reference to the UI image for the boss's health bar
    public Image healthBar;

    void Start()
    {
        // Initialize the health bar
        UpdateHealthBar();
    }

    void Update()
    {
        if (health <= 0)
        {
            Die();
        }
        else if (health <= phaseChangeThreshold * maxHealth && currentPhase == 1 && !isPhaseChanging)
        {
            StartCoroutine(ChangePhase(2));
        }
    }

    // Take damage
    public void TakeDamage(int damage)
    {
        health -= damage;

        // Clamp health to avoid negative values
        health = Mathf.Clamp(health, 0, maxHealth);

        // Update the health bar's fill amount
        UpdateHealthBar();

        if (health <= 0)
        {
            Die();
        }
    }

    // Update the health bar based on the current health
    void UpdateHealthBar()
    {
        float fillAmount = (float)health / maxHealth;
        healthBar.fillAmount = fillAmount;
    }

    // Trigger phase change
    private IEnumerator ChangePhase(int newPhase)
    {
        isPhaseChanging = true;
        // Do something like play animation or special effect here
        yield return new WaitForSeconds(2f); // Delay for visual effect

        currentPhase = newPhase;
        Debug.Log("Boss has entered Phase " + currentPhase);
        isPhaseChanging = false;

        // Switch to new attack patterns or behavior
        if (newPhase == 2)
        {
            attackPatternsPhase1 = attackPatternsPhase2;
        }
    }

    // Handle death
    void Die()
    {
        Debug.Log("Boss Defeated");
        Destroy(gameObject);
    }
}
