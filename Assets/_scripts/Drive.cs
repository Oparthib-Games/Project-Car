using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour
{
    [Header("[-Get Components-]")]
    public AudioSource skid_audioSrs1;
    public AudioSource engine_audioSrs2;
    public Rigidbody RB;

    public WheelCollider[] wheelCols;
    public GameObject[] wheelMeshes;

    [Header("[-FOR DRIVE-]")]
    public float maxSpeed = 200;
    public float torque = 200;
    public float maxSteerAngle = 30; // max steer andle 30 degree andle
    public float maxBrakeTorque = 2000;
    public GameObject[] breakLights;

    [Header("[-FOR SKID EFFECT-]")]
    public float skidThreshold = 0.4f; // VALUE GREATED THAN THIS WILL OCCURE SKID EFFECT
    public Transform skidTrailPrefab;
    Transform[] skidTrails = new Transform[4];
    public ParticleSystem smokePrefab;
    ParticleSystem[] skidSmokes = new ParticleSystem[4];

    [Header("[-FOR Engine Sound-]")]
    public float GearLength = 3; // LENGTH BETWEEN EVERY GEARS.
    public int GearNum = 5;
    public float currSpeed { get { return RB.velocity.magnitude * GearLength; } }
    public float lowPitch = 1;
    public float highPitch = 6;
    float RPM; // Revolution Per Minute
    int currGear = 1;
    float currGearPercentage;


    void Start()
    {
        for(int i=0; i<4; i++)
        {   // INSTANTIATE PARTICLE AT START AND STOP EMMISSION, AND USE IT SOMEWHERE ELSE...
            skidSmokes[i] = Instantiate(smokePrefab);
            skidSmokes[i].Stop();
        }

        foreach (GameObject x in breakLights) x.SetActive(false);
    }

    public void GO0o(float acceleration, float steer, float brake)
    {
        acceleration = Mathf.Clamp(acceleration, -1, 1);
        steer = Mathf.Clamp(steer, -1, 1);
        brake = Mathf.Clamp(brake, 0, 1);

        if(brake == 0)
            foreach (GameObject x in breakLights) x.SetActive(false);
        else
            foreach (GameObject x in breakLights) x.SetActive(true);

        float thrustTorque = 0;
        if (currSpeed < maxSpeed)
            thrustTorque = acceleration* torque;

        for (int i=0; i<4; i++)
        {            
            wheelCols[i].motorTorque = thrustTorque;

            if (i < 2) // IF i IS THE FIRST 2 TYRES
                wheelCols[i].steerAngle = steer * maxSteerAngle;
            else     //  IF i IS THE BACK WHEEL THEN APPLY BRAKE ONLY TO THE BACK WHEEL
                wheelCols[i].brakeTorque = brake * maxBrakeTorque;

            Vector3 position;
            Quaternion quaternion;

            wheelCols[i].GetWorldPose(out position, out quaternion); // TO ROTATE THE WHEEL MESH WITH WHEEL COLLIDER

            wheelMeshes[i].transform.position = position;
            wheelMeshes[i].transform.rotation = quaternion;
        }
    }

    public void CalcEngineSound()
    {
        float gearPercentage = 1 / (float)GearNum; // 1/5 = 0.2, SO THE PERCENTAGE IS 20% WHEN GEAR-NUM IS 5

        float targetGearFactor = Mathf.InverseLerp(gearPercentage * currGear, gearPercentage * (currGear + 1), Mathf.Abs(currSpeed / maxSpeed));

        currGearPercentage = Mathf.Lerp(currGearPercentage, targetGearFactor, Time.deltaTime * 5);

        var gearNumFactor = currGear / (float)GearNum;

        RPM = Mathf.Lerp(gearNumFactor, 1, currGearPercentage); // RPM is going to be btwn 0 & 1;

        float speedPercentage = Mathf.Abs(currSpeed / maxSpeed);

        float upperGearMax = (1 / (float)GearNum) * (currGear + 1);
        float downGearMax = (1 / (float)GearNum) * currGear;

        if (currGear > 0 && speedPercentage < downGearMax)
            currGear--;
        if (speedPercentage > upperGearMax && (currGear < (GearNum - 1)))
            currGear++;

        float pitch = Mathf.Lerp(lowPitch, highPitch, RPM);
        engine_audioSrs2.pitch = Mathf.Min(highPitch, pitch) * 0.25f; // 0.25f is just to reduce the value a bit
    }

    public void CheckWheelSKid()
    {
        int skidding_wheel_count = 0;

        for(int i=0; i<4; i++)
        {
            WheelHit wheel_hitInfo;
            wheelCols[i].GetGroundHit(out wheel_hitInfo);

            float wheel_ForwardSlip = wheel_hitInfo.forwardSlip;  // SLIP PROPERTY OF EVERY WHEEL
            float wheel_SideWaysSlip = wheel_hitInfo.sidewaysSlip;// SLIP PROPERTY OF EVERY WHEEL

            wheel_ForwardSlip = Mathf.Abs(wheel_ForwardSlip);
            wheel_SideWaysSlip = Mathf.Abs(wheel_SideWaysSlip);
            
            if(wheel_ForwardSlip > skidThreshold || wheel_SideWaysSlip > skidThreshold)
            {
                skidding_wheel_count++;

                if(!skid_audioSrs1.isPlaying)     skid_audioSrs1.Play();
                
                StartSkidTrail(i);

                skidSmokes[i].transform.position = wheelCols[i].transform.position - wheelCols[i].transform.up * wheelCols[i].radius;
                    //     MOVES SMOKES POS TO THE BOTTOM OF THE TYRE(tyres Relative Pos) ON EVERY UPDATE
                skidSmokes[i].Emit(1);
            }
            else
            {
                EndSkidTrail(i);
            }
        }

        if(skidding_wheel_count == 0) // MEANS NONE OF THE WHEELS ARE SKIDDING.
        {
            skid_audioSrs1.Stop();
        }
    }

    void StartSkidTrail(int i)
    {
        if (skidTrails[i] == null)      // IF THE TYRE HAS NO SKID TRAIL
            skidTrails[i] = Instantiate(skidTrailPrefab);
        //                                 THEN INSTANTIATE SKID TRAIL PREFAB AND SET IT TO skidTrails[i]

        skidTrails[i].parent = wheelCols[i].transform;
        skidTrails[i].localRotation = Quaternion.Euler(90, 0, 0); // MAKES THE TRAIL FACE UP.
        skidTrails[i].localPosition = Vector3.down * wheelCols[i].radius; 
        //                              SET SKID-TRAILS LOCAL-POS TO THE v3.down OF THE WHEEL-COL RADIUS
    }
    void EndSkidTrail(int i)
    {
        if (skidTrails[i] == null) return; // IF NO SKID-TRAIL THEN NO NEED TO END ANYTHING

        Transform temp = skidTrails[i];

        skidTrails[i].parent = null;
        skidTrails[i] = null;
        temp.rotation = Quaternion.Euler(90, 0, 0); // WHEN TRAIL LOSES ITS PARENT ITS ROTATION WILL CHANGED,
        //                                                      THATS WHY WE AGAIN SETS ITS ROTATION, TO FACE UP

        Destroy(temp.gameObject, 30);
    }

}
