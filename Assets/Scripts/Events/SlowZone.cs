using UnityEngine;

public class SlowZone : MonoBehaviour
{
    public float slowAmount = 4;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Player2")
        {
            Debug.Log(collision.name + " Slowed");
            collision.GetComponent<PlayerController>().topSpeed /= slowAmount;
            collision.GetComponent<PlayerController>().flightSpeed /= slowAmount;
            collision.GetComponent<PlayerController>().moveHorizontalFlightSpeed /= slowAmount;

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Player2")
        {

            collision.GetComponent<PlayerController>().topSpeed *= slowAmount;
            collision.GetComponent<PlayerController>().flightSpeed *= slowAmount;
            collision.GetComponent<PlayerController>().moveHorizontalFlightSpeed *= slowAmount;

        }
    }
}
