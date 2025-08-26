using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Fusion;

public class EndGameUIManager : MonoBehaviour
{
    public GameObject endGamePanel;
    public TMP_Text endGameMessageText;

    // Gọi hàm này để hiện panel end game với message
    public void ShowEndGame(string message)
    {
        if (endGamePanel != null)
        { 
            endGamePanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (endGameMessageText != null)
                endGameMessageText.text = message + "\nBạn muốn làm gì tiếp?";
        }
    }

    // Nút về lobby
    public void OnReturnToLobbyButton()
    {
        StartCoroutine(ReturnToLobbyCoroutine());
    }

    private System.Collections.IEnumerator ReturnToLobbyCoroutine()
    {
        // Nếu có Fusion Runner thì shutdown
        var runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
        {
            runner.Shutdown();
            yield return null;
        }
        SceneManager.LoadScene("LobbyScene");
    }

    // Nút thoát game
    public void OnQuitButton()
    {
        Application.Quit();
    }
}
