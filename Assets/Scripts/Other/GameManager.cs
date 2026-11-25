using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private DungeonGenerator _dungeonGenerator;
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private DungeonEventManager _dungeonEventManager;

    private void Start()
    {
        _dungeonGenerator.GenerateDungeon();
        _dungeonGenerator.PrintMapToConsole();

        _playerSpawner.SpawnPlayerInRandomRoom();
        _enemySpawner.SpawnEnemies();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
