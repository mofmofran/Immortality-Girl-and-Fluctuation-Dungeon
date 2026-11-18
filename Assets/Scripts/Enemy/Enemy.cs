using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("基礎ステータス")]
    public string name = "";
    public int maxHP;//最大HP
    public int hP;//現在HP

    public int attackPower;//攻撃力
    public int armor;//守備力

    public int visionRange = 2;//通路内の視界

    GameObject player;//Playerの格納変数

    // プレイヤーを発見関連
    private bool _hasDetectedPlayer = false;//プレイヤーを見つけたか
    [HideInInspector] public bool isInRoom = true;//部屋にいるか


    //メソッド
    
    private void OnEnable()// TurnManagerに自分自身を登録する
    {
        
        if (TurnManager.instance != null)
        {
            TurnManager.instance.RegisterEnemy(this);
        }
    }

    private void OnDisable()// TurnManagerから自分自身を登録解除する
    {
        
        if (TurnManager.instance != null)
        {
            TurnManager.instance.UnregisterEnemy(this);
        }
    }


    // 敵のターン
    public void TakeTurn()
    {
        Debug.Log("敵の行動");
        //if (player == null)//playerがいなかったら返す
        //{
        //    return;
        //}
        //// プレイヤーとの距離を確認する

        //// マス目の座標を取得
        //Vector2 enemyPos = transform.position;
        //Vector2 playerPos = player.transform.position;
        //float dx = Mathf.Abs(playerPos.x - enemyPos.x);
        //float dy = Mathf.Abs(playerPos.y - enemyPos.y);


        //if (isInRoom)//部屋にいる時
        //{
        //    bool pIsRroom = player.GetComponent<PlayerControler>().isInRoom;
        //    bool pIsInFrontOfTheRoom = player.GetComponent<PlayerControler>().isInFrontOfTheRoom;
        //    if (pIsRroom)//プレイヤーも部屋にいたら見つける
        //    {
        //        _hasDetectedPlayer = true;

        //    }
        //    else if (pIsInFrontOfTheRoom)//部屋にいるときにプレイヤーが部屋前通路に居たら見つける
        //    {
        //        _hasDetectedPlayer = true;
        //    }
        //}

        //else
        //{
        //    if (dx <= visionRange || dy <= visionRange)//通路で2マス以内なら見つける 
        //    {
        //        _hasDetectedPlayer = true;
        //    }

        //    if (_hasDetectedPlayer)
        //    {
        //        // 隣接していたら攻撃する
        //        if (dx <= 1 || dy <= 1)
        //        {
        //            AttackPlayer();
        //        }
        //        else // 離れていたら移動する
        //        {
        //            MoveToPlayer();
        //        }

        //    }


        //}

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
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        // Update is called once per frame
        void Update()
        {

        }
}
