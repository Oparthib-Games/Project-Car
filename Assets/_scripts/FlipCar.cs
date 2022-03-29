using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipCar : MonoBehaviour
{
    Rigidbody RB;
    float lastTimeChecked;

    void Start()
    {
        RB = GetComponent<Rigidbody>();    
    }

    void Update()
    {
        if (transform.up.y > 0.5f || RB.velocity.magnitude > 1)
        {
            lastTimeChecked = Time.time;
        }
        if(Time.time > lastTimeChecked + 3)
        {
            RightCar();
        }
    }

    void RightCar()
    {
        this.transform.position += Vector3.up;
        this.transform.rotation = Quaternion.LookRotation(this.transform.forward);
    }
}
