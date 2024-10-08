using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class breakableBox : interactableObstacle
{
    [SerializeField] public float timeUntilDestroyed = 0f;
    public override void ObjectCollision()
    {
        Debug.Log("Crashed");
        StartCoroutine(breakWall());
    }

    private IEnumerator breakWall()
    {
        yield return new WaitForSeconds(timeUntilDestroyed);
        Destroy(gameObject);
    }

}
