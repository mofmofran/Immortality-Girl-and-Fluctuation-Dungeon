using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TurnManager;

public class TurnManager : MonoBehaviour
{
    // シングルトンの実装
    public static TurnManager instance { get; private set; }

    //ターンの変数
    public enum turnState
    {
        PlayerTurn,
        EnemyTurn,
        OtherTurn,
    }

    private turnState _currentTurn = turnState.PlayerTurn;//現在のターンと初期値設定 
    private readonly List<Enemy> enemies = new List<Enemy>();//エネミーのリスト

    //プロパティ
    public turnState CurrentTurn//他から現在のターン状態を確認するプロパティ
    {
        get { return _currentTurn; }
    }

    public bool IsPlayerTurn//プレイヤーターンかどうかを参照するプロパティ
    {
        get { return _currentTurn == turnState.PlayerTurn; }//プレイヤーターンならTrueを返す
    }
    //メソッド
    public void EnemySpawn()
    {
        Debug.Log("敵呼び出し");
    }

    // 敵を登録するメソッド
    public void RegisterEnemy(Enemy enemy)
    {
        // もしenemyがnullでなく、かつenemiesリストに含まれていない場合は追加する
        if (enemy != null && !enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
    }

    // 敵を登録解除するメソッド
    public void UnregisterEnemy(Enemy enemy)
    {
        // もしenemyがnullの場合は何もしない
        if (enemy == null) return;
        // enemiesリストからenemyを削除する
        enemies.Remove(enemy);
    }

    // プレイヤーのターンを終了し、敵のターンを開始するメソッド
    public void EndPlayerTurn()
    {
        if (_currentTurn != turnState.PlayerTurn) return;
        StartEnemyTurn();
    }

    // 敵のターンを開始するメソッド
    public void StartEnemyTurn()
    {
        _currentTurn = turnState.EnemyTurn;
        StartCoroutine(EnemyTurnCoroutine());
    }
    // 敵のターンを処理するコルーチン
    private IEnumerator EnemyTurnCoroutine()
    {
        // 破棄等でnullになった敵をリストから削除
        enemies.RemoveAll(e => e == null);

        // enemiesリストのスナップショットを作成してからループを回す
        // これにより、敵が行動中に破棄等の処理でリストが変更されても安全に処理できる
        List<Enemy> enemySnapshot = new List<Enemy>(enemies);

        foreach (Enemy enemy in enemySnapshot)
        {
            if (enemy == null) continue;
            enemy.TakeTurn();
            yield return null; // 敵ごとに1フレーム待機
        }

        _currentTurn = turnState.PlayerTurn;
    }

    private void Awake()
    {
        // if文でInstanceがnullでない、かつInstanceが自分自身でない場合はインスタンスを破棄する
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // そうでなければInstanceに自分自身を代入する
        instance = this;
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
