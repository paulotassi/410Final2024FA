using UnityEngine;

public class SlowZone : MonoBehaviour


{
    public float slowAmount = 4;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip enterSlowZoneClip;
    [SerializeField] private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Player2"))
        {
            if (audioSource != null && enterSlowZoneClip != null)
            {
                audioSource.PlayOneShot(enterSlowZoneClip);
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        var playV = collision.GetComponent<PlayerController>();
        
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Player2")
        {   

                playV.playerSlowed(slowAmount);


        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var playV = collision.GetComponent<PlayerController>();
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Player2")
        {
            playV.playerUnslowed(slowAmount);

        }
    }

    
}
