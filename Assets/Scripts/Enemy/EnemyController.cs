using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("基礎ステータス")]
    public string name = "";
    public int maxHP;//最大HP
    public int hP;//現在HP

    public int attackPower;//攻撃力
    public int armor;//守備力

    public int visionRange = 2;//通路内の視界

    GameObject player;//Playerの格納変数

    private DungeonGenerator _dungeonGenerator;
    private PlayerSpawner _playerSpawner;



    // プレイヤーを発見関連
    private bool _hasFindPlayer = false;//プレイヤーを見つけたか
    [HideInInspector] public bool isInRoom = true;//部屋にいるか


    //メソッド
    //ダンジョンジェネレーターとプレイヤースポナーを登録
    public void Initialize(DungeonGenerator dungeonGenerator, PlayerSpawner playerSpawner)
    {
        _dungeonGenerator = dungeonGenerator;
        _playerSpawner = playerSpawner;
    }

    // ワールド座標からグリッド座標へ変換
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x),
            Mathf.FloorToInt(worldPos.y)
        );
    }

    // TurnManagerに自分自身を登録する
    private void OnEnable()
    {
        
        if (TurnManager.instance != null)
        {
            TurnManager.instance.RegisterEnemy(this);
        }
    }

    // TurnManagerから自分自身を登録解除する
    private void OnDisable()
    {
        
        if (TurnManager.instance != null)
        {
            TurnManager.instance.UnregisterEnemy(this);
        }
    }

    // プレイヤーと敵が同じ部屋にいるか判定
    public bool IsPlayerInSameRoom()
    {
        if (_dungeonGenerator == null || _playerSpawner == null) return false;

        Vector2Int enemyGrid = WorldToGrid(transform.position);
        Vector2Int playerGrid = _playerSpawner.PlayerGridPosition;

        if (_dungeonGenerator.TryGetRoomAt(enemyGrid, out RectInt enemyRoom) &&
            _dungeonGenerator.TryGetRoomAt(playerGrid, out RectInt playerRoom))
        {
            return enemyRoom == playerRoom;
        }

        return false;
    }

    // 敵のターン
    public void TakeTurn()
    {
        Debug.Log("敵の行動");


        //// プレイヤーを攻撃
        //void AttackPlayer()
        //{
        //    Debug.Log("プレイヤーを攻撃！");
        //}

        //// プレイヤーに近づく
        //void MoveToPlayer()
        //{
        //    Debug.Log("プレイヤーに近づく！");
        //}
    }
       
        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        // Update is called once per frame
        void Update()
        {

        }
}
