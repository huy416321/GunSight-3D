using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using TMPro;
using System.Collections;
using StarterAssets;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Team")]
    [Tooltip("True nếu là police, false là team khác")]
    public bool isPolice; // Added variable to indicate if the player is police
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public Animator HitAnimator; // Animator to play hit animation
    public AudioClip hitSound; // Âm thanh khi bị trúng đạn
    [Networked] public float currentHealth { get; set; }
    [Networked] public bool isDead { get; set; }

    private void Start()
    {
        if (HasStateAuthority)
        {
            currentHealth = maxHealth;
        }
        // Đảm bảo LocalHealthUI.Instance đã có nếu là local player
        if (Object.HasInputAuthority && LocalHealthUI.Instance == null)
        {
            LocalHealthUI.Instance = FindFirstObjectByType<LocalHealthUI>();
            if (LocalHealthUI.Instance == null)
            {
                Debug.LogWarning("Không tìm thấy LocalHealthUI trên scene!");
            }
        }
        UpdateHealthUI();
    }

    public override void FixedUpdateNetwork()
    {
        // Sync UI for local player only
        if (Object.HasInputAuthority)
        {
            UpdateHealthUI();
        }
    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"TakeDamage called on {gameObject.name}, amount: {amount}, currentHealth: {currentHealth}, IsStateAuthority: {Object.HasStateAuthority}, IsInputAuthority: {Object.HasInputAuthority}");
        if (!Object.HasStateAuthority) return;
        if (isDead) return;
        HitAnimator.SetTrigger("Hit"); // Gọi animation máu
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        // Gọi RPC để tất cả client đều thấy VFX sát thương
        if (Object.HasInputAuthority)
        {
            var healthUI = LocalHealthUI.Instance;
            if (healthUI != null)
                healthUI.SetHealth(currentHealth, maxHealth);
        }
        if (currentHealth <= 0)
        {
            Debug.Log($"Player {gameObject.name} died. Calling RPC_Die. IsStateAuthority: {Object.HasStateAuthority}");
            // chết
            RPC_Die();
        }
    }

    private void UpdateHealthUI()
    {
        // Gọi UI chung cho local player
        if (Object.HasInputAuthority && LocalHealthUI.Instance != null)
        {
            LocalHealthUI.Instance.SetHealth(currentHealth, maxHealth);
        }
    }

    public void ResetFullHealth()
    {
        currentHealth = maxHealth;
        // Nếu có UI máu thì cập nhật lại
        if (LocalHealthUI.Instance != null && Object.HasInputAuthority)
            LocalHealthUI.Instance.healthSlider.value = currentHealth;
        // Nếu có hiệu ứng hồi máu thì gọi ở đây
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Die()
    {
        Debug.Log($"RPC_Die called on {gameObject.name}, IsStateAuthority: {Object.HasStateAuthority}, InputAuthority: {Object.InputAuthority}");
        isDead = true;
        // vô hiệu các hành động của người chơi
        var matchManager = FindFirstObjectByType<MatchManagerThirh>();
        if (matchManager != null && matchManager.Object.HasStateAuthority)
        {
            matchManager.OnPlayerDie(Object.InputAuthority);
        }
    }
}
