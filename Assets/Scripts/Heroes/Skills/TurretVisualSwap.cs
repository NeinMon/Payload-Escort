using UnityEngine;

public class TurretVisualSwap : MonoBehaviour
{
    [SerializeField] private GameObject normalModel;
    [SerializeField] private GameObject overdriveModel;

    public void SetOverdrive(bool active)
    {
        if (normalModel != null)
            normalModel.SetActive(!active);
        if (overdriveModel != null)
            overdriveModel.SetActive(active);
    }
}
