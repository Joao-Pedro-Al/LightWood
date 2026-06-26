using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Game Over")]
    public string gameOverSceneName = "GameOver";
    public bool useGameOverCanvas = false;
    public GameObject gameOverCanvas;

    [Header("UI de Vida (opcional)")]
    public Slider healthSlider;

    [Header("Efeito de Dano — Overlay Vermelho")]
    public Image damageOverlay;
    [Range(0f, 1f)] public float damageFlashAlpha = 0.45f;
    public float overlayFadeSpeed = 2f;

    [Header("Efeito de Sangue nas Bordas")]
    public Image bloodVignetteOverlay;
    [Range(0f, 1f)] public float bloodVignetteMaxAlpha = 0.8f;
    public float bloodFadeSpeed = 0.8f;

    private bool isDead = false;
    private Player_Teste_Alves playerController;
    private float damageOverlayAlpha = 0f;
    private float bloodVignetteAlpha = 0f;

    void Awake()
    {
        currentHealth = maxHealth;
        playerController = GetComponent<Player_Teste_Alves>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        SetOverlayAlpha(damageOverlay, 0f);
        SetOverlayAlpha(bloodVignetteOverlay, 0f);

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);
    }

    void Update()
    {
        if (damageOverlay != null && damageOverlayAlpha > 0f)
        {
            damageOverlayAlpha -= Time.deltaTime * overlayFadeSpeed;
            damageOverlayAlpha = Mathf.Max(damageOverlayAlpha, 0f);
            SetOverlayAlpha(damageOverlay, damageOverlayAlpha);
        }

        if (bloodVignetteOverlay != null && bloodVignetteAlpha > 0f)
        {
            bloodVignetteAlpha -= Time.deltaTime * bloodFadeSpeed;
            bloodVignetteAlpha = Mathf.Max(bloodVignetteAlpha, 0f);
            SetOverlayAlpha(bloodVignetteOverlay, bloodVignetteAlpha);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"[Player] ❤️ Vida: {currentHealth}/{maxHealth}");

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        damageOverlayAlpha = damageFlashAlpha;
        SetOverlayAlpha(damageOverlay, damageOverlayAlpha);

        float healthRatio = currentHealth / maxHealth;
        float vignetteIntensity = Mathf.Lerp(bloodVignetteMaxAlpha, bloodVignetteMaxAlpha * 0.5f, healthRatio);
        bloodVignetteAlpha = Mathf.Max(bloodVignetteAlpha, vignetteIntensity);
        SetOverlayAlpha(bloodVignetteOverlay, bloodVignetteAlpha);

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (healthSlider != null) healthSlider.value = currentHealth;
        Debug.Log($"[Player] 💊 Curado! Vida: {currentHealth}/{maxHealth}");
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[Player] 💀 MORREU — a recarregar cena...");

        // O GeradorSalvamento já tem tudo guardado em tempo real
        // Não é preciso fazer nada extra aqui

        if (playerController != null)
            playerController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (useGameOverCanvas)
        {
            if (gameOverCanvas != null)
                gameOverCanvas.SetActive(true);
            else
                Debug.LogWarning("[Player] ⚠️ useGameOverCanvas = true mas gameOverCanvas não está atribuído!");
        }
        else
        {
            if (!string.IsNullOrEmpty(gameOverSceneName))
                SceneManager.LoadScene(gameOverSceneName);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void SetOverlayAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    public bool IsDead() => isDead;
    public float GetHealthPercent() => currentHealth / maxHealth;
}