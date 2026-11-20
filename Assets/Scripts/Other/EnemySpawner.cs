using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private DungeonGenerator _dungeonGenerator;
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private GameObject[] _enemyPrefabs;
    [SerializeField] private int _enemyCount = 5;

    public void SpawnEnemies()
    {
        if (_dungeonGenerator == null || _dungeonGenerator.Rooms.Count == 0) return;
        if (_enemyPrefabs == null || _enemyPrefabs.Length == 0) return;

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

            GameObject prefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];
            GameObject enemyObj = Instantiate(
                prefab,
                new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f),
                Quaternion.identity
            );

            // 生成した敵に参照を渡す場合（EnemyController側でInitializeを用意しておくと便利）
            if (enemyObj.TryGetComponent(out EnemyController enemy))
            {
                enemy.Initialize(_dungeonGenerator, _playerSpawner);
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
