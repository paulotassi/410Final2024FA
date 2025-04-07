using UnityEngine;

public class SlowZone : MonoBehaviour
{
    public float slowAmount = 4;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var playV = collision.GetComponent<PlayerController>();
        
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Player2")
        {   
            float limitedSlow = playV.topSpeed/slowAmount;
            if (playV.topSpeed > limitedSlow)
            {
                Debug.Log(collision.name + " Slowed");
                playV.topSpeed /= slowAmount;
                playV.flightSpeed /= slowAmount;
                playV.moveHorizontalFlightSpeed /= slowAmount;
            }

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Player2")
        {
            var playV = collision.GetComponent<PlayerController>();
            playV.topSpeed *= slowAmount;
            playV.flightSpeed *= slowAmount;
            playV.moveHorizontalFlightSpeed *= slowAmount;

        }
    }
}
