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
    public float shipSpeed = 10f;

    // This is a somewhat protected private singleton for PlayerShip
    static private PlayerShip _S;
    static public PlayerShip S
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

    Rigidbody rigid;
    public GameObject bulletPrefab;

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
        if (CrossPlatformInputManager.GetButtonDown("Fire1") || Input.touchCount > 0)
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

        //Grab unhandled exceptions too
        var backtraceClient = GetComponent<BacktraceClient>();

        var switchVar = UnityEngine.Random.Range(0, 4);
        Debug.Log($"Switch argument: ${ switchVar }");
        switch (switchVar)
        {
            case 0:
                try
                {
                    Debug.Log("Okay, throwing a parameter cannot be null exception now.");
                    throw new System.Exception("Parameter cannot be null");
                }
                catch (System.Exception e)
                {
                    var report = new BacktraceReport(
                        exception: e,
                        //Adding an attribute that will show in Backtrace
                        //as "Testing" with a value of "True"
                        attributes: new Dictionary<string, object>() { { "Testing", "True" } }
                    );

                    backtraceClient.Send(report);
                }
                break;
            case 1:
                try
                {
                    Debug.Log("Okay, throwing out of memory exception now.");
                    throw new InsufficientMemoryException("Insuff mem.");
                }
                catch (InsufficientMemoryException e)
                {
                    var report = new BacktraceReport(e);
                    backtraceClient.Send(report);
                }
                break;
            case 2:
                try
                {
                    Debug.Log("Throwing a file not found exception now.");
                    System.IO.File.ReadAllBytes("Path to not existing file");
                }
                catch (Exception e)
                {
                    var report = new BacktraceReport(e);
                    backtraceClient.Send(report);
                }
                break;
            case 3:
                {
                    int x = 0;
                    int y = 100 / x;
                }
                break;
            default:
                Debug.Log("Okay, bai!");
                Invoke("Fire", 1);
                break;
        }
    }
    static public float MAX_SPEED
    {
        get
        {
            return S.shipSpeed;
        }
    }
}
