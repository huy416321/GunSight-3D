using Fusion;
using UnityEngine;
using TMPro;

public class MatchManager : NetworkBehaviour
{
    private int alivePolice = 0;
    private int aliveRobber = 0;
    [Networked] public int player1Score { get; set; }
    [Networked] public int player2Score { get; set; }
    [Networked] public int currentRound { get; set; }
    [Networked] public bool isRoundActive { get; set; }
    [Networked] public float roundTimer { get; set; }
    [Networked] public bool isWaitingRound { get; set; }

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
            currentRound = 0;
            isRoundActive = false;
            roundTimer = roundTimeLimit;

            // Đếm số lượng player mỗi team
            alivePolice = 0;
            aliveRobber = 0;
            var runner = playerSpawner != null ? playerSpawner.runner : null;
            if (runner != null)
            {
                foreach (var p in runner.ActivePlayers)
                {
                    var obj = runner.GetPlayerObject(p);
                    if (obj != null)
                    {
                        var ctrl = obj.GetComponent<PlayerControllerRPC>();
                        if (ctrl != null)
                        {
                            if (ctrl.isPolice) alivePolice++;
                            else aliveRobber++;
                        }
                    }
                }
            }
            // Đếm ngược round 1
            StartCoroutine(RoundCountdownCoroutine());
        }
        UpdateUI();
    }

    private void Update()
    {
        // Chỉ giảm timer ở StateAuthority
        if (Object.HasStateAuthority && isRoundActive)
        {
            roundTimer -= Time.deltaTime;
            if (roundTimer <= 0f)
            {
                Debug.Log("[MatchManager] Round time out!");
                isRoundActive = false;
                RpcShowWinName("TIME OUT"); // hoặc gọi logic xử lý hòa
                StartCoroutine(NextRoundDelay());
            }
        }
        // Luôn cập nhật UI timer cho mọi client
        if (timerText) timerText.text = $"Time: {Mathf.CeilToInt(roundTimer)}";
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

        // Giảm số lượng player còn sống của team
        var loserObj = runner.GetPlayerObject(loserRef);
        if (loserObj != null)
        {
            var ctrl = loserObj.GetComponent<PlayerControllerRPC>();
            if (ctrl != null)
            {
                if (ctrl.isPolice) alivePolice--;
                else aliveRobber--;
            }
        }

        // Kiểm tra nếu một team bị loại hết
        if (alivePolice <= 0 || aliveRobber <= 0)
        {
            isRoundActive = false;
            int winnerIndex = loserIndex == 0 ? 1 : 0;
            if (winnerIndex == 0) player1Score++;
            else player2Score++;
            UpdateUI();
            Debug.Log($"Score updated: P1={player1Score}, P2={player2Score}");
            string winMsg = "";
            if (player1Score >= maxRoundsToWin || winnerIndex == 0)
                winMsg = "Cảnh WIN!";
            else if (player2Score >= maxRoundsToWin || winnerIndex == 1)
                winMsg = "Cướp WIN!";
            Debug.Log(winMsg);
            RpcShowWinName(winMsg);
            if (player1Score < maxRoundsToWin && player2Score < maxRoundsToWin)
            {
                StartCoroutine(NextRoundDelay());
            }
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
        StartCoroutine(RoundCountdownCoroutine());
    }

    private System.Collections.IEnumerator RoundCountdownCoroutine()
    {
        isWaitingRound = true;
        int countdown = 5;
        for (int i = countdown; i > 0; i--)
        {
            if (winText != null)
                winText.text = $"Round {currentRound + 1} bắt đầu sau: {i}s";
            yield return new WaitForSeconds(1f);
        }
        winText.text = "";
        if (Object.HasStateAuthority)
        {
            currentRound++;
            isRoundActive = true;
            roundTimer = roundTimeLimit;
        }
        isWaitingRound = false;
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
    private void RpcShowWinName(string winMessage)
    {
        winText.text = winMessage;
        isRoundActive = false;
        // Chỉ về lobby nếu một team thắng đủ 3 round
        if (player1Score >= maxRoundsToWin || player2Score >= maxRoundsToWin)
        {
            StartCoroutine(ReturnToLobbyCoroutine());
        }
    }

    private System.Collections.IEnumerator ReturnToLobbyCoroutine()
    {
        yield return new WaitForSeconds(2f);
        // Ngắt kết nối Fusion
        if (Runner != null)
        {
            Runner.Shutdown();
        }
        // Load lại scene lobby (giả sử tên scene là "Lobby")
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }

    private void UpdateUI()
    {
        if (roundText) roundText.text = $"Round: {currentRound}";
        if (player1ScoreText) player1ScoreText.text = player1Score.ToString();
        if (player2ScoreText) player2ScoreText.text = player2Score.ToString();
    }
}
