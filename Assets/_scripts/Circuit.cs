using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circuit : MonoBehaviour
{
    public GameObject[] waypoints;
    private void OnDrawGizmos()
    {
        if(waypoints.Length > 1)
            for(int i=0; i < waypoints.Length; i++)
                Gizmos.DrawLine(waypoints[i].transform.position, i!=waypoints.Length-1? waypoints[i + 1].transform.position: waypoints[0].transform.position);
    }
}
