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
          Invoke("SpawnAsteroid", spawnRate);
     }

#if ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR)
     private List<Texture2D> textures = new List<Texture2D>();
     private void OnLowMemory()
    {
        Debug.Log("OnLowMemory, we had " + textures.Count + " textures in here!");
        Debug.LogError("trigger error");
        textures = new List<Texture2D>();
        Resources.UnloadUnusedAssets();
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
               Debug.Log("Update, we have " + this.textures.Count + " textures in here!");
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
               var asteroid = Instantiate(chosenAsteroid, pos, Quaternion.identity);

               // Breadcrumb bc = new Breadcrumb();
               // bc.message = "new asteroid spawned!";
               // bc.attributes = new Dictionary<string, string>();
               // bc.attributes.Add("currentNumberOfAsteroids", currentNumberOfAsteroids.ToString());
               // bc.attributes.Add("type",chosenAsteroid.name);
               // AsteraX.bcw.AddBreadcrumb(bc);

               Invoke("SpawnAsteroid", spawnRate);
          }
     }
 }