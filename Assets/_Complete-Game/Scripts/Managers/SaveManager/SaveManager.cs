using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        private GameSave saveData;
        private string savePath;

        // If true, apply game save on SceneManager.sceneLoaded callback
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

            savePath = Application.persistentDataPath + "/" + saveFileName;
            //Debug.Log(savePath);

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            playerHealth = FindObjectOfType<PlayerHealth>();
            enemyManager = FindObjectOfType<EnemyManager>();
        }

        // 
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            if (!applyGameSaveOnSceneLoaded)
            {
                return;
            }

            //Debug.Log("OnSceneLoaded()");

            playerHealth = FindObjectOfType<PlayerHealth>();
            enemyManager = FindObjectOfType<EnemyManager>();

            ApplySaveDataToGame();
        }

        // Use this for initialization
        void Start()
        {
            //Debug.Log("SaveManager.Start()");
            
        }

        // Update is called once per frame
        void Update()
        {

        }

#endregion UNITY_CALLBACKS

        #region SAVING

        public void Save()
        {
            //List<GameObject> enemies = enemyManager.GetAllAliveEnemies();

            // create save data object
            // TODO extract
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

            Debug.Log("Save(), playerData.Position: " + playerData.Position);

            return playerData;
        }

        private List<EnemyData> CreateEnemiesData()
        {
            List<GameObject> enemyGOs = enemyManager.GetAllAliveEnemies();

            foreach (var enemy in enemyGOs)
            {
                string prefabName = ConvertGameObjectNameToPrefabName(enemy.name);
                Debug.Log("CreateEnemiesData(), prefabName: " + prefabName);
                EnemyData enemyData = new EnemyData()
                {
                    
                };
            }

            return null;
        }

        private string ConvertGameObjectNameToPrefabName(string enemyName)
        {
            int parenthesisIndex = enemyName.IndexOf("(");
            string prefabName = enemyName.Substring(0, parenthesisIndex);

            return prefabName;
        }

        private SVector3 GetPlayerSerializablePosition()
        {
            Vector3 position = playerHealth.transform.position;
            SVector3 serializablePos = new SVector3(position);

            return serializablePos;
        }

        private void SerializeSaveData(GameSave gameSaveData)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.Create(savePath);
            binaryFormatter.Serialize(fileStream, gameSaveData);
            fileStream.Close();

            Debug.Log("SerializeSaveData()");
        }
        #endregion SAVING

        #region LOADING

        public void Load()
        {
            saveData = DeserializeSaveData();

            // TODO if saveData is empty, inform the user end return

            applyGameSaveOnSceneLoaded = true;

            // reload level
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
            // update camera position
            Vector3 cameraPos = saveData.GameData.CameraPosition.Base();
            Camera.main.transform.position = cameraPos;

            // update player health
            int healthValue = saveData.PlayerData.Health;
            playerHealth.currentHealth = healthValue;
            // TODO this should be done automatically in the PlayerHealth class via property or setter method
            playerHealth.healthSlider.value = healthValue;

            // update player position
            // TODO update through rigidbody instead of transform
            Rigidbody playerRigidbody = playerHealth.transform.GetComponent<Rigidbody>();
            //playerRigidbody.position = saveData.PlayerData.Position.Base();
            playerHealth.transform.position = saveData.PlayerData.Position.Base();
            Debug.Log("ApplySaveDataToGame(), playerData.Position: " + saveData.PlayerData.Position);
            //Debug.Log("ApplySaveDataToGame(), playerRigidbody.position: " + playerRigidbody.position);
            Debug.Log("ApplySaveDataToGame(), playerHealth.transform.position: " + playerHealth.transform.position);

            // update game score
            ScoreManager.score = saveData.PlayerData.Score;
            //Debug.Log("ApplySaveDataToGame() saveData.PlayerData.Health: " + saveData.PlayerData.Health);
        }

        // TODO This could be wrapped in try/catch in case there's a null in the GameSave obj
    }
#endregion LOADING

}
