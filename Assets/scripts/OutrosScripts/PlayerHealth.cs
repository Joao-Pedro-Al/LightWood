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

    [Tooltip("Imagem de ecrã vermelho que pisca ao receber dano. Pode ficar vazia.")]
    public Image damageOverlay;

    [Tooltip("Quanto tempo o overlay demora a desaparecer.")]
    public float overlayFadeSpeed = 2f;

    // ═══════════════════════════════════════════════════════
    // ESTADO INTERNO
    // ═══════════════════════════════════════════════════════
    private bool isDead = false;
    private Player_Teste_Alves playerController;

    // ═══════════════════════════════════════════════════════
    // INÍCIO
    // ═══════════════════════════════════════════════════════
    void Start()
    {
        currentHealth = maxHealth;
        playerController = GetComponent<Player_Teste_Alves>();

        // Configura o slider se existir
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Garante que o overlay começa transparente
        if (damageOverlay != null)
        {
            Color c = damageOverlay.color;
            c.a = 0f;
            damageOverlay.color = c;
        }

        // Esconde o canvas de Game Over se existir
        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════
    // UPDATE — fade do overlay de dano
    // ═══════════════════════════════════════════════════════
    void Update()
    {
        // Faz o overlay vermelho desaparecer gradualmente
        if (damageOverlay != null && damageOverlay.color.a > 0f)
        {
            Color c = damageOverlay.color;
            c.a -= Time.deltaTime * overlayFadeSpeed;
            c.a = Mathf.Max(c.a, 0f);
            damageOverlay.color = c;
        }
    }

    // ═══════════════════════════════════════════════════════
    // RECEBER DANO — chamado pelo MonsterAI
    // ═══════════════════════════════════════════════════════
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"[Player] ❤️ Vida: {currentHealth}/{maxHealth}");

        // Atualiza o slider
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        // Mostra o flash vermelho
        if (damageOverlay != null)
        {
            Color c = damageOverlay.color;
            c.a = 0.6f; // intensidade do flash
            damageOverlay.color = c;
        }

        if (currentHealth <= 0f)
            Die();
    }

    // ═══════════════════════════════════════════════════════
    // CURAR (opcional, para uso futuro)
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

        // Para o movimento do player (usa o teu script existente)
        if (playerController != null)
            playerController.enabled = false;

        // Liberta o rato
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Game Over: Canvas ou cena
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
                Debug.LogWarning("[Player] ⚠️ gameOverSceneName está vazio! Define o nome da cena no Inspector.");
        }
    }

    // ═══════════════════════════════════════════════════════
    // GETTER PÚBLICO (para HUD ou outros scripts)
    // ═══════════════════════════════════════════════════════
    public bool IsDead() => isDead;
    public float GetHealthPercent() => currentHealth / maxHealth;
}