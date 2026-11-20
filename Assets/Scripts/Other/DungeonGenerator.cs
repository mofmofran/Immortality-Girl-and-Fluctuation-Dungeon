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

    // 外部公開用プロパティ
    public IReadOnlyList<RectInt> Rooms => _rooms;//部屋リストの参照
    public int MapWidth => _mapWidth;//マップの横幅参照
    public int MapHeight => _mapHeight;//マップの縦幅参照


    //メソッド関連
    public int GetTileValue(int x, int y) //指定座標のタイル取得
    { 
        return _map[x, y]; 
    }

    // 指定座標が含まれる部屋を取得（部屋外なら false）
    public bool TryGetRoomAt(Vector2Int gridPos, out RectInt foundRoom)
    {
        foreach (RectInt room in _rooms)
        {
            if (gridPos.x >= room.xMin + 1 && gridPos.x <= room.xMax - 2 &&
                gridPos.y >= room.yMin + 1 && gridPos.y <= room.yMax - 2)
            {
                foundRoom = room;
                return true;
            }
        }

        foundRoom = default;
        return false;
    }

    //マップ全体の生成
    public void GenerateDungeon()
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

        CreateBranches();

        CreateRoomBranches();

        InstantiateTile();
    }

    //部屋の生成
    private void CreateRoom()
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

    //部屋が重なっていないかの判定メソッド
    private bool IsOverlapping(int roomX, int roomY, int roomWidth, int roomHeight)
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

    //部屋の中心から別の部屋への通路を作るメソッド
    private void ConnectRoomsInOrder()
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

    //L字型通路を作るメソッド
    private void CarveCorridorL(Vector2Int from, Vector2Int to)
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

    //通路の枝分かれメソッド
    private void CreateBranches()
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

    //部屋から行き止まりを作るメソッド
    private void CreateRoomBranches()
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

    //部屋の辺を取得し、ランダムに値を設定するメソッド
    private Vector2Int GetRandomRoomEdge(RectInt room)
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

    // 指定座標が床（1）かどうかを安全に判定する
    public bool IsFloor(int x, int y)
    {
        if (_map == null) return false;
        if (x < 0 || x >= _mapWidth || y < 0 || y >= _mapHeight) return false;
        return _map[x, y] == 1;
    }

    //壁を掘るメソッド
    public bool DigTile(Vector2Int pos)
    {
        if (_map == null) return false;

        // 外周の壁枠は掘れないようにする
        if (pos.x <= 0 || pos.x >= _mapWidth - 1 || pos.y <= 0 || pos.y >= _mapHeight - 1)
            return false;

        // すでに床（1）なら何もしない
        if (_map[pos.x, pos.y] == 1)
            return false;

        // 1. 1マス掘って床にする
        _map[pos.x, pos.y] = 1;
        Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);
        _wallTilemap.SetTile(tilePos, null);
        _floorTilemap.SetTile(tilePos, _floorTile);

        // 2. 既存の部屋と繋がって四角形になったか判定し、RectInt を拡張
        if (TryExpandExistingRoomFromDig(pos))
        {
            return true;
        }

        // 3.（任意）既存部屋と繋がっていない場合でも、一定の広さの四角い床ができたら新規部屋に昇格
        TryPromoteNewRectangleRoom(pos, minWidth: 4, minHeight: 4);

        return true;
    }

    /// <summary>
    /// 掘ったマス(digPos)を含む「すべて床(1)の四角形」を作り、
    /// 既存の部屋とぴったり接して合体できるなら _rooms を拡張する
    /// </summary>
    private bool TryExpandExistingRoomFromDig(Vector2Int digPos)
    {
        // digPos を含む「すべて床(1)の極大長方形」を列挙
        List<RectInt> floorRects = GetAllFloorRectanglesContaining(digPos);

        for (int i = 0; i < _rooms.Count; i++)
        {
            RectInt room = _rooms[i];

            foreach (RectInt rect in floorRects)
            {
                // すでに部屋の中ならスキップ
                if (room.xMin <= rect.xMin && rect.xMax <= room.xMax &&
                    room.yMin <= rect.yMin && rect.yMax <= room.yMax)
                {
                    continue;
                }

                // 既存部屋と「辺」でぴったり接しているか判定
                if (IsAdjacentAndCanMergeIntoRect(room, rect, out RectInt mergedRoom))
                {
                    _rooms[i] = mergedRoom;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 指定した座標(target)を必ず含む「全マス床(1)の長方形」をすべて求める
    /// </summary>
    private List<RectInt> GetAllFloorRectanglesContaining(Vector2Int target)
    {
        List<RectInt> results = new List<RectInt>();
        if (!IsFloor(target.x, target.y)) return results;

        // target を含む行（y）ごとに、左右にどこまで床が続くか調べる
        int minY = target.y;
        while (minY > 1 && IsFloor(target.x, minY - 1)) minY--;

        int maxY = target.y;
        while (maxY < _mapHeight - 2 && IsFloor(target.x, maxY + 1)) maxY++;

        // 上端・下端を動かしながら、その範囲で横方向にどこまで全行床が続くか求める
        for (int yStart = minY; yStart <= target.y; yStart++)
        {
            for (int yEnd = target.y; yEnd <= maxY; yEnd++)
            {
                int xMin = target.x;
                while (xMin > 1 && IsColumnAllFloor(xMin - 1, yStart, yEnd))
                {
                    xMin--;
                }

                int xMax = target.x;
                while (xMax < _mapWidth - 2 && IsColumnAllFloor(xMax + 1, yStart, yEnd))
                {
                    xMax++;
                }

                // RectInt の width / height は (max - min + 1)
                results.Add(new RectInt(xMin, yStart, xMax - xMin + 1, yEnd - yStart + 1));
            }
        }

        return results;
    }

    private bool IsColumnAllFloor(int x, int yStart, int yEnd)
    {
        for (int y = yStart; y <= yEnd; y++)
        {
            if (!IsFloor(x, y)) return false;
        }
        return true;
    }

    /// <summary>
    /// room と rect が上下または左右で接しており、
    /// 合体した外接長方形の内部がすべて床(1)なら true を返す
    /// </summary>
    private bool IsAdjacentAndCanMergeIntoRect(RectInt room, RectInt rect, out RectInt merged)
    {
        merged = default;

        // 左右で接しているケース（Y範囲が一致していること）
        bool touchesLeft = (rect.xMax == room.xMin) && (rect.yMin == room.yMin && rect.yMax == room.yMax);
        bool touchesRight = (room.xMax == rect.xMin) && (rect.yMin == room.yMin && rect.yMax == room.yMax);

        // 上下で接しているケース（X範囲が一致していること）
        bool touchesDown = (rect.yMax == room.yMin) && (rect.xMin == room.xMin && rect.xMax == room.xMax);
        bool touchesUp = (room.yMax == rect.yMin) && (rect.xMin == room.xMin && rect.xMax == room.xMax);

        if (!touchesLeft && !touchesRight && !touchesDown && !touchesUp)
            return false;

        int xMin = Mathf.Min(room.xMin, rect.xMin);
        int yMin = Mathf.Min(room.yMin, rect.yMin);
        int xMax = Mathf.Max(room.xMax, rect.xMax);
        int yMax = Mathf.Max(room.yMax, rect.yMax);

        // 合体した長方形のすべてが床(1)か確認（通路と繋がって欠けている箇所があればNG）
        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                if (!IsFloor(x, y))
                {
                    return false;
                }
            }
        }

        merged = new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
        return true;
    }

    /// <summary>
    /// 既存部屋と繋がっていない場合でも、指定サイズ以上の床四角形ができたら新部屋にする
    /// </summary>
    private void TryPromoteNewRectangleRoom(Vector2Int digPos, int minWidth, int minHeight)
    {
        List<RectInt> floorRects = GetAllFloorRectanglesContaining(digPos);

        foreach (RectInt rect in floorRects)
        {
            if (rect.width >= minWidth && rect.height >= minHeight)
            {
                // 既存の部屋と重なる場合は除外
                bool overlapsExisting = false;
                foreach (RectInt room in _rooms)
                {
                    if (room.Overlaps(rect))
                    {
                        overlapsExisting = true;
                        break;
                    }
                }

                if (!overlapsExisting)
                {
                    _rooms.Add(rect);
                    return;
                }
            }
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
}