using UnityEngine;

public enum SpeedType
{
    slow = -1,
    Nomal = 0,
    DoubleMove = 1,
    DoubleAttack = 2,
}
public enum EnemyType
{

    Slime,      // スライム系
    Floating,   // 浮遊系
    Dragon,     // ドラゴン系
    Undead,     // アンデッド系
    Aquatic,    // 水棲系
}



[CreateAssetMenu(fileName = "EnemyBaseData", menuName = "Scriptable Objects/EnemyBaseData")]
public class EnemyBaseData : ScriptableObject
{
    [Header("基礎情報")]
    public string enemyName;//名前
    public int maxHP;//最大HP
    public int attackPoint;//攻撃力
    public int defensePoint;//防御力
    public int range;//維持する距離
    public int visionRange = 2;//視界
    public int exp;//経験値
    public float dropChance;//ドロップ率
    public bool isSpecial = false;//特殊な能力があるか
    public SpeedType speedType = 0;
    public EnemyType[] enemyTypes;//敵の種族
    [Header("説明文")]
    [TextArea(2, 5)]
    public string text;//図鑑などに載る説明文
}
