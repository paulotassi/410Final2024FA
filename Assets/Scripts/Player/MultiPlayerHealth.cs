using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class MultiPlayerHealth : NetworkBehaviour
{
    public int maxHealth = 100;

    // Make currentHealth a NetworkVariable so it syncs across clients
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private Vector2 lastPosition;
    public PlayerController playerController;
    public NetworkVariable<bool> isInvincible = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public float invincibilityDurationSeconds;
    public GameObject playerModel;
    public GameObject Shield;
    private Rigidbody2D playerRB;



    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            currentHealth.Value = maxHealth;
            isInvincible.Value = false;
        }
        playerRB = GetComponent<Rigidbody2D>();
    }


    [ServerRpc (RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        //if (isInvincible.Value) return;

        Debug.LogWarning(this.gameObject.name + " Current Health:  " + currentHealth.Value);

        if (IsOwner) // Ensure only the owner can modify health.
        {
            currentHealth.Value -= damage;  // Reduce health by the damage.
            currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);  // Clamp health between 0 and maxHealth.

            if (currentHealth.Value <= 0)
            {
                Die();  // Call die method if health is 0.
            }
        }
    }

    private void Die()
    {
        StartCoroutine(Respawn(5f));
    }

    public IEnumerator Respawn(float duration)
    {
        playerController.enabled = false;
        playerRB.simulated = false;
        transform.localScale = Vector3.zero;

        yield return new WaitForSeconds(duration);

        transform.position = lastPosition;
        transform.localScale = new Vector3(1,1,1);
        playerController.enabled = true;
        playerRB.simulated = true;
        currentHealth.Value = maxHealth;
    }
}
