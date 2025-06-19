using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System;
using Fusion.Sockets;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner runner;
    public NetworkPrefabRef playerPrefab;
    public Transform redSpawn;
    public Transform blueSpawn;

    public float gameCountdown = 30f;
    private float countdownTimer = 0f;
    public TMPro.TMP_Text gameCountdownText; // Kéo TMP_Text hiển thị đếm ngược vào đây
    private bool isCountingDown = false;

    void Awake()
    {
        if (runner == null)
            runner = FindFirstObjectByType<NetworkRunner>();
        if (runner != null)
        {
            runner.AddCallbacks(this);
            Debug.Log($"PlayerSpawner Awake, runner: {runner}, runner.Mode: {runner.Mode}, runner.IsRunning: {runner.IsRunning}");
        }
        else
        {
            Debug.LogError("PlayerSpawner: Không tìm thấy NetworkRunner trong scene!");
        }
    }

    void Update()
    {
        if (isCountingDown)
        {
            countdownTimer -= Time.deltaTime;
            if (gameCountdownText != null)
                gameCountdownText.text = $"Game bắt đầu sau: {Mathf.CeilToInt(countdownTimer)}s";
            if (countdownTimer <= 0)
            {
                isCountingDown = false;
                if (gameCountdownText != null)
                    gameCountdownText.text = "Bắt đầu!";
                // TODO: Gọi hàm bắt đầu game thực sự ở đây
            }
        }
    }

    public void StartGameCountdown()
    {
        countdownTimer = gameCountdown;
        isCountingDown = true;
        if (gameCountdownText != null)
            gameCountdownText.text = $"Game bắt đầu sau: {Mathf.CeilToInt(countdownTimer)}s";
    }

    private bool spawnedSelf = false;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"OnPlayerJoined called. player: {player}, LocalPlayer: {runner.LocalPlayer}");
        if (player != runner.LocalPlayer) return;
        if (spawnedSelf) return;
        SpawnLocalPlayer(runner, player);
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            StartGameCountdown();
            // Nếu là LocalPlayer và chưa spawn, tự spawn player cho chính mình (fix cho player 1)
            if (!spawnedSelf && runner.LocalPlayer != null)
            {
                Debug.Log("Player 1 tự spawn player cho chính mình (OnSceneLoadDone)");
                SpawnLocalPlayer(runner, runner.LocalPlayer);
            }
        }
    }

    private void SpawnLocalPlayer(NetworkRunner runner, PlayerRef player)
    {
        int team = player.RawEncoded % 2;
        Vector3 spawnPos = team == 0 ? redSpawn.position : blueSpawn.position;
        var obj = runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
        if (obj == null)
            Debug.LogError("Spawn player FAILED! Prefab chưa add vào NetworkRunner hoặc prefab lỗi.");
        else
            Debug.Log("Spawned player at: " + spawnPos);
        spawnedSelf = true;
    }

    // Các hàm callback còn lại (Fusion 2.x yêu cầu đầy đủ)
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}