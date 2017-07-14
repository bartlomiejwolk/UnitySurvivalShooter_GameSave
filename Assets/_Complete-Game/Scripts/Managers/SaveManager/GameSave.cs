using System.Collections.Generic;

namespace CompleteProject
{
    [System.Serializable]
    public struct GameSave
    {
        public GameData GameData;
        public PlayerData PlayerData;
        public List<EnemyData> EnemiesData;
    }
}
