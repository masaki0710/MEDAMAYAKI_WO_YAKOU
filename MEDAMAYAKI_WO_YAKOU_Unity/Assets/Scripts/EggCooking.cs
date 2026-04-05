using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EggCooking : MonoBehaviour
{
    public SerialHandler serialHandler;

    private SkinnedMeshRenderer skinnedMesh;
    private Material eggWhite;
    private AudioSource audioSource;

    // 結果発表のAudio
    public AudioClip fanfareSound;
    public AudioClip normalSound;
    public AudioClip badSound;

    // 焼き加減の基本進行速度
    public float cookSpeed = 0.2f;

    public float backCookProgress = 0f;
    public float frontCookProgress = 0f;
    public float totalCookProgress = 0f;

    // シリアル通信で最後に送信した焼き加減の整数値
    private int lastSentProgress = -1;

    // ボタンの状態を追跡するための変数
    private int lastButtonState = 0;

    // 結果発表が完了したかどうかを追跡するフラグ
    private bool isResultFinished = false;

    // 白身の色の設定
    public Color rawWhiteColor = new Color(1f, 1f, 1f, 0f);
    public Color cookedWhiteColor = new Color(1f, 1f, 1f, 1f);

    // 黄身の色の設定
    public Color rawYolkColor = new Color(1f, 0.9f, 0.5f, 0f);
    public Color cookedYolkColor = new Color(1f, 0.9f, 0.5f, 1f);

    // 結果発表
    public GameObject fork;
    public Canvas resultCanvas;
    public Image resultImage;

    public Sprite spritePerfect;
    public Sprite spriteGood;
    public Sprite spriteBad;
    public Sprite spriteRaw;

    public Transform forkSpawnPoint;
    public Transform forkAwayPoint;

    private bool isEndingSequence = false;
    private enum CookResult { Perfect, Good, Bad, Raw }

    void Start()
    {
        skinnedMesh = GetComponent<SkinnedMeshRenderer>();
        eggWhite = skinnedMesh.materials[1];
        skinnedMesh.SetBlendShapeWeight(0, 0f);
        eggWhite.color = rawWhiteColor;

        if (serialHandler == null)
            serialHandler = GameObject.FindAnyObjectByType<SerialHandler>();

        if (serialHandler != null)
            serialHandler.OnDataReceived += HandleButtonInput;

        audioSource = GetComponent<AudioSource>();
    }

    void HandleButtonInput(string message)
    {
        if (isResultFinished) return;

        try
        {
            string[] receiveData = message.Split(',');

            if (receiveData.Length >= 5)
            {
                int currentButtonState = int.Parse(receiveData[4]);

                if (currentButtonState == 1 && lastButtonState == 0 && totalCookProgress > 0.05f)
                    ShowResult();
                lastButtonState = currentButtonState;
            }
        }
        catch (System.Exception)
        {
            Debug.LogWarning("不正なデータ: " + message);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("FryingPan") && !isResultFinished)
            Cook();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("FryingPan"))
            if (!audioSource.isPlaying) audioSource.Play();
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("FryingPan"))
            if (audioSource.isPlaying) audioSource.Stop();
    }

    void Cook()
    {
        float dot = Vector3.Dot(transform.up, Vector3.up);

        if (dot > 0.5f)
        {
            backCookProgress += Time.deltaTime * cookSpeed;
            float curvedValue = Mathf.Pow(backCookProgress, 2.0f);
            Debug.Log($"裏面の焼き加減: {(backCookProgress * 100f):F1}%");
        }
        else if (dot < -0.5f)
        {
            frontCookProgress += Time.deltaTime * cookSpeed;
            float curvedValue = Mathf.Pow(frontCookProgress, 2.0f);
            Debug.Log($"表面の焼き加減: {(frontCookProgress * 100f):F1}%");
        }

        totalCookProgress = (backCookProgress + frontCookProgress) / 2f;
        UpdateVisuals(totalCookProgress);
    }

    void UpdateVisuals(float progress)
    {
        float curvedValue = Mathf.Clamp01(progress);
        skinnedMesh.SetBlendShapeWeight(0, curvedValue * 100f);
        eggWhite.color = Color.Lerp(rawWhiteColor, cookedWhiteColor, curvedValue);

        if (progress > 1.0f)
            eggWhite.color = Color.Lerp(cookedWhiteColor, Color.black, (progress - 1.0f) * 2f);

        int currentProgressInt = Mathf.RoundToInt(Mathf.Clamp01(progress) * 100f);

        if (currentProgressInt != lastSentProgress)
        {
            serialHandler.Write(currentProgressInt.ToString() + '\n');
            lastSentProgress = currentProgressInt;
        }

        audioSource.volume = Mathf.Lerp(0.2f, 1f, curvedValue);
    }

    void ShowResult()
    {
        if (isEndingSequence) return;
        isEndingSequence = true;

        isResultFinished = true;
        audioSource.Stop();

        StartCoroutine(ForkEndingSequence());
    }

    IEnumerator ForkEndingSequence()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        fork.transform.position = forkSpawnPoint.position;
        fork.SetActive(true);

        float elapsed = 0f;
        Vector3 startPos = forkSpawnPoint.position;
        while(elapsed < 1.0f)
        {
            elapsed += Time.deltaTime;
            fork.transform.position = Vector3.Lerp(startPos, transform.position, Mathf.SmoothStep(0f, 1f, elapsed));
            yield return null;
        }

        transform.SetParent(fork.transform);
        transform.localPosition = new Vector3(0, 0, 0.1f);

        yield return new WaitForSeconds(0.5f);

        EvaluateAndSetResultImage();

        elapsed = 0f;
        startPos = fork.transform.position;
        while (elapsed < 1.0f)
        {
            elapsed += Time.deltaTime;
            fork.transform.position = Vector3.Lerp(startPos, forkAwayPoint.position, Mathf.SmoothStep(0f, 1f, elapsed));
            yield return null;
        }

        resultCanvas.gameObject.SetActive(true);

        yield return new WaitForSeconds(6.0f);

        Debug.Log("ゲーム終了");
        SceneManager.LoadScene("MenuScene");
    }

    void EvaluateAndSetResultImage()
    {
        // 片面ずつの状態を定義
        bool isBackOk = (backCookProgress >= 0.7f && backCookProgress <= 1.0f);
        bool isFrontOk = (frontCookProgress >= 0.7f && frontCookProgress <= 1.0f);

        // 両面OKならパーフェクト
        if (isBackOk && isFrontOk)
        {
            resultImage.sprite = spritePerfect;
        }
        // どちらかが焦げすぎ（1.1以上）
        else if (backCookProgress > 1.1f || frontCookProgress > 1.1f)
        {
            resultImage.sprite = spriteBad; // 焦げ画像
        }
        // どちらかが生（0.4未満）
        else if (backCookProgress < 0.4f || frontCookProgress < 0.4f)
        {
            resultImage.sprite = spriteRaw; // 生画像
        }
        // 焦げてないし生でもないけど、両面完璧ではない場合
        else
        {
            resultImage.sprite = spriteGood; // 普通画像
        }

        resultImage.SetNativeSize();
    }
}
