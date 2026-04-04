using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SerialCube : MonoBehaviour
{
    public SerialHandler serialHandler;

    public Text text;
    public GameObject cube;

    // 動きの滑らかさを改善する
    // 0に近いほどぬるぬる、1で即時反映
    // 0.01f-1.0f
    public float smoothness = 0.1f;

    private Quaternion targetRotation;
    private Vector3 basePosition;
    private Rigidbody rb;

    void Start()
    {
        //信号を受信したときに、そのメッセージの処理を行う
        serialHandler.OnDataReceived += OnDataReceived;
        basePosition = cube.transform.position;
        targetRotation = transform.rotation;

        rb = cube.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Quaternion nextRotation = Quaternion.Slerp(rb.rotation, targetRotation, smoothness);

        rb.MoveRotation(nextRotation);

        rb.MovePosition(basePosition);
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

            targetRotation = Quaternion.Euler(pitch, 0, roll);

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
