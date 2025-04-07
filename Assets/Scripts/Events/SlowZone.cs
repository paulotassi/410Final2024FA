using UnityEngine;

public class SlowZone : MonoBehaviour
{
    public float slowAmount = 4;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
