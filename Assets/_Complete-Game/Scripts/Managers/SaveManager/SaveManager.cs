using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace CompleteProject
{
    public class SaveManager : MonoBehaviour
    {
        [SerializeField]
        private EnemyManager enemyManager;

        [SerializeField]
        private PlayerHealth playerHealth;

        private string savePath;

        void Awake()
        {
            // TODO create constant for the save file name
            savePath = Application.persistentDataPath + "/save.dat";
            Debug.Log(savePath);
        }

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
            GameSave saveData = DeserializeSaveData();
            ApplySaveDataToGame(saveData);
        }

        // TODO This could be wrapped in try/catch in case there's a null in the GameSave obj
        private void ApplySaveDataToGame(GameSave saveData)
        {
            int healthValue = saveData.PlayerData.Health;
            playerHealth.currentHealth = healthValue;
            // TODO this should be done automatically in the PlayerHealth class via property or setter method
            playerHealth.healthSlider.value = healthValue;
            Debug.Log("ApplySaveDataToGame() saveData.PlayerData.Health: " + saveData.PlayerData.Health);
            ScoreManager.score = saveData.PlayerData.Score;
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
