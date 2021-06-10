using UnityEngine;
using UnityEngine.Diagnostics;
using System.Collections.Generic;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(OffScreenWrapper))]
public class Asteroid : MonoBehaviour
{
    [Header("Set Dynamically")]
    public int          size = 3;
    public bool         immune = false;

    public int minVel = 5;
    public int maxVel = 10;
    
    public int maxAngularVel = 10;

    Rigidbody           rigid; // protected
    OffScreenWrapper    offScreenWrapper;
    
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        offScreenWrapper = GetComponent<OffScreenWrapper>();
    }


    // Start is called before the first frame update
    void Start()
    {
        this.transform.rotation = Random.rotation;
        InitVelocity();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitVelocity()
    {
        Vector3 vel;

        // The initial velocity depends on whether the Asteroid is currently off screen or not
        if (ScreenBounds.OOB(transform.position))
        {
            // If the Asteroid is out of bounds, just point it toward a point near the center of the sceen
            vel = ((Vector3)Random.insideUnitCircle * 4) - transform.position;
            vel.Normalize();
        }
        else
        {
            // If in bounds, choose a random direction, and make sure that when you Normalize it, it doesn't
            //  have a length of 0 (which might happen if Random.insideUnitCircle returned [0,0,0].
            do
            {
                vel = Random.insideUnitCircle;
                vel.Normalize();
            } while (Mathf.Approximately(vel.magnitude, 0f));
        }

        // Multiply the unit length of vel by the correct speed (randomized) for this size of Asteroid
        vel = vel * Random.Range(this.minVel, this.maxVel) / (float)size;
        rigid.velocity = vel;

        rigid.angularVelocity = Random.insideUnitSphere * this.maxAngularVel;
    }

    public void OnCollisionEnter(Collision coll)
    {
        if (immune)
        {
            return;
        }

        GameObject otherGO = coll.gameObject;

        if (otherGO.tag == "Bullet")
        {
            AsteroidHitByBullet(otherGO);
        }
        else if (otherGO.tag == "Player")
        {
            Destroy(gameObject);
            PlayerShip.S.health -= 5;

            AsteraX.GetBacktraceClient()["shipHealth"] = "" + PlayerShip.S.health;

            if (PlayerShip.S.health <= 0) {
                Destroy(otherGO);

                AsteraX.backtraceClient.Breadcrumbs.Info("Player Died!", new Dictionary<string, string>() {
                    {"application.version", AsteraX.backtraceClient["application.version"]},
                });

                // this crashes the entire game 
                Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
            }
        }  
        else if (otherGO.tag == "Asteroid") 
        {
            //Destroy(otherGO);
            //Destroy(gameObject);
        }        
    }

    void AsteroidHitByBullet(GameObject otherGO)
    {
        Destroy(otherGO);
        Destroy(gameObject);
        PlayerShip.S.bullets += 2;
        AsteraX.score += 10;

        try 
        {
            throw new System.NullReferenceException("Parameter cannot be null");
        }
        catch (System.NullReferenceException nre)
        {
            AsteraX.GetBacktraceClient().Send(nre);
        }
    }
}
