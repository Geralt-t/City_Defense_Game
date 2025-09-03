using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Game/Level Database")]
public class LevelDatabase : ScriptableObject
{
    public List<LevelDataSO> levels;

    public LevelDataSO GetLevel(int index)
    {
        if (index < 0 || index >= levels.Count)
            return null;
        return levels[index];
    }

    public int TotalLevels => levels.Count;
}
