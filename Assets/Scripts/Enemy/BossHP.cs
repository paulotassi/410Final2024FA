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

    public Animator BossAni;
    //public GameObject PhaseChangeAni;
    public GameObject[] attackPatternsPhase1;
    public GameObject[] attackPatternsPhase2;

    private bool isPhaseChanging = false;
    private bool isInvincible = false;
    

    public float invincibilityDurationSeconds;
    public BossMove BossMoveScript;
    public SpriteRenderer sprite;

    public AudioSource AudioSource;
    public AudioClip Clip;
    public float volume;
    public bool bossDead = false;
    

    // Reference to the UI image for the boss's health bar
    public Image healthBar;

    void Start()
    {
        // Initialize the health bar
        UpdateHealthBar();
        //PhaseChangeAni.SetActive(false);
        SetActiveAttackPatterns(attackPatternsPhase1,true);
        SetActiveAttackPatterns(attackPatternsPhase2,false);
    }

    void Update()
    {


        if (health <= 0)
        {
            bossDead = true;
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
        if (isInvincible) 
        {
            return;
        }
        else
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
        //BossMoveScript.enabled = false;
        //PhaseChangeAni.SetActive(true);
        BossAni.SetBool("Stage2", true);
        SetActiveAttackPatterns(attackPatternsPhase1, false);  // Deactivate Phase 1 attacks
        //sprite.color = Color.red;
        AudioSource.PlayOneShot(Clip, volume);


        StartCoroutine(BecomeTempInvincible());

        yield return new WaitForSeconds(3f); // Delay for visual effect

        currentPhase = newPhase;
        Debug.Log("Boss has entered Phase " + currentPhase);
        //sprite.color = Color.white;
        //PhaseChangeAni.SetActive(false);
        //BossMoveScript.enabled = true;
        isPhaseChanging = false;

        // Switch to new attack patterns or behavior
        if (newPhase == 2)
        {
            SetActiveAttackPatterns(attackPatternsPhase2, true);   // Activate Phase 2 attacks
            BossMoveScript.speed = 20;
        }
    }

    // Handle death
    void Die()
    {
        Debug.Log("Boss Defeated");
        //Destroy(gameObject);
        gameObject.SetActive(false);
        bossDead = true;
    }

    void SetActiveAttackPatterns(GameObject[] attackPatterns, bool isActive)
    {
        foreach (GameObject attack in attackPatterns)
        {
            attack.SetActive(isActive);
        }
    }

    IEnumerator BecomeTempInvincible()
    {
        
        isInvincible = true;

        yield return new WaitForSeconds(invincibilityDurationSeconds);

        isInvincible = false;

    }


}
