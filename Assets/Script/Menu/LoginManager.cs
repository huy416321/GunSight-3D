using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [Header("Common UI")]
    public Text statusText;

    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Login UI")]
    public InputField loginEmailInput;
    public InputField loginPasswordInput;
    public Button loginButton;
    public Button switchToRegisterButton;

    [Header("Register UI")]
    public InputField registerEmailInput;
    public InputField registerPasswordInput;
    public Button registerButton;
    public Button switchToLoginButton;

    private FirebaseAuth auth;
    private DatabaseReference db;

    async void Start()
    {
        // Setup Firebase
        var depStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (depStatus == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseDatabase.DefaultInstance.RootReference;
        }
        else
        {
            statusText.text = "Firebase Error: " + depStatus;
        }

        // Gắn sự kiện nút
        loginButton.onClick.AddListener(Login);
        registerButton.onClick.AddListener(Register);
        switchToRegisterButton.onClick.AddListener(() => SwitchPanel(false));
        switchToLoginButton.onClick.AddListener(() => SwitchPanel(true));

        // Mặc định hiển thị login
        SwitchPanel(true);
    }

    void SwitchPanel(bool showLogin)
    {
        loginPanel.SetActive(showLogin);
        registerPanel.SetActive(!showLogin);
        statusText.text = "";
    }

    public async void Login()
    {
        try
        {
            var userCredential = await auth.SignInWithEmailAndPasswordAsync(
                loginEmailInput.text, loginPasswordInput.text);

            string userId = userCredential.User.UserId;
            PlayerPrefs.SetString("userId", userId);

            var snapshot = await db.Child("users").Child(userId).Child("playerName").GetValueAsync();

            if (snapshot.Exists)
            {
                string playerName = snapshot.Value.ToString();
                PlayerPrefs.SetString("playerName", playerName);
                SceneManager.LoadScene("LobbyScene");
            }
            else
            {
                SceneManager.LoadScene("EnterNameScene");
            }
        }
        catch (System.Exception e)
        {
            statusText.text = "Đăng nhập lỗi: " + e.Message;
        }
    }

    public async void Register()
    {
        try
        {
            var userCredential = await auth.CreateUserWithEmailAndPasswordAsync(
                registerEmailInput.text, registerPasswordInput.text);

            PlayerPrefs.SetString("userId", userCredential.User.UserId);
            SceneManager.LoadScene("EnterNameScene");
        }
        catch (System.Exception e)
        {
            statusText.text = "Đăng ký lỗi: " + e.Message;
        }
    }
}
