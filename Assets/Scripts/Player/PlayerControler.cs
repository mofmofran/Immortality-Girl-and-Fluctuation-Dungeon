using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class PlayerControler : MonoBehaviour
{
    [Header("初期ステータス")]
    public static float maxHP = 15f;//HP最大値
    public static float hP = 15f;//現在HP
    public static int maxStrength = 8;//力の最大値
    public static int strength = 8;//現在の力
    public static float maxSatietyLevel = 100f;//満腹度最大値
    public static float satietyLevel = 100f;//現在満腹度

    [Header("移動関連")]
    public float moveSpeed = 0.3f;//次のマスまでの移動速度
    public float inputWait = 0.1f; // 入力猶予時間
    [Header("壁判定")]
    [SerializeField] private LayerMask _stopLayer;

    //コンポーネント類
    Rigidbody2D rb;//Rigidbody用変数

    //移動用
    private Vector3 _moveStartPos;//移動開始地点
    private Vector3 _moveEndPos;//移動終了地点
    bool _isMoving;//移動中フラグ
    private Vector2 _moveInput;//入力感知
    private bool _isWaiting;//入力待機時間

    //部屋にいるかどうか
    [HideInInspector]public bool isInRoom = true;
    [HideInInspector]public bool isInFrontOfTheRoom = false;



    //メソッド関連
    void OnMove(InputValue value)
    {
        if (TurnManager.instance == null) return;
        if (!TurnManager.instance.IsPlayerTurn) return; 
        StartCoroutine(MoveInput());
        if (_isMoving) return;//移動動作中は移動入力を停止
        _moveInput = value.Get<Vector2>();
        if (_isWaiting) return;//入力待機時間中は停止       
        if (_moveInput == Vector2.zero) return;// 入力がなければ何もしない
  
        //_moveEndPos = new Vector3(
        //    _moveStartPos.x + Mathf.Round(move.x),//Mathf.Roundは小さい入力値を1や0、-1にする
        //    _moveStartPos.y + Mathf.Round(move.y),//Mathf.Roundは小さい入力値を1や0、-1にする
        //    0);

        StartCoroutine(WaitInput());
    }

    IEnumerator WaitInput()//入力猶予用
    {
        _isWaiting = true;

        // 入力猶予
        yield return new WaitForSeconds(inputWait);

        Vector2 direction = new Vector2(
        Mathf.Abs(_moveInput.x) > 0.5f ? Mathf.Sign(_moveInput.x) : 0,
        Mathf.Abs(_moveInput.y) > 0.5f ? Mathf.Sign(_moveInput.y) : 0
        );// 斜め移動を含め、移動距離を1にする

        _isWaiting = false;

        if (direction == Vector2.zero)
        {
            yield break;
        }

        // 壁があるか確認
        if (!CanMove(direction))
        {
            yield break;
        }

        // 壁がなければ移動
        yield return StartCoroutine(MoveOneCell(direction));
    }

    IEnumerator MoveInput()
    {
        yield return new WaitForSeconds(1f);
    }

    IEnumerator MoveOneCell(Vector2 direction)
    {
        _isMoving = true;

        float elapsed = 0;


        _moveStartPos = transform.position;//移動開始地点に現在地を代入
        _moveEndPos = _moveStartPos + new Vector3(
            direction.x,
            direction.y,
            0);

        while (elapsed < moveSpeed)//elapsedがmoveSpeedを上回るまで繰り返す
        {
            float t = elapsed / moveSpeed;

            //移動開始位置から終了地点までをLerpで結ぶ
            transform.position = Vector3.Lerp(
                _moveStartPos,
                _moveEndPos,
                t
            );

            elapsed += Time.deltaTime;//elapsedに前フレームまでの差を＋

            yield return null;
        }

        // 最後に正確な座標へ移動
        transform.position = _moveEndPos;

        // 座標を .5 刻みに補正
        CorrectPosition();

        _isMoving = false;
        TurnManager.instance.EndPlayerTurn();
    }

    void CorrectPosition()//位置補正メソッド
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Round(pos.x - 0.5f) + 0.5f;
        pos.y = Mathf.Round(pos.y - 0.5f) + 0.5f;

        transform.position = pos;
    }

    bool CanMove(Vector2 direction)//移動先が空いているかどうか
    {
        // Raycastを飛ばす距離
        float distance = direction.magnitude;

        // 壁に向かってRaycastを飛ばす
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction.normalized,
            distance,
            _stopLayer
        );

        // SceneビューでRaycastを確認する
        Debug.DrawRay(
            transform.position,
            direction.normalized * distance,
            hit.collider != null ? Color.red : Color.green,
            0.3f
        );

        // 壁に当たった場合は移動できない
        if (hit.collider != null)
        {
            Debug.Log("壁にぶつかりました！");
            return false;
        }

        return true;
    }


    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {

    }
}
