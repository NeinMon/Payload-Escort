using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonLobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Spawn Settings")]
    public string playerPrefabName = "Player";
    public Vector3 spawnPosition = new Vector3(0, 1, 0);
    public Quaternion spawnRotation = Quaternion.identity;
    public bool autoSpawnOnJoin = true;

    [Header("Room Settings")]
    public string roomName = "TestRoom";
    public byte maxPlayers = 10;
    public bool autoConnect = true;
    public bool autoJoinRoom = true;

    void Start()
    {
        if (!autoConnect) return;

        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
            return;
        }

        if (!PhotonNetwork.InLobby && !PhotonNetwork.InRoom)
        {
            Debug.Log("Already connected. Joining Lobby...");
            PhotonNetwork.JoinLobby();
            return;
        }

        if (PhotonNetwork.InLobby && autoJoinRoom)
        {
            JoinOrCreateRoom();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby. Joining or Creating Room...");
        if (autoJoinRoom)
        {
            JoinOrCreateRoom();
        }
    }

    void JoinOrCreateRoom()
    {
        RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers };
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room successfully. Spawning Player.");
        if (!autoSpawnOnJoin) return;

        if (FindAnyObjectByType<PayloadEscortMatchManager>() != null || FindAnyObjectByType<MultiplayerFPSGameManager>() != null)
        {
            return;
        }

        PhotonNetwork.Instantiate(playerPrefabName, spawnPosition, spawnRotation);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected from Photon: {cause}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Join room failed: {returnCode} - {message}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Create room failed: {returnCode} - {message}");
    }
}