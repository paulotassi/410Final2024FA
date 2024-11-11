using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class breakableBox : interactableObstacle
{
    [SerializeField] public float timeUntilDestroyed = 0f;
    [SerializeField] private Animator Ani;

    public override void ObjectCollision()
    {
        Debug.Log("Crashed");
        StartCoroutine(breakWall());
    }

    private void Awake()
    {
        Ani = gameObject.GetComponent<Animator>();
    }
    private IEnumerator breakWall()
    {
        this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        Ani.SetTrigger("Collided");

        yield return new WaitForSeconds(timeUntilDestroyed);

        Destroy(gameObject);
    }

}
