using Fusion;
using UnityEngine;
using TMPro;

public class MatchManagerThirh : NetworkBehaviour
{
    private float checkTeamTimer = 0f;
    private int deadPolice = 0;
    private int deadRobber = 0;
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
            deadPolice = 0;
            deadRobber = 0;
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
                player2Score++;
                UpdateUI();
                RpcShowWinName("Cướp WIN! (Hết giờ)", player1Score, player2Score);
                if (player1Score < maxRoundsToWin && player2Score < maxRoundsToWin)
                {
                    StartCoroutine(NextRoundDelay());
                }
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

        // Đếm số lượng người chết mỗi team
        var loserObj = runner.GetPlayerObject(loserRef);
        if (loserObj != null)
        {
            var ctrl = loserObj.GetComponent<PlayerHealth>();
            if (ctrl != null)
            {
                if (ctrl.isPolice) deadPolice++;
                else deadRobber++;
            }
        }

        // Nếu có 2 người của một team chết thì team còn lại win
        if (deadPolice >= 2 || deadRobber >= 2)
        {
            isRoundActive = false;
            if (deadPolice >= 2)
            {
                player2Score++;
                RpcShowWinName("Cướp WIN! (Cảnh sát chết đủ)", player1Score, player2Score);
            }
            else
            {
                player1Score++;
                RpcShowWinName("Cảnh WIN! (Cướp chết đủ)", player1Score, player2Score);
            }
            UpdateUI();
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


// Truyền cả điểm số để mọi client đều cập nhật đúng
[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
private void RpcShowWinName(string winMessage, int p1Score, int p2Score)
{
    winText.text = winMessage;
    isRoundActive = false;
    player1Score = p1Score;
    player2Score = p2Score;
    UpdateUI();
    // Nếu đã kết thúc trận thì gọi UI end game ngoài
    if (player1Score >= maxRoundsToWin || player2Score >= maxRoundsToWin)
    {
    var ui = UnityEngine.Object.FindFirstObjectByType<EndGameUIManager>();
        if (ui != null) ui.ShowEndGame(winMessage);
    }
}


    private void UpdateUI()
    {
        if (roundText) roundText.text = $"Round: {currentRound}";
        if (player1ScoreText) player1ScoreText.text = player1Score.ToString();
        if (player2ScoreText) player2ScoreText.text = player2Score.ToString();
    }
}
