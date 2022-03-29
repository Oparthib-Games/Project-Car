using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarUserCtrl : MonoBehaviour
{
    Drive driveCS;
    void Start()
    {
        driveCS = GetComponent<Drive>();
    }
    void Update()
    {
        float A = Input.GetAxis("Vertical");
        float S = Input.GetAxis("Horizontal");
        float B = Input.GetAxis("Jump");

        driveCS.GO0o(A, S, B);
        driveCS.CheckWheelSKid();
        driveCS.CalcEngineSound();
    }
}
