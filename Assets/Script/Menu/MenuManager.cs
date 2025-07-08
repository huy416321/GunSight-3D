using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;

    private FirebaseAuth auth;
    private DatabaseReference db;

    async void Start()
    {
        // Khởi tạo Firebase
        var depStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (depStatus == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseDatabase.DefaultInstance.RootReference;

            await LoadPlayerNameFromFirebase();
        }
        else
        {
            playerNameText.text = "Lỗi Firebase: " + depStatus;
        }
    }

    async System.Threading.Tasks.Task LoadPlayerNameFromFirebase()
    {
        string userId = auth.CurrentUser != null ? auth.CurrentUser.UserId : PlayerPrefs.GetString("userId", "");

        if (string.IsNullOrEmpty(userId))
        {
            playerNameText.text = "Không tìm thấy tài khoản!";
            return;
        }

        var snapshot = await db.Child("users").Child(userId).Child("playerName").GetValueAsync();

        if (snapshot.Exists)
        {
            string playerName = snapshot.Value.ToString();
            playerNameText.text = playerName;
        }
        else
        {
            playerNameText.text = "Chưa đặt tên!";
        }
    }
}
