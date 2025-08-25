using Fusion;
using UnityEngine;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using StarterAssets;

public class PlayerSpawnerThird : MonoBehaviour, INetworkRunnerCallbacks
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
        if (SceneManager.GetActiveScene().name == "RankedScene")
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
        {
            Debug.LogError("Spawn player FAILED! Prefab chưa add vào NetworkRunner hoặc prefab lỗi.");
        }
        else
        {
            runner.SetPlayerObject(player, obj); // Gán PlayerObject cho PlayerRef
            // Đảm bảo các script điều khiển di chuyển được enable lại
            var moveCtrl = obj.GetComponent<StarterAssets.ThirdPersonController>();
            var inputCtrl = obj.GetComponent<StarterAssetsInputs>();
            Debug.Log($"[SPAWN] PlayerObj: {obj.name}, HasInputAuthority: {obj.HasInputAuthority}, PlayerRef: {player}");
            Debug.Log($"[SPAWN] ThirdPersonController enabled: {(moveCtrl != null ? moveCtrl.enabled.ToString() : "null")}");
            Debug.Log($"[SPAWN] StarterAssetsInputs enabled: {(inputCtrl != null ? inputCtrl.enabled.ToString() : "null")}");
            if (moveCtrl != null) moveCtrl.enabled = true;
            if (inputCtrl != null) inputCtrl.enabled = true;
        }
        spawnedPlayerObj = obj;
        spawnedSelf = true;
    }

    // Thêm hàm này để MatchManager gọi khi reset round
    public void RespawnAllPlayers()
    {
        // Teleport tất cả player về vị trí ban đầu
        var sortedPlayers = new System.Collections.Generic.List<PlayerRef>(runner.ActivePlayers);
        sortedPlayers.Sort((a, b) => a.RawEncoded.CompareTo(b.RawEncoded));
        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            var playerRef = sortedPlayers[i];
            var obj = runner.GetPlayerObject(playerRef) as NetworkObject;
            if (obj != null && obj.HasStateAuthority)
            {
                Vector3 spawnPos = player1SpawnPos;
                if (i == 1) spawnPos = player2SpawnPos;
                else if (i == 2) spawnPos = player3SpawnPos;
                else if (i == 3) spawnPos = player4SpawnPos;
                obj.transform.position = spawnPos;
                obj.transform.rotation = Quaternion.identity;
            }
        }
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