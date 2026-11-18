using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject _player;//プレイヤー格納変数
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (_player == null) { _player = GameObject.FindGameObjectWithTag("Player"); }
        transform.position = new Vector3(_player.transform.position.x, _player.transform.position.y, -10);
    }
}
