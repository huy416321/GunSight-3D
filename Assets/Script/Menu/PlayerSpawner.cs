using Fusion;
using UnityEngine;
using Fusion.Sockets;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner runner;
    public NetworkPrefabRef player1Prefab;
    public NetworkPrefabRef player2Prefab;

    private bool spawnedSelf = false;

    void Awake()
    {
        if (runner == null)
            runner = FindFirstObjectByType<NetworkRunner>();
        if (runner != null)
        {
            runner.AddCallbacks(this);
        }
        else
        {
            Debug.LogError("PlayerSpawner: Không tìm thấy NetworkRunner trong scene!");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player != runner.LocalPlayer) return;
        if (spawnedSelf) return;
        SpawnLocalPlayer(runner, player);
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            if (!spawnedSelf && runner.LocalPlayer != null)
            {
                SpawnLocalPlayer(runner, runner.LocalPlayer);
            }
        }
    }

    private void SpawnLocalPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (spawnedSelf) return;
        // Tìm player có RawEncoded nhỏ nhất (người vào đầu tiên)
        int minRaw = int.MaxValue;
        foreach (var p in runner.ActivePlayers)
        {
            if (p.RawEncoded < minRaw) minRaw = p.RawEncoded;
        }
        NetworkPrefabRef prefab = (player.RawEncoded == minRaw) ? player1Prefab : player2Prefab;
        Vector3 spawnPos = Vector3.zero;
        var obj = runner.Spawn(prefab, spawnPos, Quaternion.identity, player);
        if (obj == null)
            Debug.LogError("Spawn player FAILED! Prefab chưa add vào NetworkRunner hoặc prefab lỗi.");
        spawnedSelf = true;
    }

    // Các hàm callback Fusion bắt buộc, để trống
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
}