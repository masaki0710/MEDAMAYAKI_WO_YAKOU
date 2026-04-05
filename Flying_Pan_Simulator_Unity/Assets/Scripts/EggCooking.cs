using UnityEngine;

public class EggCooking : MonoBehaviour
{
    public SerialHandler serialHandler;

    private SkinnedMeshRenderer skinnedMesh;
    private Material eggWhite;
    private AudioSource audioSource;

    // 焼けるスピード
    public float cookSpeed = 0.2f;

    // 焼き加減
    private float cookProgress = 0f;

    private int lastSentProgress = -1;

    // 白身の色の設定
    public Color rawColor = new Color(1f, 1f, 1f, 0f);
    public Color cookedColor = new Color(1f, 1f, 1f, 1f);

    void Start()
    {
        skinnedMesh = GetComponent<SkinnedMeshRenderer>();

        eggWhite = skinnedMesh.materials[1];

        skinnedMesh.SetBlendShapeWeight(0, 0f);
        eggWhite.color = rawColor;

        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("FryingPan"))
        {
            Cook();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("FryingPan"))
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("FryingPan"))
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    void Cook()
    {
        if (cookProgress < 1f)
        {
            cookProgress += Time.deltaTime * cookSpeed;
            float curvedValue = Mathf.Pow(cookProgress, 2.0f);

            Debug.Log($"焼き加減: {(cookProgress * 100f):F1}%");

            skinnedMesh.SetBlendShapeWeight(0, curvedValue * 100f);
            eggWhite.color = Color.Lerp(rawColor, cookedColor, curvedValue);

            int currentProgressInt = Mathf.RoundToInt(curvedValue * 100f);
            if (currentProgressInt != lastSentProgress)
            {
                serialHandler.Write(currentProgressInt.ToString() + '\n');
                lastSentProgress = currentProgressInt;
            }

            audioSource.volume = Mathf.Lerp(0.2f, 1f, curvedValue);
        }
    }
}
