using UnityEngine;

public enum EnemyType
{
    Slime,
}

[CreateAssetMenu(fileName = "EnemyBaseData", menuName = "Scriptable Objects/EnemyBaseData")]
public class EnemyBaseData : ScriptableObject
{
    [Header("基礎情報")]
    public string name;//名前
    public float maxHP;//最大HP
    public int attackPoint;//攻撃力
    public int defensePoint;//防御力
    public int range;//維持する距離
    public int visionRange = 2;//視界
    public int exp;//経験値
    public float dropChance;//ドロップ率
    public bool isSpecial = false;//特殊な能力があるか
    public EnemyType enemyType;//敵の種族
    [Header("説明文")]
    [TextArea(2, 5)]
    public string text;//図鑑などに載る説明文
}
