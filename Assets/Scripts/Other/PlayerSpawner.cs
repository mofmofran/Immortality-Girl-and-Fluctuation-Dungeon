using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private DungeonGenerator _dungeonGenerator;
    [SerializeField] private GameObject _playerPrefab;

    //プロパティ関連
    public GameObject PlayerInstance { get; private set; }
    public Vector2Int PlayerGridPosition { get; private set; }

    //メソッド関連
    //Playerを部屋にスポーンするメソッド
    public void SpawnPlayerInRandomRoom()
    {
        if (_dungeonGenerator == null || _dungeonGenerator.Rooms.Count == 0 || _playerPrefab == null)
            return;

        var rooms = _dungeonGenerator.Rooms;
        RectInt room = rooms[Random.Range(0, rooms.Count)];

        int x = Random.Range(room.xMin + 1, room.xMax - 1);
        int y = Random.Range(room.yMin + 1, room.yMax - 1);

        PlayerGridPosition = new Vector2Int(x, y);
        Vector3 worldPos = new Vector3(x + 0.5f, y + 0.5f, 0f);

        if (PlayerInstance == null)
        {
            PlayerInstance = Instantiate(_playerPrefab, worldPos, Quaternion.identity);
        }
        else
        {
            PlayerInstance.transform.position = worldPos;
        }
    }

    // プレイヤー移動時に呼び出して現在グリッド座標を更新する用
    public void UpdatePlayerGridPosition(Vector2Int newGridPos)
    {
        PlayerGridPosition = newGridPos;
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
