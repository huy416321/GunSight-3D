using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Team")]
    [Tooltip("True nếu là police, false là team khác")]
    public bool isPolice; // Added variable to indicate if the player is police
    [Header("Health Settings")]
    public float maxHealth = 100f;
    [Networked] public float currentHealth { get; set; }

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
        if (!HasStateAuthority) return;
        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        if (currentHealth <= 0f)
        {
            Die();
        }
        // Gọi update UI nếu là local
        if (Object.HasInputAuthority)
        {
            UpdateHealthUI();
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

    private void Die()
    {
        // Xử lý chết ở đây (ẩn nhân vật, respawn, v.v.)
        Debug.Log($"Player {Object.Id} died");
    }
}
