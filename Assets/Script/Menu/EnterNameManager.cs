using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using UnityEngine.SceneManagement;
using TMPro;

public class EnterNameManager : MonoBehaviour
{
    public TMP_InputField nameInput;
    public Button confirmButton;
    public TextMeshProUGUI statusText;

    private DatabaseReference db;

    async void Start()
    {
        // Gắn sự kiện cho nút xác nhận
        confirmButton.onClick.AddListener(OnConfirm);

        // Khởi tạo Firebase Database
        var depStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (depStatus == DependencyStatus.Available)
        {
            db = FirebaseDatabase.DefaultInstance.RootReference;
        }
        else
        {
            statusText.text = "Lỗi Firebase: " + depStatus;
        }
    }

    public async void OnConfirm()
    {
        string playerName = nameInput.text.Trim();
        string userId = PlayerPrefs.GetString("userId", "");

        if (string.IsNullOrEmpty(playerName))
        {
            statusText.text = "Vui lòng nhập tên!";
            return;
        }

        if (string.IsNullOrEmpty(userId))
        {
            statusText.text = "Không tìm thấy userId!";
            return;
        }

        try
        {
            // Ghi tên vào Firebase
            await db.Child("users").Child(userId).Child("playerName").SetValueAsync(playerName);

            // Lưu vào PlayerPrefs để dùng sau
            PlayerPrefs.SetString("playerName", playerName);

            // Chuyển sang LobbyScene
            SceneManager.LoadScene("LobbyScene");
        }
        catch (System.Exception e)
        {
            statusText.text = "Lỗi khi lưu tên: " + e.Message;
        }
    }
}
