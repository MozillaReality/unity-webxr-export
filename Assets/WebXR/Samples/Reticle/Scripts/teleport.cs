using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class teleport : MonoBehaviour
{
    public void changePos()
    {
        this.transform.position = new Vector3(Random.Range(-1, 1), Random.Range(0.3f, 1), Random.Range(2, 4));
    }
}
