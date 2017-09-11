using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CompleteProject
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager instance;

        // Settings
        private const string saveFileName = "save.dat";

        // Refs
        private EnemyManager enemyManager;
        private PlayerHealth playerHealth;

        // Helpers
        // TODO see if you can remove any of them
        private GameSave saveData;
        private string savePath;

        // If true, apply game save on SceneManager.sceneLoaded callback
        // TODO see if using flag can be avoided
        private bool applyGameSaveOnSceneLoaded;

        #region UNITY_CALLBACKS

        void Awake()
        {
            // init singleton
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                if (this != instance)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            InitClassMembers();
        }

        private void InitClassMembers()
        {
            savePath = Application.persistentDataPath + "/" + saveFileName;

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            playerHealth = FindObjectOfType<PlayerHealth>();
            enemyManager = FindObjectOfType<EnemyManager>();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            if (!applyGameSaveOnSceneLoaded)
            {
                return;
            }

            playerHealth = FindObjectOfType<PlayerHealth>();
            enemyManager = FindObjectOfType<EnemyManager>();

            ApplySaveDataToGame();
        }

#endregion UNITY_CALLBACKS

        #region SAVING

        public void Save()
        {
            if (!playerHealth) return;
            if (!enemyManager) return;
            if (!Camera.main) return;

            // TODO log info about null refs.

            GameData gameData = CreateGameData();
            PlayerData playerData = CreatePlayerData();
            List<EnemyData> enemiesData = CreateEnemiesData();

            GameSave gameSaveData = new GameSave
            {
                PlayerData = playerData,
                GameData = gameData,
                EnemiesData = enemiesData
            };

            SerializeSaveData(gameSaveData);
        }

        private GameData CreateGameData()
        {
            // get camera position
            Vector3 cameraPos = Camera.main.transform.position;
            SVector3 serializableCameraPos = new SVector3(cameraPos);

            GameData data = new GameData()
            {
                CameraPosition = serializableCameraPos
            };

            return data;
        }

        private PlayerData CreatePlayerData()
        {
            int health = playerHealth.currentHealth;
            int score = ScoreManager.score;
            SVector3 position = GetPlayerSerializablePosition();

            PlayerData playerData = new PlayerData
            {
                Health = health,
                Score = score,
                Position = position
            };

            return playerData;
        }

        private List<EnemyData> CreateEnemiesData()
        {
            List<EnemyData> output = new List<EnemyData>();

            List<GameObject> enemyGOs = enemyManager.GetAllAliveEnemies();
            foreach (var enemy in enemyGOs)
            {
                EnemyData enemyData = CreateEnemyData(enemy);
                output.Add(enemyData);
            }

            return output;
        }

        private EnemyData CreateEnemyData(GameObject enemy)
        {
            string prefabName = ConvertGameObjectNameToPrefabName(enemy.name);
            int enemyHealth = GetEnemyHealth(enemy);
            SVector3 enemySerializablePos = GetEnemySerializablePosition(enemy);
            SVector3 serializableRot = GetEnemySerializableRotation(enemy);

            EnemyData enemyData = new EnemyData()
            {
                PrefabName = prefabName,
                Health = enemyHealth,
                Position = enemySerializablePos,
                Rotation = serializableRot
            };
            return enemyData;
        }

        private SVector3 GetPlayerSerializablePosition()
        {
            Vector3 position = playerHealth.transform.position;
            SVector3 serializablePos = new SVector3(position);

            return serializablePos;
        }

        private static SVector3 GetEnemySerializableRotation(GameObject enemy)
        {
            Vector3 rot = enemy.transform.rotation.eulerAngles;
            SVector3 serializableRot = new SVector3(rot);
            return serializableRot;
        }

        private static SVector3 GetEnemySerializablePosition(GameObject enemy)
        {
            Vector3 enemyPos = enemy.transform.position;
            SVector3 enemySerializablePos = new SVector3(enemyPos);
            return enemySerializablePos;
        }

        private static int GetEnemyHealth(GameObject enemy)
        {
            EnemyHealth enemyHealthComp = enemy.GetComponent<EnemyHealth>();
            int enemyHealth = 0;
            if (enemyHealthComp)
            {
                enemyHealth = enemyHealthComp.currentHealth;
            }
            return enemyHealth;
        }

        private string ConvertGameObjectNameToPrefabName(string enemyName)
        {
            int parenthesisIndex = enemyName.IndexOf("(", StringComparison.Ordinal);
            string prefabName = enemyName.Substring(0, parenthesisIndex);

            return prefabName;
        }

        private void SerializeSaveData(GameSave gameSaveData)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.Create(savePath);
            binaryFormatter.Serialize(fileStream, gameSaveData);
            fileStream.Close();
        }

        #endregion SAVING

        #region LOADING

        // TODO This could be wrapped in try/catch in case there's a null in the GameSave obj
        public void Load()
        {
            saveData = DeserializeSaveData();
            // TODO if saveData is empty (check timestamp), inform the user end return.
            applyGameSaveOnSceneLoaded = true;
            ReloadLevel();
        }

        private static void ReloadLevel()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }

        private GameSave DeserializeSaveData()
        {
            if (!File.Exists(savePath))
            {
                return new GameSave();
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.Open(savePath, FileMode.Open);
            GameSave save = (GameSave)binaryFormatter.Deserialize(fileStream);
            fileStream.Close();

            return save;
        }

        private void ApplySaveDataToGame()
        {
            if (!playerHealth) return;
            if (!enemyManager) return;
            if (!Camera.main) return;

            ApplyCameraPosition();
            ApplyPlayerPosition();
            ApplyPlayerHealth();
            ScoreManager.score = saveData.PlayerData.Score;
            ApplyEnemySaveData();
        }

        private void ApplyCameraPosition()
        {
            Vector3 cameraPos = saveData.GameData.CameraPosition.Base();
            Camera.main.transform.position = cameraPos;
        }

        private void ApplyPlayerPosition()
        {
            // TODO update through rigidbody instead of transform
            playerHealth.transform.position = saveData.PlayerData.Position.Base();
        }

        private void ApplyPlayerHealth()
        {
            int healthValue = saveData.PlayerData.Health;
            playerHealth.currentHealth = healthValue;
            // TODO this should be done automatically in the PlayerHealth class via property or setter method
            playerHealth.healthSlider.value = healthValue;
        }

        // TODO Don't use Resources folder. Use direct prefab references.
        private void ApplyEnemySaveData()
        {
            foreach (var enemyData in saveData.EnemiesData)
            {
                InstantiateEnemy(enemyData);
            }
        }

        private void InstantiateEnemy(EnemyData enemyData)
        {
            Object prefab = Resources.Load(enemyData.PrefabName);
            Vector3 pos = enemyData.Position.Base();
            Vector3 rot = enemyData.Rotation.Base();
            Quaternion quaternionRot = Quaternion.Euler(rot);
            GameObject instanceRef = (GameObject) Instantiate(prefab, pos, quaternionRot);
            EnemyHealth healthComp = instanceRef.GetComponent<EnemyHealth>();
            if (healthComp)
            {
                healthComp.currentHealth = enemyData.Health;
            }

            // update `EnemyManager`
            enemyManager.AddSpawnedEnemy(instanceRef);
        }
    }
#endregion LOADING

}
