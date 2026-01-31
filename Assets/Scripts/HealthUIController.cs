using UnityEngine;
using UnityEngine.UIElements;
using Photon.Pun;

public class HealthUIController : MonoBehaviour
{
    private VisualElement barFill;
    private Label healthText;
    private PlayerHealth playerHealth;
    private UIDocument uiDocument;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        barFill = root.Q<VisualElement>("bar-fill");
        healthText = root.Q<Label>("health-text");

        // Tìm player cục bộ (isMine)
        foreach (var health in Object.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None))
        {
            if (health.photonView != null && health.photonView.IsMine)
            {
                playerHealth = health;
                break;
            }
        }
    }

    void Update()
    {
        if (playerHealth == null)
        {
            // Thử tìm lại PlayerHealth của người chơi cục bộ nếu chưa gán (spawn muộn)
            foreach (var health in Object.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None))
            {
                if (health.photonView != null && health.photonView.IsMine)
                {
                    playerHealth = health;
                    break;
                }
            }
            if (playerHealth == null) return;
        }

        float ratio = playerHealth.GetHealthRatio();
        barFill.style.width = new Length(ratio * 100f, LengthUnit.Percent);
        healthText.text = $"{Mathf.CeilToInt(playerHealth.CurrentHealth)} / {Mathf.CeilToInt(playerHealth.MaxHealth)}";
    }
}
