using UnityEditor.Overlays;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("基礎ステータス")]
    public string enemyName = "";
    public int maxHP;//最大HP
    public int hP;//現在HP

    public int attackPower;//攻撃力
    public int armor;//守備力
    public int range;//維持する距離
    public int visionRange = 2;//通路内の視界
    public int exp;//経験値
    public float dropChance;//ドロップ率
    private bool _isSpecial;//特殊能力があるか
    public SpeedType speedType;//速度
    GameObject player;//Playerの格納変数

    [SerializeField]
    private EnemyBaseData _baseData;
    [SerializeField]
    private EnemySpecialData _specialData;
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

    //敵のデータをSOから取得
    public void SetBaseData(EnemyBaseData data)
    {
        _baseData = data;

        enemyName = _baseData.enemyName;
        maxHP = _baseData.maxHP;
        hP = _baseData.maxHP;

        attackPower = _baseData.attackPoint;
        armor = _baseData.defensePoint;

        range = _baseData.range;
        visionRange = _baseData.visionRange;

        exp = _baseData.exp;
        dropChance = _baseData.dropChance;

        _isSpecial = _baseData.isSpecial;
        speedType = _baseData.speedType;
    }

    //敵の種族取得メソッド
    public bool HasType(EnemyType type)
    {
        if (_baseData == null || _baseData.enemyTypes == null)
            return false;

        foreach (EnemyType enemyType in _baseData.enemyTypes)
        {
            if (enemyType == type)
                return true;
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

    private void Awake()
    {
        
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
