using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpecialData", menuName = "Scriptable Objects/EnemySpecialData")]
public class EnemySpecialData : ScriptableObject
{
    public float sPChance;//特殊行動の確率
    public bool isTransparent = false;//透明かどうか
    public bool isSleep = false;//寝ているかどうか
}
