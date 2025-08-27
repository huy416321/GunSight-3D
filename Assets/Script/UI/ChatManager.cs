using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;
using System.Collections;

public class ChatManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private TextMeshProUGUI chatContentText;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private GameObject chatPanel; // Thêm panel chat để ẩn/hiện
    public bool isChatActive = false;
    private Coroutine hideChatCoroutine;
    [Networked] private NetworkString<_256> lastMessage { get; set; }

    private void Awake()
    {
        if (chatInputField != null)
            chatInputField.onEndEdit.AddListener(OnSendMessage);
        if (chatPanel != null)
            chatPanel.SetActive(false);
    }

    private void Update()
    {
        // Bấm Enter lần đầu hiện chat, lần hai bắt đầu đếm 3 giây để tắt
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isChatActive)
            {
                // Lần đầu: hiện chat, cho nhập
                if (chatPanel != null)
                    chatPanel.SetActive(true);
                if (chatInputField != null)
                {
                    chatInputField.ActivateInputField();
                    chatInputField.Select();
                }
                if (hideChatCoroutine != null)
                {
                    StopCoroutine(hideChatCoroutine);
                    hideChatCoroutine = null;
                }
                isChatActive = true;
            }
            else
            {
                // Lần hai: bắt đầu đếm 3 giây để tắt chat
                if (hideChatCoroutine != null)
                {
                    StopCoroutine(hideChatCoroutine);
                }
                hideChatCoroutine = StartCoroutine(HideChatAfterDelay());
                isChatActive = false;
            }
        }
    }

    private void OnSendMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            chatInputField.text = "";
            chatInputField.ActivateInputField();
            RPC_SendMessage(Object.InputAuthority, message);
            // Sau khi gửi thì bắt đầu ẩn chat sau 3 giây
            if (hideChatCoroutine != null && !isChatActive)
                StopCoroutine(hideChatCoroutine);
            hideChatCoroutine = StartCoroutine(HideChatAfterDelay());
        }
    }

    private IEnumerator HideChatAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        if (chatPanel != null)
            chatPanel.SetActive(false);
        isChatActive = false;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendMessage(PlayerRef sender, string message)
    {
        string senderName = $"{sender.RawEncoded}";
        string formatted = $"<color=#00FF00>{senderName}:</color> {message}\n";
        if (chatContentText != null)
        {
            chatContentText.text += formatted;
            Canvas.ForceUpdateCanvases();
            // Không tự động cuộn xuống nữa
            // if (chatScrollRect != null)
            //     chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
