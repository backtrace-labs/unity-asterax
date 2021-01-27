using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
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

    public int health = 100;

    public int bullets;
    void Start()
    {
        S = this;

        // GetInt doesn't work reliably on iOS so revert to GetString
        String shipHealthFromPrefs = PlayerPrefs.GetString("ship_health");
        if (!String.IsNullOrWhiteSpace(shipHealthFromPrefs))
        {
            Debug.Log("ship health from playerprefs: " + shipHealthFromPrefs);
            PlayerShip.S.health = Int32.Parse(shipHealthFromPrefs);
        }
        

        // NOTE: We don't need to check whether or not rigid is null because of 
        //  [RequireComponent( typeof(Rigidbody) )] above
        rigid = GetComponent<Rigidbody>();

        if (UnityEngine.InputSystem.Gyroscope.current != null)
        {
            Debug.Log("Gyro enabling..");
            InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
            Debug.Log("Gyro enabled.");
        }
        else
        {
            Debug.Log("No Gyro.");
        }
    }

    public void OnFire()
    {
        Fire();
    }

    public void OnMove(InputValue value)
    {
        Vector2 vel = value.Get<Vector2>();
        rigid.velocity = vel * shipSpeed;
    }

    void Update() 
    {
        if (UnityEngine.InputSystem.Gyroscope.current != null) 
        {
            Vector3 vel = UnityEngine.InputSystem.Gyroscope.current.angularVelocity.ReadValue();
            rigid.velocity = vel * shipSpeed;
        }
    }

    void Fire()
    {
        if (this.bullets <= 0) {
            return;
        }

        // Get direction to the mouse
        Vector3 mPos = Input.mousePosition;
        mPos.z = -Camera.main.transform.position.z;
        Vector3 mPos3D = Camera.main.ScreenToWorldPoint(mPos);
    
        // Instantiate the Bullet and set its direction
        SpawnBullet(mPos3D);

        this.bullets -= 1;

        if (this.bullets < 3)
        {
            System.IO.File.ReadAllBytes("Path to not existing file");
        }
    }

    void SpawnBullet(Vector3 mPos3D)
    {
        GameObject go = Instantiate<GameObject>(this.bulletPrefab);
        go.transform.position = transform.position;
        go.transform.LookAt(mPos3D);
    }

    static public void GyroscopeDelta()
    {
        AsteraX.GetGyroscopeDevice();
    }

    static public float MAX_SPEED
    {
        get
        {
            return S.shipSpeed;
        }
    }
}
