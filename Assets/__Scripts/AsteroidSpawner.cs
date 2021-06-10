using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 public class AsteroidSpawner : MonoBehaviour
 {
     public GameObject[] AsteroidPrefabs;

     public int numberOfAsteroids;

     public float spawnRate = 0.5f;
 
     void Start()
     {
          SpawnAsteroid();
     }

#if ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR)
     private List<Texture2D> textures = new List<Texture2D>();
     private void OnLowMemory()
    {
        Debug.Log("OnLowMemory, we have " + textures.Count + " asteroid textures!");
        //Debug.LogError("trigger error");
        //textures = new List<Texture2D>();
        //Resources.UnloadUnusedAssets();
    }

     void Awake() 
     {
          Application.lowMemory += OnLowMemory;
     }

     void FixedUpdate()
     {
          if (GameObject.FindGameObjectsWithTag("Asteroid").Length > (numberOfAsteroids/2)) 
          {
               var t = new Texture2D(1024, 1024, TextureFormat.ARGB32, true);
               t.Apply();
               this.textures.Add(t);
               //Debug.Log("Update, we have " + this.textures.Count + " textures in here!");
          }
     }
#endif
     void SpawnAsteroid()
     {
          int currentNumberOfAsteroids = GameObject.FindGameObjectsWithTag("Asteroid").Length;
          if (currentNumberOfAsteroids < numberOfAsteroids)
          {
               var pos = ScreenBounds.RANDOM_ON_EDGE_SCREEN_LOC;
               var chosenAsteroid = AsteroidPrefabs[UnityEngine.Random.Range(0, AsteroidPrefabs.Length)];

               AsteraX.backtraceClient.Breadcrumbs.Info("Spawning asteroid", new Dictionary<string, string>() {
                    {"application.version", AsteraX.backtraceClient["application.version"]},
                    {"currentNumberOfAsteroids", "" + currentNumberOfAsteroids}
                });


               var asteroid = Instantiate(chosenAsteroid, pos, Quaternion.identity);
          }
          else
          {
               AsteraX.backtraceClient.Breadcrumbs.Info("Skipping spawning asteroid", new Dictionary<string, string>() {
                    {"application.version", AsteraX.backtraceClient["application.version"]},
                    {"currentNumberOfAsteroids", "" + currentNumberOfAsteroids}
                });

          }

          Invoke("SpawnAsteroid", spawnRate);
     }
 }