using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationLock : MonoBehaviour
{

    void Update()
    {
        this.transform.rotation = Quaternion.Euler(new Vector3(this.transform.rotation.x, this.transform.rotation.y, 0));
    }
}
