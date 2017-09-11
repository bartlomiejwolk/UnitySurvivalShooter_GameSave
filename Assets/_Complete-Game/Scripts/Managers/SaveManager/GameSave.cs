using System.Collections.Generic;

namespace CompleteProject
{
    [System.Serializable]
    public class GameSave
    {
        public GameData GameData;
        public PlayerData PlayerData;
        public List<EnemyData> EnemiesData;
    }
}
