using Fusion;
using UnityEngine;
using TMPro;

public class MatchManager : NetworkBehaviour
{
    [Networked] public int player1Score { get; set; }
    [Networked] public int player2Score { get; set; }
    [Networked] public int currentRound { get; set; }
    [Networked] public bool isRoundActive { get; set; }
    [Networked] public float roundTimer { get; set; }

    public TMP_Text roundText;
    public TMP_Text player1ScoreText;
    public TMP_Text player2ScoreText;
    public TMP_Text winText;
    public TMP_Text timerText;

    public PlayerSpawner playerSpawner;
    public int maxRoundsToWin = 3;
    public float roundTimeLimit = 300f; // 5 phút

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            player1Score = 0;
            player2Score = 0;
            currentRound = 1;
            isRoundActive = true;
            roundTimer = roundTimeLimit;
        }
        UpdateUI();
    }

    private void Update()
    {
        if (!Object.HasStateAuthority || !isRoundActive) return;
        roundTimer -= Time.deltaTime;
        if (timerText) timerText.text = $"Time: {Mathf.CeilToInt(roundTimer)}";
        if (roundTimer <= 0f)
        {
            Debug.Log("[MatchManager] Round time out!");
            isRoundActive = false;
            RpcShowWinName("TIME OUT"); // hoặc gọi logic xử lý hòa
            StartCoroutine(NextRoundDelay());
        }
    }

    public void OnPlayerDie(PlayerRef loserRef)
    {
        Debug.Log($"[OnPlayerDie] StateAuthority: {Object.HasStateAuthority}, isRoundActive: {isRoundActive}");
        var runner = playerSpawner != null ? playerSpawner.runner : null;
        if (runner == null)
        {
            Debug.LogError("MatchManager: runner is null!");
            return;
        }
        var sortedPlayers = new System.Collections.Generic.List<PlayerRef>(runner.ActivePlayers);
        sortedPlayers.Sort((a, b) => a.RawEncoded.CompareTo(b.RawEncoded));
        int loserIndex = sortedPlayers.IndexOf(loserRef);
        Debug.Log($"OnPlayerDie called, loserIndex: {loserIndex}, isRoundActive: {isRoundActive}");
        if (!Object.HasStateAuthority || !isRoundActive) return;
        isRoundActive = false;
        int winnerIndex = loserIndex == 0 ? 1 : 0;
        if (winnerIndex == 0) player1Score++;
        else player2Score++;
        // Lấy tên người thắng
        PlayerRef winnerRef = sortedPlayers[winnerIndex];
        string winnerName = "";
        var winnerObj = runner.GetPlayerObject(winnerRef);
        if (winnerObj != null)
        {
            var ctrl = winnerObj.GetComponent<PlayerControllerRPC>();
            if (ctrl != null && ctrl.nameText != null)
                winnerName = ctrl.nameText.text;
        }
        UpdateUI();
        Debug.Log($"Score updated: P1={player1Score}, P2={player2Score}");
        if (player1Score >= maxRoundsToWin)
        {
            Debug.Log($"{winnerName} WIN!");
            RpcShowWinName(winnerName);
        }
        else if (player2Score >= maxRoundsToWin)
        {
            Debug.Log($"{winnerName} WIN!");
            RpcShowWinName(winnerName);
        }
        else
        {
            Debug.Log("StartCoroutine NextRoundDelay");
            StartCoroutine(NextRoundDelay());
        }
    }

    private System.Collections.IEnumerator NextRoundDelay()
    {
        yield return new WaitForSeconds(2f);
        RpcNextRound();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcNextRound()
    {
        Debug.Log($"RpcNextRound called, round: {currentRound} -> {currentRound + 1}, StateAuthority: {Object.HasStateAuthority}");
        if (Object.HasStateAuthority)
        {
            currentRound++;
            isRoundActive = true;
            roundTimer = roundTimeLimit;
        }
        winText.text = "";
        // Reset map, respawn player, hồi máu
        if (playerSpawner != null)
        {
            Debug.Log("RespawnAllPlayers called");
            playerSpawner.RespawnAllPlayers();
        }
        else
        {
            Debug.LogWarning("playerSpawner is null in MatchManager!");
        }
        UpdateUI();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcShowWinName(string winnerName)
    {
        winText.text = $"{winnerName} WIN!";
        isRoundActive = false;
    }

    private void UpdateUI()
    {
        if (roundText) roundText.text = $"Round: {currentRound}";
        if (player1ScoreText) player1ScoreText.text = player1Score.ToString();
        if (player2ScoreText) player2ScoreText.text = player2Score.ToString();
    }
}
