using UnityEngine;
using System.Collections.Generic;

namespace CompleteProject
{
    public class EnemyManager : MonoBehaviour
    {
        public PlayerHealth playerHealth;       // Reference to the player's heatlh.
        public GameObject enemy;                // The enemy prefab to be spawned.
        public float spawnTime = 3f;            // How long between each spawn.
        public Transform[] spawnPoints;         // An array of the spawn points this enemy can spawn from.

        // TODO Make it instance field
        private static readonly List<GameObject> aliveEnemies = new List<GameObject>();

        void Awake()
        {
            // If those values persisted after level reload,
            // the game save functionality would try to access refs.
            // to destroyed game objects.
            aliveEnemies.Clear();   
        }

        void Start ()
        {
            // Call the Spawn function after a delay of the spawnTime and then continue to call after the same amount of time.
            InvokeRepeating ("Spawn", spawnTime, spawnTime);
        }

        public void AddSpawnedEnemy(GameObject enemy)
        {
            aliveEnemies.Add(enemy);
        }

        public List<GameObject> GetAllAliveEnemies()
        {
            return aliveEnemies;
        }

        public void NotifyEnemyDied(GameObject deadEnemy)
        {
            //Debug.Log("NotifyEnemyDied(), enemy count: " + aliveEnemies.Count);
            aliveEnemies.Remove(deadEnemy);
            //Debug.Log("NotifyEnemyDied(), new enemies count: " + aliveEnemies.Count);
        }

        void Spawn ()
        {
            // If the player has no health left...
            if(playerHealth.currentHealth <= 0f)
            {
                // ... exit the function.
                return;
            }

            // Find a random index between zero and one less than the number of spawn points.
            int spawnPointIndex = Random.Range (0, spawnPoints.Length);

            // Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
            GameObject enemyRef = Instantiate (enemy, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);

            // Remember all spawned enemies
            aliveEnemies.Add(enemyRef);
            //Debug.Log("Enemy spawned! Enemies: " + aliveEnemies.Count);
        }
    }
}