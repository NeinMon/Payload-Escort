using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Photon")]
    public string roomName = "PayloadRoom";
    public byte maxPlayers = 10;
    public string gameplaySceneName = "LowPolyFPS_Map1";
    public bool autoConnect = true;

    [Header("UI")]
    public TMP_Dropdown teamDropdown;
    public TMP_Dropdown roleDropdown;
    public Button readyButton;
    public Button playButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI playersText;

    [Header("Hero Preview (Optional)")]
    public TextMeshProUGUI heroNameText;
    public TextMeshProUGUI heroInfoText;
    public TextMeshProUGUI heroSkillsText;

    [Header("Heroes")]
    public HeroRoster heroRoster;

    private bool localReady;

    private const string READY_KEY = "Ready";

    void Start()
    {
        EnsureEventSystem();
        PhotonNetwork.AutomaticallySyncScene = true;

        EnsureDropdownTemplate(teamDropdown);
        EnsureDropdownTemplate(roleDropdown);

        if (teamDropdown != null)
            teamDropdown.onValueChanged.AddListener(OnTeamChanged);

        if (roleDropdown != null)
            roleDropdown.onValueChanged.AddListener(OnRoleChanged);

        if (readyButton != null)
            readyButton.onClick.AddListener(ToggleReady);

        if (playButton != null)
            playButton.onClick.AddListener(StartGameIfReady);

        if (autoConnect)
            Connect();
    }

    void EnsureDropdownTemplate(TMP_Dropdown dropdown)
    {
        if (dropdown == null) return;
        if (dropdown.template != null) return;

        GameObject template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        template.transform.SetParent(dropdown.transform, false);
        RectTransform templateRect = template.GetComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0f, 0f);
        templateRect.anchorMax = new Vector2(1f, 0f);
        templateRect.pivot = new Vector2(0.5f, 1f);
        templateRect.sizeDelta = new Vector2(0f, 180f);
        templateRect.anchoredPosition = new Vector2(0f, -5f);
        template.SetActive(false);

        Image templateImage = template.GetComponent<Image>();
        templateImage.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewport.transform.SetParent(template.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0f, 0f);
        viewportRect.anchorMax = new Vector2(1f, 1f);
        viewportRect.offsetMin = new Vector2(4f, 4f);
        viewportRect.offsetMax = new Vector2(-4f, -4f);
        Mask mask = viewport.GetComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 28f);

        GameObject item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
        item.transform.SetParent(content.transform, false);
        RectTransform itemRect = item.GetComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0f, 0.5f);
        itemRect.anchorMax = new Vector2(1f, 0.5f);
        itemRect.pivot = new Vector2(0.5f, 0.5f);
        itemRect.sizeDelta = new Vector2(0f, 28f);

        GameObject itemBackground = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
        itemBackground.transform.SetParent(item.transform, false);
        RectTransform bgRect = itemBackground.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = itemBackground.GetComponent<Image>();
        bgImage.color = new Color(1f, 1f, 1f, 0.1f);

        GameObject itemCheckmark = new GameObject("Item Checkmark", typeof(RectTransform), typeof(Image));
        itemCheckmark.transform.SetParent(item.transform, false);
        RectTransform checkRect = itemCheckmark.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0f, 0.5f);
        checkRect.anchorMax = new Vector2(0f, 0.5f);
        checkRect.pivot = new Vector2(0f, 0.5f);
        checkRect.sizeDelta = new Vector2(16f, 16f);
        checkRect.anchoredPosition = new Vector2(10f, 0f);
        Image checkImage = itemCheckmark.GetComponent<Image>();
        checkImage.color = new Color(0.2f, 0.8f, 1f, 1f);

        GameObject labelGo = new GameObject("Item Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(item.transform, false);
        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(30f, 0f);
        labelRect.offsetMax = new Vector2(-10f, 0f);
        TextMeshProUGUI label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = "Option";
        label.fontSize = 22f;
        label.alignment = TextAlignmentOptions.Left;
        label.color = Color.white;

        Toggle toggle = item.GetComponent<Toggle>();
        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;

        ScrollRect scrollRect = template.GetComponent<ScrollRect>();
        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        dropdown.template = templateRect;
        dropdown.itemText = label;
        dropdown.itemImage = checkImage;
    }

    void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null) return;
        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(eventSystem);
    }

    void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            JoinLobby();
            return;
        }

        SetStatus("Connecting...");
        PhotonNetwork.ConnectUsingSettings();
    }

    void JoinLobby()
    {
        if (!PhotonNetwork.InLobby)
        {
            SetStatus("Joining Lobby...");
            PhotonNetwork.JoinLobby();
        }
    }

    void JoinRoom()
    {
        RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers };
        SetStatus("Joining Room...");
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnConnectedToMaster()
    {
        SetStatus("Connected. Joining Lobby...");
        JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        JoinRoom();
    }

    public override void OnJoinedRoom()
    {
        SetStatus("In Room. Choose team/role and Ready.");
        ApplyCurrentSelections();
        UpdateHeroPreview();
        UpdatePlayersList();
        UpdatePlayButton();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayersList();
        UpdatePlayButton();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayersList();
        UpdatePlayButton();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        UpdatePlayersList();
        UpdatePlayButton();
    }

    void OnTeamChanged(int index)
    {
        ApplyTeamSelection(index);
    }

    void OnRoleChanged(int index)
    {
        ApplyRoleSelection(index);
        UpdateHeroPreview();
    }

    void ApplyCurrentSelections()
    {
        if (teamDropdown != null)
            ApplyTeamSelection(teamDropdown.value);

        if (roleDropdown != null)
            ApplyRoleSelection(roleDropdown.value);
    }

    void ApplyTeamSelection(int index)
    {
        if (!PhotonNetwork.InRoom) return;

        if (index == 0)
        {
            Hashtable props = new Hashtable { { PayloadTeamConstants.TEAM_KEY, null } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            return;
        }

        PayloadTeam team = index == 1 ? PayloadTeam.Attackers : PayloadTeam.Defenders;
        PayloadTeamUtils.SetPlayerTeam(PhotonNetwork.LocalPlayer, team);
    }

    void ApplyRoleSelection(int index)
    {
        if (!PhotonNetwork.InRoom) return;

        PlayerRole role = PlayerRole.Engineer;
        switch (index)
        {
            case 1: role = PlayerRole.Engineer; break;
            case 2: role = PlayerRole.Heavy; break;
            case 3: role = PlayerRole.Medic; break;
            case 4: role = PlayerRole.Saboteur; break;
        }

        PlayerRoleUtils.SetPlayerRole(PhotonNetwork.LocalPlayer, role);
    }

    void UpdateHeroPreview()
    {
        if (heroRoster == null || roleDropdown == null) return;

        PlayerRole role = PlayerRole.Engineer;
        switch (roleDropdown.value)
        {
            case 1: role = PlayerRole.Engineer; break;
            case 2: role = PlayerRole.Heavy; break;
            case 3: role = PlayerRole.Medic; break;
            case 4: role = PlayerRole.Saboteur; break;
        }

        if (!heroRoster.TryGetLoadoutByRole(role, out HeroRoster.HeroLoadout loadout) || loadout == null)
            return;

        if (heroNameText != null && loadout.heroDefinition != null)
            heroNameText.text = loadout.heroDefinition.displayName;

        if (heroInfoText != null && loadout.heroDefinition != null)
        {
            heroInfoText.text = $"HP: {loadout.heroDefinition.maxHealth}\nSpeed: {loadout.heroDefinition.moveSpeed}";
        }

        if (heroSkillsText != null && loadout.skills != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < loadout.skills.Length; i++)
            {
                HeroSkillDefinition skill = loadout.skills[i];
                if (skill == null) continue;
                string keyHint = skill.slot == HeroSkillSlot.R ? "X" : skill.inputHint;
                sb.AppendLine($"{keyHint} - {skill.displayName}");
            }
            heroSkillsText.text = sb.ToString();
        }
    }


    void ToggleReady()
    {
        if (!PhotonNetwork.InRoom) return;

        localReady = !localReady;
        Hashtable props = new Hashtable { { READY_KEY, localReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        UpdateReadyButton();
        UpdatePlayButton();
    }

    void StartGameIfReady()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!AreAllPlayersReady()) return;

        PhotonNetwork.LoadLevel(gameplaySceneName);
    }

    void UpdateReadyButton()
    {
        if (readyButton == null) return;
        TextMeshProUGUI label = readyButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = localReady ? "Unready" : "Ready";
    }

    void UpdatePlayButton()
    {
        if (playButton == null) return;
        playButton.interactable = PhotonNetwork.IsMasterClient && AreAllPlayersReady();
    }

    bool AreAllPlayersReady()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties == null || !player.CustomProperties.ContainsKey(READY_KEY))
                return false;

            if (!(bool)player.CustomProperties[READY_KEY])
                return false;
        }
        return PhotonNetwork.PlayerList.Length > 0;
    }

    void UpdatePlayersList()
    {
        if (playersText == null) return;

        List<string> lines = new List<string>();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string team = "Auto";
            if (PayloadTeamUtils.TryGetPlayerTeam(player, out PayloadTeam t))
                team = t.ToString();

            string role = "None";
            if (PlayerRoleUtils.TryGetPlayerRole(player, out PlayerRole r))
                role = r.ToString();

            bool ready = player.CustomProperties != null && player.CustomProperties.ContainsKey(READY_KEY) && (bool)player.CustomProperties[READY_KEY];

            lines.Add($"{player.NickName} | {team} | {role} | {(ready ? "Ready" : "Not Ready")}");
        }

        playersText.text = string.Join("\n", lines);
    }

    void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
