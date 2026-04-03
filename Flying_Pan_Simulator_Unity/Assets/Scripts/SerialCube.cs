using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SerialCube : MonoBehaviour
{
    // 通信設定
    public SerialHandler serialHandler;

    // 表示設定
    public Text text;
    public GameObject cube;

    // 動きの滑らかさを改善する
    // 0に近いほどぬるぬる、1で即時反映
    // 0.01f-1.0f
    public float smoothness = 0.1f;

    // あおり判定のしきい値
    //public float tossThreshold = 50f;
    //public float tossStrength = 2.0f;

    //public float forwardAmount = 0.5f;
    //public float upwardAmount = 0.8f;

    private Quaternion targetRotation;
    private Vector3 basePosition;
    //private float verticalOffset = 0f;

    //private float offsetZ = 0f;
    //private float offsetY = 0f;

    // Use this for initialization
    void Start()
    {
        //信号を受信したときに、そのメッセージの処理を行う
        serialHandler.OnDataReceived += OnDataReceived;
        basePosition = cube.transform.position;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        cube.transform.rotation = Quaternion.Slerp(cube.transform.rotation, targetRotation, smoothness);

        //offsetZ = Mathf.Lerp(offsetZ, 0, Time.deltaTime * 0.5f);
        //offsetY = Mathf.Lerp(offsetY, 0, Time.deltaTime * 0.5f);

        //cube.transform.position = basePosition + new Vector3(0, offsetY, offsetZ);

        cube.transform.position = basePosition;
    }

    // シリアルデータを受信したときの処理
    void OnDataReceived(string message)
    {
        //Debug.Log("届いたデータ: " + message);
        try
        {
            string[] angles = message.Split(',');

            // データが足りない場合は無視する
            if (angles.Length < 4)
            {
                Debug.LogWarning("データが足りません: " + message);
                return;
            }

            // 前後の傾き（X軸）
            float pitch = float.Parse(angles[0]);
            // 左右の傾き（Z軸）
            float roll = float.Parse(angles[1]);
            //// 水平の回転（Y軸）
            //float yaw = float.Parse(angles[2]);
            //// 強さ
            //float power = float.Parse(angles[3]);

            targetRotation = Quaternion.Euler(pitch, 0, roll);

            //if (power > tossThreshold)
            //{
            //    offsetZ = forwardAmount * (power / 20f);
            //    offsetY = upwardAmount * (power / 20f);

            //    Debug.Log("あおった！強度: " + power);
            //}

            if (text != null)
            {
                text.text = $"Pitch(X): {pitch:F1}\nRoll(Y): {roll:F1}";
            }
            
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Data Parse Error: " + e.Message);
        }
    }
}
