using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Backtrace.Unity;
using Backtrace.Unity.Model;

[RequireComponent(typeof(Rigidbody))]
public class PlayerShip : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float shipSpeed = 10f;

    [DllImport("kernel32.dll")]
    static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags,  uint nNumberOfArguments, IntPtr lpArguments);


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

    public int health = 100;

    public int bullets;
    void Start()
    {
        S = this;

        // NOTE: We don't need to check whether or not rigid is null because of 
        //  [RequireComponent( typeof(Rigidbody) )] above
        rigid = GetComponent<Rigidbody>();
    }

    public void OnFire()
    {
        Debug.Log("OnFire");
        Fire();
    }

    public void OnMove(InputValue value)
    {
        // Using Horizontal and Vertical axes to set velocity
        Vector2 vel = value.Get<Vector2>();

        if (vel.magnitude > 1)
        {
            //Avoid speed multiplying by 1.414 when moving at a diagonal
            vel.Normalize();
        }

        rigid.velocity = vel * shipSpeed;

        // Mouse input for firing

    }

    void Fire()
    {
        Debug.Log("OnFire");
        if (this.bullets <= 0) {
            return;
        }

        // Get direction to the mouse
        Vector3 mPos = Input.mousePosition;
        mPos.z = -Camera.main.transform.position.z;
        Vector3 mPos3D = Camera.main.ScreenToWorldPoint(mPos);

        // Instantiate the Bullet and set its direction
        GameObject go = Instantiate<GameObject>(bulletPrefab);
        go.transform.position = transform.position;
        go.transform.LookAt(mPos3D);

        this.bullets -= 1;

        var switchVar = UnityEngine.Random.Range(0, 4);
        switch (switchVar)
        {
            case 0:
                try
                {
                    throw new System.Exception("Parameter cannot be null");
                }
                catch (System.Exception e)
                {
                    var report = new BacktraceReport(
                        exception: e
                    );

                    AsteraX.GetBacktraceClient().Send(report);
                }
                break;
            case 1:
                throw new System.InsufficientMemoryException("Insuff mem.");
            case 2:
                System.IO.File.ReadAllBytes("Path to not existing file");
                break;
            case 3:
                int x = 0;
                int y = 100 / x;
                break;
            default:
                Debug.Log("No error.");
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
