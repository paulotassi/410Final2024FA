using System.Collections;
using UnityEngine;

public class SpiderlingDeath : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpiderlingDie());
    }

    public IEnumerator SpiderlingDie()
    {
        yield return new WaitForSeconds(Random.Range(3,5));
        Destroy(gameObject);
           
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            Debug.Log("colliding with players");
            Destroy(gameObject);
        }
    }
}
