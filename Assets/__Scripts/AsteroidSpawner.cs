using System;
using System.Collections.Generic;
using UnityEngine;
 public class AsteroidSpawner : MonoBehaviour
 {
     public GameObject[] AsteroidPrefabs;

     public int numberOfAsteroids;
 
     void Start()
     {
          SpawnAsteroids(numberOfAsteroids);
     }

#if UNITY_ANDROID || UNITY_IOS
     private List<Texture2D> textures = new List<Texture2D>();
     private void OnLowMemory()
    {
        Debug.Log("OnLowMemory, we had " + textures.Count + " textures in here!");
        textures = new List<Texture2D>();
        Resources.UnloadUnusedAssets();
    }

     void Awake() 
     {
          Application.lowMemory += OnLowMemory;
     }

     void Update()
     {
          var t = new Texture2D(2048, 2048, TextureFormat.ARGB32, true);
          t.Apply();
          this.textures.Add(t);
          Debug.Log("Update, we have " + this.textures.Count + " textures in here!");
     }
#endif

     private void SpawnAsteroids(int amount) 
     {
          
          for (int i=0; i < amount; i++) 
          {
               var pos = ScreenBounds.RANDOM_ON_SCREEN_LOC;
               var chosenAsteroid = AsteroidPrefabs[UnityEngine.Random.Range(0, AsteroidPrefabs.Length)];
               var asteroid = Instantiate(chosenAsteroid, pos, Quaternion.identity);
 
               // You can still manipulate asteroid afterwards like .AddComponent etc
          }
     }
 }