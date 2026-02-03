using System.Collections;
using UnityEngine;

public class TurretOverdriveEffect : MonoBehaviour
{
    public Color overdriveColor = new Color(0.2f, 0.9f, 1f, 1f);
    public float defaultDuration = 12f;
    public float damageMultiplier = 1.5f;
    public float rangeMultiplier = 1.3f;
    public int extraTargets = 1;

    private Coroutine running;
    private MaterialPropertyBlock block;
    private Renderer[] renderers;

    void Awake()
    {
        block = new MaterialPropertyBlock();
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void Activate(float duration)
    {
        if (running != null)
            StopCoroutine(running);

        running = StartCoroutine(ApplyOverdrive(duration > 0f ? duration : defaultDuration));
    }

    private IEnumerator ApplyOverdrive(float duration)
    {
        TurretAutoAttack attack = GetComponent<TurretAutoAttack>();
        if (attack != null)
            attack.ApplyOverdrive(duration, damageMultiplier, rangeMultiplier, extraTargets);

        TurretVisualSwap swap = GetComponent<TurretVisualSwap>();
        if (swap != null)
            swap.SetOverdrive(true);

        ApplyColor(overdriveColor, true);
        yield return new WaitForSeconds(duration);
        ApplyColor(Color.white, false);

        if (swap != null)
            swap.SetOverdrive(false);

        running = null;
    }

    private void ApplyColor(Color color, bool emission)
    {
        if (renderers == null || block == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;
            r.GetPropertyBlock(block);
            block.SetColor("_Color", color);
            if (emission)
                block.SetColor("_EmissionColor", color);
            else
                block.SetColor("_EmissionColor", Color.black);
            r.SetPropertyBlock(block);
        }
    }
}
