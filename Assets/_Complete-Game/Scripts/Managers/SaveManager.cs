using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CompleteProject
{
    public class SaveManager : MonoBehaviour
    {
        [SerializeField]
        private EnemyManager enemyManager;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Save()
        {
            List<GameObject> enemies = enemyManager.GetAllAliveEnemies();
        }

        public void Load()
        {

        }
    }

}
