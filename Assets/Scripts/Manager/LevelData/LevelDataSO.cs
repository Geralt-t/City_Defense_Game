using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data", order = 0)]
public class LevelDataSO : ScriptableObject
{
    public int levelIndex;

    [Header("Defense Line")]
    public Vector3 defenseLinePosition;

    [Header("WaveStats")]
    public WaveEnemyStats waveEnemyStats;

    [Header("Camera")]
    public Vector3 cameraPosition;
}


