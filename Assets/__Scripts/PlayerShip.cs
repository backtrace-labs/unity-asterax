using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using Backtrace.Unity;
using Backtrace.Unity.Model;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BacktraceClient))]
public class PlayerShip : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float        shipSpeed = 10f;

    // This is a somewhat protected private singleton for PlayerShip
    static private PlayerShip   _S;
    static public PlayerShip    S
    {
        get
        {
            return _S;
        }
        private set
        {
            if (_S != null)
            {
                Debug.LogWarning("Second attempt to set PlayerShip singleton _S.");
            }
            _S = value;
        }
    }

    Rigidbody           rigid;
    public GameObject   bulletPrefab;

    void Start()
    {
        S = this;

        // NOTE: We don't need to check whether or not rigid is null because of 
        //  [RequireComponent( typeof(Rigidbody) )] above
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Using Horizontal and Vertical axes to set velocity
        float aX = CrossPlatformInputManager.GetAxis("Horizontal");
        float aY = CrossPlatformInputManager.GetAxis("Vertical");

        Vector3 vel = new Vector3(aX, aY);
        if (vel.magnitude > 1)
        {
            // Avoid speed multiplying by 1.414 when moving at a diagonal
            vel.Normalize();
        }

        rigid.velocity = vel * shipSpeed;

        // Mouse input for firing
        if (CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            Fire();
        }
    }

    void Fire()
    {
        // Get direction to the mouse
        Vector3 mPos = Input.mousePosition;
        mPos.z = -Camera.main.transform.position.z;
        Vector3 mPos3D = Camera.main.ScreenToWorldPoint(mPos);

        // Instantiate the Bullet and set its direction
        GameObject go = Instantiate<GameObject>(bulletPrefab);
        go.transform.position = transform.position;
        go.transform.LookAt(mPos3D);

         //Read from manager BacktraceClient instance
        var backtraceClient = GetComponent<BacktraceClient>();
        try 
        {
            throw new Exception("Fatal error drawing the bullet");
        }
        catch (Exception e)
        {
            var report = new BacktraceReport(e);
            backtraceClient.Send(report);
        }
        

        Invoke("Fire", 0.1f);   
    }


    static public float MAX_SPEED
    {
        get
        {
            return S.shipSpeed;
        }
    }
}
