using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════
    // CONFIGURAÇÃO
    // ═══════════════════════════════════════════════════════
    [Header("Vida")]
    [Tooltip("Vida máxima do player.")]
    public float maxHealth = 100f;

    [Tooltip("Vida atual (começa igual à máxima).")]
    public float currentHealth;

    [Header("Game Over")]
    [Tooltip("Nome da cena de Game Over (tem de existir no Build Settings).")]
    public string gameOverSceneName = "GameOver";

    [Tooltip("Se ativado, em vez de carregar uma cena mostra um Canvas de Game Over.")]
    public bool useGameOverCanvas = false;

    [Tooltip("Canvas de Game Over (só usado se useGameOverCanvas = true).")]
    public GameObject gameOverCanvas;

    [Header("UI de Vida (opcional)")]
    [Tooltip("Slider que representa a barra de vida. Pode ficar vazio.")]
    public Slider healthSlider;

    [Header("Efeito de Dano — Overlay Vermelho")]
    [Tooltip("Imagem de ecrã vermelho que pisca ao receber dano.")]
    public Image damageOverlay;

    [Tooltip("Alpha máximo do flash vermelho (0 = invisível, 1 = opaco).")]
    [Range(0f, 1f)]
    public float damageFlashAlpha = 0.45f;

    [Tooltip("Velocidade de desvanecimento do flash vermelho.")]
    public float overlayFadeSpeed = 2f;

    [Header("Efeito de Sangue nas Bordas")]
    [Tooltip("Imagem de vinheta de sangue nas bordas do ecrã.")]
    public Image bloodVignetteOverlay;

    [Tooltip("Alpha máximo da vinheta de sangue.")]
    [Range(0f, 1f)]
    public float bloodVignetteMaxAlpha = 0.8f;

    [Tooltip("Velocidade de desvanecimento da vinheta (mais lento que o flash).")]
    public float bloodFadeSpeed = 0.8f;

    // ═══════════════════════════════════════════════════════
    // ESTADO INTERNO
    // ═══════════════════════════════════════════════════════
    private bool isDead = false;
    private Player_Teste_Alves playerController;
    private float damageOverlayAlpha = 0f;
    private float bloodVignetteAlpha = 0f;

    // ═══════════════════════════════════════════════════════
    // AWAKE — corre antes de qualquer Start() ou colisão
    // ═══════════════════════════════════════════════════════
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

    // ═══════════════════════════════════════════════════════
    // UPDATE — fade dos overlays
    // ═══════════════════════════════════════════════════════
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

    // ═══════════════════════════════════════════════════════
    // RECEBER DANO — chamado pelo MonsterAI e TAKING_DAMAGE
    // ═══════════════════════════════════════════════════════
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"[Player] ❤️ Vida: {currentHealth}/{maxHealth}");

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        // Flash vermelho imediato
        damageOverlayAlpha = damageFlashAlpha;
        SetOverlayAlpha(damageOverlay, damageOverlayAlpha);

        // Vinheta de sangue — mais intensa com menos vida
        float healthRatio = currentHealth / maxHealth;
        float vignetteIntensity = Mathf.Lerp(bloodVignetteMaxAlpha, bloodVignetteMaxAlpha * 0.5f, healthRatio);
        bloodVignetteAlpha = Mathf.Max(bloodVignetteAlpha, vignetteIntensity);
        SetOverlayAlpha(bloodVignetteOverlay, bloodVignetteAlpha);

        if (currentHealth <= 0f)
            Die();
    }

    // ═══════════════════════════════════════════════════════
    // CURAR
    // ═══════════════════════════════════════════════════════
    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        Debug.Log($"[Player] 💊 Curado! Vida: {currentHealth}/{maxHealth}");
    }

    // ═══════════════════════════════════════════════════════
    // MORTE
    // ═══════════════════════════════════════════════════════
    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[Player] 💀 MORREU — GAME OVER");

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
                Debug.LogWarning("[Player] ⚠️ gameOverSceneName está vazio!");
        }
    }

    // ═══════════════════════════════════════════════════════
    // UTILITÁRIO
    // ═══════════════════════════════════════════════════════
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