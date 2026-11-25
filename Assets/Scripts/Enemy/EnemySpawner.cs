using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public EnemyBaseData enemyBaseData;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private DungeonGenerator _dungeonGenerator;
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private EnemySpawnData[] _enemySpawnDatas;
    [SerializeField] private int _enemyCount = 5;//初期配置の数
    [SerializeField] private int _spawnInterval = 40;//スポーン間隔
    [SerializeField] private int _maxEnemyCount = 18;//敵の最大数
    [SerializeField] private int _initialEnemyCount;//敵の現在数格納用

    public void SpawnEnemies()
    {
        if (_dungeonGenerator == null || _dungeonGenerator.Rooms.Count == 0) return;
        if (_enemySpawnDatas == null || _enemySpawnDatas.Length == 0) return;

        List<Vector2Int> candidates = new List<Vector2Int>();
        Vector2Int playerPos = _playerSpawner != null ? _playerSpawner.PlayerGridPosition : Vector2Int.zero;

        foreach (RectInt room in _dungeonGenerator.Rooms)
        {
            for (int x = room.xMin + 1; x < room.xMax - 1; x++)
            {
                for (int y = room.yMin + 1; y < room.yMax - 1; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (_playerSpawner != null && pos == playerPos) continue;
                    candidates.Add(pos);
                }
            }
        }

        for (int i = 0; i < _enemyCount; i++)
        {
            if (candidates.Count == 0) break;

            int index = Random.Range(0, candidates.Count);
            Vector2Int pos = candidates[index];
            candidates.RemoveAt(index);

            EnemySpawnData spawnData =_enemySpawnDatas[Random.Range(0, _enemySpawnDatas.Length)];

            GameObject enemyObj = Instantiate(
                spawnData.enemyPrefab,
                new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f),
                Quaternion.identity
            );

            // 生成した敵に参照を渡す場合（EnemyController側でInitializeを用意しておくと便利）
            if (enemyObj.TryGetComponent(out EnemyController enemy))
            {
                enemy.Initialize(_dungeonGenerator, _playerSpawner);
                enemy.SetBaseData(spawnData.enemyBaseData);
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
