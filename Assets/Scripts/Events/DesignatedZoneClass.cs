using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesignatedZoneClass : MonoBehaviour
{

    [SerializeField] public GameManager gm;

private void Start()
{
    gm = FindFirstObjectByType<GameManager>();
}

public virtual void EnteredDesignatedZone()
{

}


private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Player2")
    {
       
        EnteredDesignatedZone();

    }
}

private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Player2")
        {
            
            

        }
    }

}
