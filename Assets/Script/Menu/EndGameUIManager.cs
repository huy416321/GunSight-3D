using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Fusion;

public class EndGameUIManager : MonoBehaviour
{
    public GameObject endGame2;
    public GameObject endGame1;
    public TMP_Text endGameMessageText;

    // Gọi hàm này để hiện panel end game với message

    public void ShowEndGame1(string message)
    {
        if (endGame1 != null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            endGame1.SetActive(true);
            if (endGameMessageText != null)
                endGameMessageText.text = message + "\nBạn muốn làm gì tiếp?";
            StartCoroutine(LoadLobbyAfterDelay());
        }
    }

    public void ShowEndGame2(string message)
    {
        if (endGame2 != null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            endGame2.SetActive(true);
            if (endGameMessageText != null)
                endGameMessageText.text = message + "\nBạn muốn làm gì tiếp?";
            StartCoroutine(LoadLobbyAfterDelay());
        }
    }

     private System.Collections.IEnumerator LoadLobbyAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("LobbyScene");
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
