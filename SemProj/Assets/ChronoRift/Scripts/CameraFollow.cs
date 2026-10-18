using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private static CameraFollow instance;
    public static CameraFollow Instance
    {
        get
        {
            if (instance == null) instance = FindAnyObjectByType<CameraFollow>();
            return instance;
        }
        private set => instance = value;
    }

    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.2f, -10f);
    [SerializeField] private float smoothTime = 0.2f;

    [Header("Level Bounds")]
    [SerializeField] private float minX = -4f;
    [SerializeField] private float maxX = 70f;
    [SerializeField] private float minY = -2f;
    [SerializeField] private float maxY = 15f;

    private Vector3 currentVelocity = Vector3.zero;
    private float shakeTimer = 0f;
    private float shakeIntensity = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            if (PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }
            else return;
        }

        Vector3 targetPos = target.position + offset;
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        targetPos.z = offset.z;

        Vector3 smoothPos = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);

        if (shakeTimer > 0f)
        {
            Vector2 shakeOffset = Random.insideUnitCircle * shakeIntensity;
            smoothPos += new Vector3(shakeOffset.x, shakeOffset.y, 0f);
            shakeTimer -= Time.deltaTime;
        }

        transform.position = smoothPos;
    }

    public void TriggerShake(float duration, float intensity)
    {
        shakeTimer = duration;
        shakeIntensity = intensity;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
