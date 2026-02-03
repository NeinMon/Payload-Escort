using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PayloadEscortHUD : MonoBehaviour
{
    public PayloadEscortMatchManager matchManager;
    public PayloadController payloadController;

    [Header("UI")]
    public Slider progressBar;
    public TextMeshProUGUI stateText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI attackersScoreText;
    public TextMeshProUGUI defendersScoreText;
    public TextMeshProUGUI roleText;
    public TextMeshProUGUI payloadStatusText;
    public Slider sabotageProgressBar;
    public Slider repairProgressBar;

    [Header("State Colors")]
    public Color movingColor = new Color(0.2f, 1f, 0.4f, 1f);
    public Color contestedColor = new Color(1f, 0.75f, 0.2f, 1f);
    public Color stoppedColor = new Color(1f, 0.3f, 0.3f, 1f);
    public Color disabledColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    void Update()
    {
        if (matchManager != null && timerText != null)
        {
            int minutes = Mathf.FloorToInt(matchManager.CurrentTimer / 60f);
            int seconds = Mathf.FloorToInt(matchManager.CurrentTimer % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        if (matchManager != null)
        {
            if (attackersScoreText != null)
                attackersScoreText.text = $"ATK: {matchManager.AttackersScore}";
            if (defendersScoreText != null)
                defendersScoreText.text = $"DEF: {matchManager.DefendersScore}";
        }

        if (payloadController != null)
        {
            if (stateText != null)
            {
                stateText.text = payloadController.CurrentState.ToString();
                stateText.color = GetStateColor(payloadController.CurrentState);
            }

            if (progressBar != null)
                progressBar.value = payloadController.Progress;

            if (progressText != null)
            {
                int percent = Mathf.RoundToInt(payloadController.Progress * 100f);
                progressText.text = $"{percent}%";
            }

            UpdatePayloadStatusUI();
        }

        UpdateRoleUI();
    }

    void UpdateRoleUI()
    {
        if (roleText == null || !PhotonNetwork.InRoom) return;
        if (PlayerRoleUtils.TryGetPlayerRole(PhotonNetwork.LocalPlayer, out PlayerRole role))
            roleText.text = $"Role: {role}";
    }

    void UpdatePayloadStatusUI()
    {
        PayloadTeam localTeam = PayloadTeam.Attackers;
        bool hasTeam = PhotonNetwork.InRoom && PayloadTeamUtils.TryGetPlayerTeam(PhotonNetwork.LocalPlayer, out localTeam);

        if (payloadStatusText != null)
        {
            if (payloadController.IsDisabled && hasTeam && localTeam == PayloadTeam.Attackers && payloadController.LocalPlayerInZone)
                payloadStatusText.text = "Payload Disabled - Hold F to Repair";
            else if (payloadController.SabotageProgress > 0f && hasTeam && localTeam == PayloadTeam.Defenders)
                payloadStatusText.text = "Sabotaging Payload...";
            else
                payloadStatusText.text = string.Empty;
        }

        if (sabotageProgressBar != null)
        {
            sabotageProgressBar.value = payloadController.SabotageProgress;
            bool showSabotage = hasTeam && localTeam == PayloadTeam.Defenders && payloadController.SabotageProgress > 0f && !payloadController.IsDisabled;
            sabotageProgressBar.gameObject.SetActive(showSabotage);
        }

        if (repairProgressBar != null)
        {
            repairProgressBar.value = payloadController.RepairProgress;
            bool showRepair = hasTeam && localTeam == PayloadTeam.Attackers && payloadController.IsDisabled && payloadController.RepairProgress > 0f;
            repairProgressBar.gameObject.SetActive(showRepair);
        }
    }

    Color GetStateColor(PayloadState state)
    {
        switch (state)
        {
            case PayloadState.Moving:
                return movingColor;
            case PayloadState.Contested:
                return contestedColor;
            case PayloadState.Disabled:
                return disabledColor;
            default:
                return stoppedColor;
        }
    }
}
