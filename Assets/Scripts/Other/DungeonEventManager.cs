using UnityEngine;

public class DungeonEventManager : MonoBehaviour
{
    
    private bool _isFirstWindTriggered = false;
    private bool _isSecondWindTriggered = false;
    private bool _isThirdWindTriggered = false;

    //メソッド関係
    
    //ターン経過の風処理
    public void OnTurnPassed(int turn)
    {
        if (turn >= 800 && !_isFirstWindTriggered)
        {
            _isFirstWindTriggered = true;
            StrongWind();
        }
        if (turn >= 900 && !_isSecondWindTriggered)
        {
            _isSecondWindTriggered = true;
            StrongWind();
        }
        if (turn >= 1000 && !_isThirdWindTriggered)
        {
            _isThirdWindTriggered = true;
            StrongWind();
        }
    }

    //実際に風が吹く処理
    private void StrongWind()
    {
        if(_isFirstWindTriggered)
        Debug.Log("強風が吹き始めた！");

        if(_isSecondWindTriggered)
            Debug.Log("強風が更に強まった！");

        if(_isThirdWindTriggered)
            Debug.Log("強風に吹き飛ばされてしまった！");
        // 冒険失敗の処理
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
