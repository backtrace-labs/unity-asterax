using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Helpshift;

[RequireComponent(typeof(Rigidbody))]
public class PlayerShip : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float shipSpeed = 10f;

    // Define a texture and GUIContent
    private GUIContent button_tex_con;


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

    public Texture button_tex;

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
        
        // Define a GUIContent which uses the texture
        this.button_tex_con = new GUIContent(button_tex);
        this.button_tex_con.text = " Looks like you experienced a problem.\nClick here to report!";
    }

    public void OnFire()
    {
        Fire();
    }

    void OnGUI() {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical();
        // Starting a conversation with your customers
        if (GUILayout.Button(button_tex_con))
        {
            var configMap = new Dictionary<string, object>();
            Dictionary<string, string> backtraceid = new Dictionary<string, string>();
            backtraceid.Add("type", "singleline");
            backtraceid.Add("value", AsteraX.backtraceClient["guid"]);
            Dictionary<string, object> cifDictionary = new Dictionary<string, object>();
            cifDictionary.Add("device_id", backtraceid);
            //Map<String, Object> config = new HashMap<>();    
            configMap.Add("customIssueFields", cifDictionary);
            AsteraX.help.ShowConversation(configMap);
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
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
