using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RotationLock : NetworkBehaviour
{

    void Update()
    {
        this.transform.rotation = Quaternion.Euler(new Vector3(this.transform.rotation.x, this.transform.rotation.y, 0));
    }
}
