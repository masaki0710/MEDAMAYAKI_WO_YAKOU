using UnityEngine;

public class EggCooking : MonoBehaviour
{
    private SkinnedMeshRenderer skinnedMesh;
    private Material eggWhite;

    // 焼けるスピード
    public float cookSpeed = 0.2f;

    // 焼き加減
    private float cookProgress = 0f;

    // 白身の色の設定
    public Color rawColor = new Color(1f, 1f, 1f, 0f);
    public Color cookedColor = new Color(1f, 1f, 1f, 1f);

    void Start()
    {
        skinnedMesh = GetComponent<SkinnedMeshRenderer>();

        eggWhite = skinnedMesh.materials[1];

        skinnedMesh.SetBlendShapeWeight(0, 0f);
        eggWhite.color = rawColor;
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("FryingPan"))
        {
            Cook();
        }
    }

    void Cook()
    {
        if (cookProgress < 1f)
        {
            cookProgress += Time.deltaTime * cookSpeed;

            Debug.Log($"焼き加減: {(cookProgress * 100f):F1}%");

            skinnedMesh.SetBlendShapeWeight(0, cookProgress * 100f);

            eggWhite.color = Color.Lerp(rawColor, cookedColor, cookProgress);
        }
    }
}
