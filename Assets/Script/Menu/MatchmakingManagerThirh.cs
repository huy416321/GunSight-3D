using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections;

public class MatchmakingManagerThirh : MonoBehaviour
{
    // Các GameObject sẽ kích hoạt lần lượt khi có người chơi mới
    public GameObject[] activateOnPlayerJoin;
    public GameObject panelModeSelect;
    public GameObject panelPrepare;
    public GameObject panelMatchmaking;
    public GameObject panelGame;
    public GameObject panelMain; // Thêm biến này
    public TMP_Text matchmakingTimerText;
    public Button quickMatchBtn, rankBtn, prepareStartBtn, matchmakingCancelBtn;
    public Button mainStartBtn; // Nút Start Game ở panel chính

    public NetworkRunner runner;

    private float matchmakingTime = 0f;

    public enum MatchMode { Host, Client, Shared }
    public MatchMode matchMode = MatchMode.Host;

    void Start()
    {
        ShowPanel(panelMain); // Hiện panel chính đầu tiên
        mainStartBtn.onClick.AddListener(() => ShowPanel(panelModeSelect)); // Bấm Start Game -> panel chọn chế độ
        // quickMatchBtn.onClick.AddListener(() => OnModeSelected("QuickMatch"));
        rankBtn.onClick.AddListener(() => OnModeSelected("Rank"));
        prepareStartBtn.onClick.AddListener(StartMatchmaking);
        matchmakingCancelBtn.onClick.AddListener(CancelMatchmaking);

        // Ẩn tất cả các GameObject khi bắt đầu
        if (activateOnPlayerJoin != null)
        {
            foreach (var obj in activateOnPlayerJoin)
                if (obj != null) obj.SetActive(false);
        }
    }

    void ShowPanel(GameObject panel)
    {
        panelMain.SetActive(false); // Ẩn panel chính
        panelModeSelect.SetActive(false);
        panelPrepare.SetActive(false);
        panelMatchmaking.SetActive(false);
        panelGame.SetActive(false);
        panel.SetActive(true);
    }

    void OnModeSelected(string mode)
    {
        // Lưu mode nếu cần
        StartCoroutine(WaitForChangePanel()); // Chọn chế độ xong -> panel chuẩn bị
    }
    IEnumerator WaitForChangePanel()
    {
        yield return new WaitForSeconds(1f);
        // ShowPanel(panelPrepare);
    }

    async void StartMatchmaking()
    {
        ShowPanel(panelMatchmaking);
        matchmakingTime = 0f;

        if (runner != null)
        {
            GameMode fusionMode = GameMode.Host;
            switch (matchMode)
            {
                case MatchMode.Host: fusionMode = GameMode.Host; break;
                case MatchMode.Client: fusionMode = GameMode.Client; break;
                case MatchMode.Shared: fusionMode = GameMode.Shared; break;
            }
            Debug.Log($"Starting Fusion with mode: {fusionMode}");
            var result = await runner.StartGame(new StartGameArgs()
            {
                GameMode = fusionMode,
                SessionName = "MyRoom",
                SceneManager = runner.GetComponent<INetworkSceneManager>()
            });
            Debug.Log("StartGame result: " + result.Ok + " - " + result.ShutdownReason);
        }

        StartCoroutine(MatchmakingTimer());
        // Gọi Fusion tìm phòng hoặc tạo phòng
        // FusionNetworkRunner.Instance.StartGame...
    }

    IEnumerator MatchmakingTimer()
    {
        int lastPlayerCount = 0;
        while (true)
        {
            matchmakingTime += Time.deltaTime;
            matchmakingTimerText.text = $"Đang tìm trận: {matchmakingTime:F1}s";

            // Kiểm tra số lượng người chơi (trừ chính mình)
            int playerCount = runner != null && runner.SessionInfo != null ? runner.SessionInfo.PlayerCount : 1;
            int otherPlayers = Mathf.Max(0, playerCount - 1);
            if (activateOnPlayerJoin != null)
            {
                for (int i = 0; i < activateOnPlayerJoin.Length; i++)
                {
                    if (activateOnPlayerJoin[i] != null)
                        activateOnPlayerJoin[i].SetActive(i < otherPlayers);
                }
            }
            lastPlayerCount = playerCount;

            // Kiểm tra nếu đã đủ 2 người
            if (CheckEnoughPlayers())
            {
                // Đủ người thì chuyển scene, đếm ngược sẽ xử lý ở PlayerSpawner
                if (runner != null)
                {
                    runner.LoadScene("RankedScene");
                }
                yield break;
            }
            yield return null;
        }
    }

    bool CheckEnoughPlayers()
    {
        if (runner == null)
            runner = FindFirstObjectByType<NetworkRunner>();
        if (runner != null && runner.SessionInfo != null)
            return runner.SessionInfo.PlayerCount >= 1;
        return false;
    }

    void CancelMatchmaking()
    {
        // Hủy ghép trận và ngắt kết nối mạng
        if (runner != null)
        {
            runner.Shutdown();
        }
        ShowPanel(panelPrepare);
    }
}