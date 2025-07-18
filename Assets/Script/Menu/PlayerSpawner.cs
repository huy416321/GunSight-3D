using Fusion;
using UnityEngine;
using Fusion.Sockets;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner runner;
    public NetworkPrefabRef player1Prefab;
    public NetworkPrefabRef player2Prefab;
    public NetworkPrefabRef player3Prefab;
    public NetworkPrefabRef player4Prefab;

    public Vector3 player1SpawnPos = new Vector3(0, 0, 0); // Vị trí spawn cho player 1
    public Vector3 player2SpawnPos = new Vector3(10, 0, 0); // Vị trí spawn cho player 2
    public Vector3 player3SpawnPos = new Vector3(-10, 0, 0); // Vị trí spawn cho player 3
    public Vector3 player4SpawnPos = new Vector3(0, 0, 10); // Vị trí spawn cho player 4

    private bool spawnedSelf = false;
    private NetworkObject spawnedPlayerObj;

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
        // Đảm bảo ActivePlayers đã được sắp xếp theo RawEncoded tăng dần
        var sortedPlayers = new System.Collections.Generic.List<PlayerRef>(runner.ActivePlayers);
        sortedPlayers.Sort((a, b) => a.RawEncoded.CompareTo(b.RawEncoded));
        int index = sortedPlayers.IndexOf(player);
        NetworkPrefabRef prefab;
        Vector3 spawnPos;
        switch (index)
        {
            case 0:
                prefab = player1Prefab;
                spawnPos = player1SpawnPos;
                break;
            case 1:
                prefab = player2Prefab;
                spawnPos = player2SpawnPos;
                break;
            case 2:
                prefab = player3Prefab;
                spawnPos = player3SpawnPos;
                break;
            case 3:
                prefab = player4Prefab;
                spawnPos = player4SpawnPos;
                break;
            default:
                prefab = player1Prefab;
                spawnPos = player1SpawnPos;
                break;
        }
        // Xoá player cũ nếu còn
        if (spawnedPlayerObj != null && spawnedPlayerObj.IsValid)
        {
            runner.Despawn(spawnedPlayerObj);
        }
        var obj = runner.Spawn(prefab, spawnPos, Quaternion.identity, player);
        if (obj == null)
            Debug.LogError("Spawn player FAILED! Prefab chưa add vào NetworkRunner hoặc prefab lỗi.");
        spawnedPlayerObj = obj;
        spawnedSelf = true;
    }

    // Thêm hàm này để MatchManager gọi khi reset round
    public void RespawnAllPlayers()
    {
        spawnedSelf = false;
        // Xoá player cũ nếu còn
        if (spawnedPlayerObj != null && spawnedPlayerObj.IsValid)
        {
            runner.Despawn(spawnedPlayerObj);
            spawnedPlayerObj = null;
        }
        SpawnLocalPlayer(runner, runner.LocalPlayer);
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