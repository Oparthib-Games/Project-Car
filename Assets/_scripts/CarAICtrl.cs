using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAICtrl : MonoBehaviour
{
    public Circuit circuitCS;
    Drive driveCS;

    public float steeringSensitivity = 0.01f;
    Vector3 targetPoint;
    int currWaypoint = 0;

    void Start()
    {
        driveCS = GetComponent<Drive>();
        targetPoint = circuitCS.waypoints[currWaypoint].transform.position;
    }

    void Update()
    {
        Vector3 localTarget = driveCS.RB.gameObject.transform.InverseTransformPoint(targetPoint);//??????????????
        float distanceToTarget = Vector3.Distance(targetPoint, driveCS.RB.gameObject.transform.position);

        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(driveCS.currSpeed);
        float acceleration = 0.5f;
        float brake = 0;

        if(distanceToTarget < 5.0f)
        {
            brake = 0.9f;
            acceleration = 0.1f;
        }

        driveCS.GO0o(acceleration, steer, brake);

        if(distanceToTarget < 5.0f)// Threshold make larger if car start to circle waypoint
        {
            currWaypoint++;
            if (currWaypoint >= circuitCS.waypoints.Length)
                currWaypoint = 0;

            targetPoint = circuitCS.waypoints[currWaypoint].transform.position;
        }
    }
}
