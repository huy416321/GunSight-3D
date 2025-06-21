using Fusion;
using UnityEngine;
using TMPro;

public class MatchManager : NetworkBehaviour
{
    [Networked] public int player1Score { get; set; }
    [Networked] public int player2Score { get; set; }
    [Networked] public int currentRound { get; set; }
    [Networked] public bool isRoundActive { get; set; }

    public TMP_Text roundText;
    public TMP_Text player1ScoreText;
    public TMP_Text player2ScoreText;
    public TMP_Text winText;

    public PlayerSpawner playerSpawner;
    public int maxRoundsToWin = 3;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            player1Score = 0;
            player2Score = 0;
            currentRound = 1;
            isRoundActive = true;
        }
        UpdateUI();
    }

    public void OnPlayerDie(int loserIndex)
    {
        if (!Object.HasStateAuthority || !isRoundActive) return;
        isRoundActive = false;
        int winnerIndex = loserIndex == 0 ? 1 : 0;
        if (winnerIndex == 0) player1Score++;
        else player2Score++;
        UpdateUI();
        if (player1Score >= maxRoundsToWin)
        {
            RpcShowWin(1);
        }
        else if (player2Score >= maxRoundsToWin)
        {
            RpcShowWin(2);
        }
        else
        {
            Runner.Invoke(nameof(RpcNextRound), 2f); // delay 2s rồi reset round
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcNextRound()
    {
        currentRound++;
        isRoundActive = true;
        winText.text = "";
        // Reset map, respawn player, hồi máu
        playerSpawner.RespawnAllPlayers();
        UpdateUI();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcShowWin(int winner)
    {
        winText.text = $"Player {winner} WIN!";
        isRoundActive = false;
    }

    private void UpdateUI()
    {
        if (roundText) roundText.text = $"Round: {currentRound}";
        if (player1ScoreText) player1ScoreText.text = player1Score.ToString();
        if (player2ScoreText) player2ScoreText.text = player2Score.ToString();
    }
}
