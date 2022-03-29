using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCamera : MonoBehaviour
{
    public Transform target;

    Vector3 carOffset;

    void Start()
    {
        carOffset = target.position - transform.position;
    }

    void Update()
    {
        transform.position = target.position - carOffset;
    }
}
