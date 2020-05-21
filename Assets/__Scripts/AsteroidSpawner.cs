using UnityEngine;
 public class AsteroidSpawner : MonoBehaviour
 {
     public GameObject[] AsteroidPrefabs;
 
     void Start()
     {
          SpawnAsteroids(25);
     }
     
     private void SpawnAsteroids(int amount) 
     {
          
          for (int i=0; i < amount; i++) 
          {
               var pos = ScreenBounds.RANDOM_ON_SCREEN_LOC;
               var chosenAsteroid = AsteroidPrefabs[Random.Range(0, AsteroidPrefabs.Length)];
               var asteroid = Instantiate(chosenAsteroid, pos, Quaternion.identity);
 
               // You can still manipulate asteroid afterwards like .AddComponent etc
          }
     }
 }