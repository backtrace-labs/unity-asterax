﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltWithVelocity : MonoBehaviour
{
    [Tooltip("The number of degrees that the ship will tilt at its maximum speed.")]
    public int      degrees = 30;
    public bool     tiltTowards = true;

    private int     prevDegrees = int.MaxValue;
    private float   tan;

    Rigidbody rigid;

    // Use this for initialization
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Mathf.Tan() is a little expensive, so we can cache the result instead 
        //  of calculating each FixedUpdate.
        if (degrees != prevDegrees)
        {
            prevDegrees = degrees;
            tan = Mathf.Tan(Mathf.Deg2Rad * degrees);
        }
        transform.LookAt(transform.position + CalculatePitchDirection());
    }

    private Vector3 CalculatePitchDirection()
    {
        Vector3 pitchDir = (this.tiltTowards) ? -this.rigid.velocity : this.rigid.velocity;
        pitchDir += Vector3.forward / tan * PlayerShip.MAX_SPEED;

        if (pitchDir.x > 9f)
        {
            CalculateOverrideCoordinates();
        }

        return pitchDir;
    }

    void CalculateOverrideCoordinates()
    {
        CompensateForGyroscopeDrift();
    }

    void CompensateForGyroscopeDrift()
    {
        PlayerShip.GyroscopeDelta();
    }
}
