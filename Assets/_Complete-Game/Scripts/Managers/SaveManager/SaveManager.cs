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

        private EnemyManager enemyManager;
        private PlayerHealth playerHealth;

        // If true, apply game save on SceneManager.sceneLoaded callback
        private bool applyGameSaveOnSceneLoaded;

        // Deserialized game save data
        private GameSave saveData;
        private string savePath;

        private const string saveFileName = "save.dat";

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

            Debug.Log("OnSceneLoaded()");

            playerHealth = FindObjectOfType<PlayerHealth>();
            enemyManager = FindObjectOfType<EnemyManager>();

            ApplySaveDataToGame();
        }

        // Use this for initialization
        void Start()
        {
            Debug.Log("SaveManager.Start()");
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Save()
        {
            List<GameObject> enemies = enemyManager.GetAllAliveEnemies();

            // create save data object
            PlayerData playerData = CreatePlayerData();
            GameSave gameSaveData = new GameSave
            {
                PlayerData = playerData
            };

            SerializeSaveData(gameSaveData);
        }

        private PlayerData CreatePlayerData()
        {
            int health = playerHealth.currentHealth;
            Debug.Log("CreatePlayerData() health: " + health);
            int score = ScoreManager.score;
            return new PlayerData
            {
                Health = health,
                Score = score
            };
        }

        private void SerializeSaveData(GameSave gameSaveData)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.Create(savePath);
            binaryFormatter.Serialize(fileStream, gameSaveData);
            fileStream.Close();

            Debug.Log("SerializeSaveData()");
        }

        public void Load()
        {
            saveData = DeserializeSaveData();

            // TODO if saveData is empty, inform the user end return

            applyGameSaveOnSceneLoaded = true;

            // reload level
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }

        // TODO This could be wrapped in try/catch in case there's a null in the GameSave obj
        private void ApplySaveDataToGame()
        {
            int healthValue = saveData.PlayerData.Health;
            playerHealth.currentHealth = healthValue;
            // TODO this should be done automatically in the PlayerHealth class via property or setter method
            playerHealth.healthSlider.value = healthValue;
            Debug.Log("ApplySaveDataToGame() saveData.PlayerData.Health: " + saveData.PlayerData.Health);
            ScoreManager.score = saveData.PlayerData.Score;
            Debug.Log("ApplySaveDataToGame() playerHealth.currentHealth: " + playerHealth.currentHealth);
        }

        private GameSave DeserializeSaveData()
        {
            if (!File.Exists(savePath))
            {
                return new GameSave();
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.Open(savePath, FileMode.Open);
            GameSave saveData = (GameSave)binaryFormatter.Deserialize(fileStream);
            fileStream.Close();

            return saveData;
        }
    }

}
