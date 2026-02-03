using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PayloadZoneIndicator : MonoBehaviour
{
    [Header("Visual")]
    public Color ringColor = new Color(0.2f, 0.9f, 1f, 0.6f);
    public float ringWidth = 0.12f;
    public float yOffset = 0.05f;
    public int segments = 64;
    public bool animatePulse = true;
    public float pulseSpeed = 1.5f;
    public float pulseScale = 0.15f;

    [Header("Auto Radius")]
    public bool useTriggerRadius = true;
    public float radiusOverride = 3f;

    private LineRenderer lineRenderer;
    private float baseRadius;
    private float pulseTime;

    void Awake()
    {
        EnsureLineRenderer();
        CacheBaseRadius();
        UpdateRing(baseRadius);
    }

    void Update()
    {
        if (!animatePulse) return;
        pulseTime += Time.deltaTime * pulseSpeed;
        float pulse = 1f + Mathf.Sin(pulseTime) * pulseScale;
        UpdateRing(baseRadius * pulse);
    }

    void EnsureLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = Mathf.Max(16, segments);
        lineRenderer.startWidth = ringWidth;
        lineRenderer.endWidth = ringWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = ringColor;
        lineRenderer.endColor = ringColor;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }

    void CacheBaseRadius()
    {
        if (!useTriggerRadius)
        {
            baseRadius = Mathf.Max(0.1f, radiusOverride);
            return;
        }

        Collider col = GetComponent<Collider>();
        if (col is SphereCollider sphere)
        {
            baseRadius = Mathf.Max(0.1f, sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z));
        }
        else if (col is CapsuleCollider capsule)
        {
            baseRadius = Mathf.Max(0.1f, capsule.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z));
        }
        else if (col is BoxCollider box)
        {
            Vector3 size = Vector3.Scale(box.size, transform.lossyScale);
            baseRadius = Mathf.Max(0.1f, Mathf.Max(size.x, size.z) * 0.5f);
        }
        else
        {
            baseRadius = Mathf.Max(0.1f, radiusOverride);
        }
    }

    void UpdateRing(float radius)
    {
        if (lineRenderer == null) return;

        int count = Mathf.Max(16, segments);
        if (lineRenderer.positionCount != count)
            lineRenderer.positionCount = count;

        Vector3 center = transform.position + Vector3.up * yOffset;
        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float rad = Mathf.Deg2Rad * (i * angleStep);
            Vector3 pos = new Vector3(Mathf.Cos(rad) * radius, 0f, Mathf.Sin(rad) * radius);
            lineRenderer.SetPosition(i, center + pos);
        }
    }
}
