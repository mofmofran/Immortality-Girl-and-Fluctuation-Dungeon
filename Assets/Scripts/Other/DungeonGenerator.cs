using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    [Header("マップタイル")]

    [SerializeField] private TileBase _wallTile;  // 壁のスプライト
    [SerializeField] private TileBase _floorTile; // 床のスプライト
    [Header("マップのサイズ")]
    [SerializeField] private int _mapWidth = 40;
    [SerializeField] private int _mapHeight = 25;

    [Header("部屋の数")]
    [SerializeField] private int _roomCount = 6;
    [Header("部屋同士の間隔")]
    [SerializeField] private int _roomMargin = 3;

    [Header("部屋のサイズ")]
    [SerializeField] private int _minRoomSize = 4;
    [SerializeField] private int _maxRoomSize = 8;
    [Header("部屋作成の最大試行回数")]
    [SerializeField] private int _maxRoomCreateCount = 10;
    [Header("タイルマップオブジェクト")]
    [SerializeField] private Tilemap _wallTilemap;
    [SerializeField] private Tilemap _floorTilemap;
    [Header("通路の直線距離")]
    [SerializeField] private int _minStraightLength = 3;
    [SerializeField] private int _maxStraightLength = 10;
    [Header("枝分かれ設定")]
    [SerializeField] private int _branchCount = 4;
    [SerializeField] private int _minBranchLength = 4;
    [SerializeField] private int _maxBranchLength = 5;
    [Header("部屋からの枝分かれ")]
    [SerializeField] private int _roomBranchCount = 3;
    [SerializeField] private int _minRoomBranchLength = 4;
    [SerializeField] private int _maxRoomBranchLength = 5;
    [Header("プレイヤー")]
    [SerializeField] private GameObject _player;
    [Header("敵の生成")]
    [SerializeField] private GameObject[] _enemyPrefabs;
    [SerializeField] private int _enemyCount = 5;

    private Vector2Int _playerGridPosition;//プレイヤーの位置

    // マップデータ（二次元配列）
    private int[,] _map;

    // 部屋の中心点をリストに保存
    private readonly List<Vector2Int> _roomCenters = new List<Vector2Int>();//readonlyは読み取り専用
    // 部屋の位置と大きさを保存
　　private readonly List<RectInt> _rooms = new List<RectInt>();



    //メソッド関連
    private void MapGenerate()//マップ全体の生成
    {
        //マップの初期化
        _map = new int[_mapWidth, _mapHeight];
        // 中心点のリストを初期化
        _roomCenters.Clear();//Clearメソッドはリストから全ての要素を削除する
        _rooms.Clear();//roomsリストを初期化

        //部屋数になるように部屋を生成する
        for (int i = 0; i < _roomCount; i++)//int iが部屋の数になるように繰り返す
        {
            CreateRoom();
        }
        ConnectRoomsInOrder();

        CreateRoomBranches();

        InstantiateTile();
    }


    private void CreateRoom()//部屋の生成
    {
        for (int i = 0; i < _maxRoomCreateCount; i++)//部屋生成の試行回数まで繰り返す
        {
            // 部屋の大きさを決める
            int roomWidth = Random.Range(_minRoomSize, _maxRoomSize + 1);//Random.Rangeは(第一引数,第二引数－1)
            int roomHeight = Random.Range(_minRoomSize, _maxRoomSize + 1);

            // 部屋の位置を決める
            int roomX = Random.Range(1, _mapWidth - roomWidth - 1);//1～マップの大きさから部屋の大きさを引いて(差を出して)そこから1マス余裕を持たせた数値
            int roomY = Random.Range(1, _mapHeight - roomHeight - 1);

            if (IsOverlapping(roomX, roomY, roomWidth, roomHeight)) continue;//

            for (int x = roomX; x < roomX + roomWidth; x++)//int x(部屋のX軸位置)がX軸＋部屋の横幅になるまで繰り返す
            {
                for (int y = roomY; y < roomY + roomHeight; y++)//int y(部屋のy軸位置)がy軸＋部屋の縦幅になるまで繰り返す
                {
                    _map[x, y] = 1;
                }
            }

            // 中心点を保存
            int currentX = roomX + roomWidth / 2;
            int currentY = roomY + roomHeight / 2;
            _roomCenters.Add(new Vector2Int(currentX, currentY));//縦と横の半分をVector2型でリストに追加
            _rooms.Add(new RectInt(roomX, roomY, roomWidth, roomHeight));// 部屋の位置と大きさを保存
            return; // 部屋作成成功で終了
        }
    }

    private bool IsOverlapping(int roomX, int roomY, int roomWidth, int roomHeight)//部屋が重なっていないかの判定メソッド
    {
        for (int x = roomX - _roomMargin; x < roomX + roomWidth + _roomMargin; x++)//xの部屋間が+-_roomMarginになるまで繰り返す
        {
            for (int y = roomY - _roomMargin; y < roomY + roomHeight + _roomMargin; y++)//yの部屋間が+-ルームの縦幅＋_roomMarginになるまで繰り返す
            {
                // マップの範囲外をチェック
                if (x < 0 || x >= _mapWidth || y < 0 || y >= _mapHeight) continue;//xとyがマイナスか部屋の幅以上の場合続ける

                // 部屋の範囲内に既に床があるかチェック
                if (_map[x, y] == 1)//既に配列に1が入っているか
                {
                    return true; // 重なっている
                }
            }
        }
        return false; // 重なっていない
    }

    private void ConnectRoomsInOrder()//部屋の中心から別の部屋への通路を作るメソッド
    {

        // 部屋が2つ未満なら終了
        if (_rooms.Count < 2) return;

        for (int i = 0; i < _rooms.Count - 1; i++)
        {
            // 接続する部屋を取得
            RectInt fromRoom = _rooms[i];
            RectInt toRoom = _rooms[i + 1];

            // 部屋の壁から接続位置をランダムに取得
            Vector2Int from = GetRandomRoomEdge(fromRoom);
            Vector2Int to = GetRandomRoomEdge(toRoom);

            // ランダムな通路を生成
            CarveCorridorL(from, to);
        }
    }

    private void CarveCorridorL(Vector2Int from, Vector2Int to)//
    {
        // 現在位置
        Vector2Int current = from;

        // 出発地点に床を設置
        _map[current.x, current.y] = 1;

        // 目的地に到着するまで繰り返す
        while (current != to)
        {
            // X軸・Y軸のどちらに進む必要があるか
            bool moveX = current.x != to.x;
            bool moveY = current.y != to.y;

            // 今回直進する距離を決める
            int distance = Random.Range(
                _minStraightLength,
                _maxStraightLength + 1
            );

            // 決めた距離だけ直進する
            for (int i = 0; i < distance; i++)
            {
                if (current == to)
                    break;

                if (moveX)
                {
                    current.x += current.x < to.x ? 1 : -1;
                }
                else if (moveY)
                {
                    current.y += current.y < to.y ? 1 : -1;
                }
                // 移動した場所に床を設置
                _map[current.x, current.y] = 1;
            }
        }
    }

    private void CreateBranches()//通路の枝分かれメソッド
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        // 枝分かれできそうな通路を探す
        for (int x = 1; x < _mapWidth - 1; x++)
        {
            for (int y = 1; y < _mapHeight - 1; y++)
            {
                if (_map[x, y] != 1) continue;

                bool left = _map[x - 1, y] == 1;
                bool right = _map[x + 1, y] == 1;
                bool up = _map[x, y + 1] == 1;
                bool down = _map[x, y - 1] == 1;

                // 横方向にまっすぐ続く通路
                if (left && right && !up && !down)
                {
                    candidates.Add(new Vector2Int(x, y));
                }
                // 縦方向にまっすぐ続く通路
                else if (up && down && !left && !right)
                {
                    candidates.Add(new Vector2Int(x, y));
                }
            }
        }

        // 枝分かれを作る
        for (int i = 0; i < _branchCount; i++)
        {
            if (candidates.Count == 0) break;

            // ランダムに枝分かれ地点を選択
            int index = Random.Range(0, candidates.Count);
            Vector2Int start = candidates[index];

            candidates.RemoveAt(index);

            // 通路の向きを調べる
            bool horizontal =
                _map[start.x - 1, start.y] == 1 &&
                _map[start.x + 1, start.y] == 1;

            Vector2Int direction;

            // 元の通路に対して垂直方向に伸ばす
            if (horizontal)
            {
                direction = Random.value < 0.5f
                    ? Vector2Int.up
                    : Vector2Int.down;
            }
            else
            {
                direction = Random.value < 0.5f
                    ? Vector2Int.left
                    : Vector2Int.right;
            }

            int length = Random.Range(
                _minBranchLength,
                _maxBranchLength + 1
            );

            Vector2Int current = start;
            bool canCreate = true;

            // 伸ばす先に障害物がないか確認
            for (int j = 0; j < length; j++)
            {
                current += direction;

                if (current.x <= 0 || current.x >= _mapWidth - 1 ||
                    current.y <= 0 || current.y >= _mapHeight - 1)
                {
                    canCreate = false;
                    break;
                }

                if (_map[current.x, current.y] == 1)
                {
                    canCreate = false;
                    break;
                }
            }

            // 問題がなければ通路を生成
            if (canCreate)
            {
                current = start;

                for (int j = 0; j < length; j++)
                {
                    current += direction;
                    _map[current.x, current.y] = 1;
                }
            }
        }
    }
    private void CreateRoomBranches()//部屋から行き止まりを作るメソッド
    {
        if (_rooms.Count == 0) return;

        for (int i = 0; i < _roomBranchCount; i++)
        {
            // ランダムに部屋を選ぶ
            RectInt room = _rooms[Random.Range(0, _rooms.Count)];

            // 0:左 1:右 2:上 3:下
            int side = Random.Range(0, 4);

            Vector2Int start;
            Vector2Int direction;

            // 部屋の壁から通路を伸ばす
            switch (side)
            {
                case 0: // 左
                    start = new Vector2Int(
                        room.xMin - 1,
                        Random.Range(room.yMin + 1, room.yMax - 1)
                    );
                    direction = Vector2Int.left;
                    break;

                case 1: // 右
                    start = new Vector2Int(
                        room.xMax,
                        Random.Range(room.yMin + 1, room.yMax - 1)
                    );
                    direction = Vector2Int.right;
                    break;

                case 2: // 上
                    start = new Vector2Int(
                        Random.Range(room.xMin + 1, room.xMax - 1),
                        room.yMax
                    );
                    direction = Vector2Int.up;
                    break;

                default: // 下
                    start = new Vector2Int(
                        Random.Range(room.xMin + 1, room.xMax - 1),
                        room.yMin - 1
                    );
                    direction = Vector2Int.down;
                    break;
            }

            // 通路の長さを決める
            int length = Random.Range(
                _minRoomBranchLength,
                _maxRoomBranchLength + 1
            );

            bool canCreate = true;

            // 通路を作れる場所か確認
            for (int j = 0; j < length; j++)
            {
                Vector2Int pos = start + direction * j;

                // マップの範囲外か確認
                if (pos.x <= 0 || pos.x >= _mapWidth - 1 ||
                    pos.y <= 0 || pos.y >= _mapHeight - 1)
                {
                    canCreate = false;
                    break;
                }

                // 既に床がある場合は作らない
                if (_map[pos.x, pos.y] == 1)
                {
                    canCreate = false;
                    break;
                }
            }

            if (!canCreate) continue;

            // 通路を生成
            for (int j = 0; j < length; j++)
            {
                Vector2Int pos = start + direction * j;
                _map[pos.x, pos.y] = 1;
            }
        }
    }

    private Vector2Int GetRandomRoomEdge(RectInt room)//部屋の辺を取得し、ランダムに値を設定するメソッド
    {
        // 0:左 1:右 2:上 3:下
        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: // 左側
                return new Vector2Int(
                    room.xMin,
                    Random.Range(room.yMin + 1, room.yMax - 1)
                );

            case 1: // 右側
                return new Vector2Int(
                    room.xMax - 1,
                    Random.Range(room.yMin + 1, room.yMax - 1)
                );

            case 2: // 上側
                return new Vector2Int(
                    Random.Range(room.xMin + 1, room.xMax - 1),
                    room.yMax - 1
                );

            default: // 下側
                return new Vector2Int(
                    Random.Range(room.xMin + 1, room.xMax - 1),
                    room.yMin
                );
        }
    }

    // プレイヤーをランダムな部屋の中にスポーンさせるメソッド
    private void SpawnPlayerInRandomRoom() 
    {
        // 部屋が存在しない場合は処理を終了
        if (_rooms.Count == 0) return;

        RectInt room = _rooms[Random.Range(0, _rooms.Count)];

        int x = Random.Range(room.xMin + 1, room.xMax - 1);
        int y = Random.Range(room.yMin + 1, room.yMax - 1);
        _playerGridPosition = new Vector2Int(x, y); // プレイヤーの座標を保存

        Instantiate(_player, new Vector3(x + 0.5f, y + 0.5f, 0f), Quaternion.identity);
    }

    // 二次元配列（map）の内容に基づいてタイルを配置するメソッド
    private void InstantiateTile() 
    {
        // タイルマップをクリア (再生成するので必要)
        _wallTilemap.ClearAllTiles();
        _floorTilemap.ClearAllTiles();

        for (int x = 0; x < _mapWidth; x++)
        {
            for (int y = 0; y < _mapHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (_map[x, y] == 1)
                {
                    // 床タイルを配置
                    _floorTilemap.SetTile(pos, _floorTile);
                }
                else
                {
                    // 壁タイルを配置
                    _wallTilemap.SetTile(pos, _wallTile);
                }
            }
        }
    }

    private void SpawnEnemies()
    {
        // 部屋が存在しない場合は終了
        if (_rooms.Count == 0) return;

        // 敵のプレハブが登録されていない場合は終了
        if (_enemyPrefabs.Length == 0) return;

        // 敵を生成できる座標を保存するリスト
        List<Vector2Int> candidates = new List<Vector2Int>();

        // 全ての部屋から生成可能な座標を取得
        foreach (RectInt room in _rooms)
        {
            for (int x = room.xMin + 1; x < room.xMax - 1; x++)
            {
                for (int y = room.yMin + 1; y < room.yMax - 1; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    // プレイヤーのいるマスは除外
                    if (pos == _playerGridPosition) continue;

                    candidates.Add(pos);
                }
            }
        }

        // 敵を生成する
        for (int i = 0; i < _enemyCount; i++)
        {
            // 生成できる場所がなくなったら終了
            if (candidates.Count == 0) break;

            // ランダムな座標を取得
            int index = Random.Range(0, candidates.Count);
            Vector2Int pos = candidates[index];

            // 使用した座標をリストから削除
            candidates.RemoveAt(index);

            // ランダムな敵のプレハブを取得
            int enemyIndex = Random.Range(0, _enemyPrefabs.Length);
            GameObject enemy = _enemyPrefabs[enemyIndex];

            // 敵を生成
            Instantiate(
                enemy,
                new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f),
                Quaternion.identity
            );
        }
    }

    //コンソール表示用メソッド
    public void PrintMapToConsole()
    {
        for (int y = _mapHeight - 1; y >= 0; y--)
        {
            string line = "";
            for (int x = 0; x < _mapWidth; x++)
            {
                line += _map[x, y] == 1 ? "■" : "□";
            }
            Debug.Log(line);
        }
    }

   




    private void Start()
    {
        MapGenerate();
        PrintMapToConsole();
        SpawnPlayerInRandomRoom();
        SpawnEnemies();
    }

}