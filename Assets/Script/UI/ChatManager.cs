using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

public class ChatManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private TextMeshProUGUI chatContentText;
    [SerializeField] private ScrollRect chatScrollRect;
    private MenuManager menuManager;
    [Networked] private NetworkString<_256> lastMessage { get; set; }

    private void Awake()
    {
        if (chatInputField != null)
            chatInputField.onEndEdit.AddListener(OnSendMessage);
    }

    private void OnSendMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            chatInputField.text = "";
            chatInputField.ActivateInputField();
            RPC_SendMessage(Object.InputAuthority, message);
        }
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
