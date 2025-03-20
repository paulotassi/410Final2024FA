using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform playerPos;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = playerPos.position;
    }
}
